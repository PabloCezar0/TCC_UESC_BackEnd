using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rota.Application.Services;

namespace Rota.Application.Jobs
{
    public class MonthlyTransactionImportJob : IHostedService, IDisposable
    {
        private readonly ILogger<MonthlyTransactionImportJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public MonthlyTransactionImportJob(ILogger<MonthlyTransactionImportJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de Agendamento de Importação de transações está iniciando.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            var today = DateTime.UtcNow;
               
            if (today.Day == 26)
            {
                _logger.LogInformation("Hoje é dia 26. Disparando a rotina de importação de comissões...");


                using (var scope = _serviceProvider.CreateScope())
                {
                    var transactionInsertService = scope.ServiceProvider.GetRequiredService<TransactionInsertService>();
                    var commissionCalculationService = scope.ServiceProvider.GetRequiredService<CommissionCalculationService>(); 

                    try
                    {
                      
                        _logger.LogInformation("Iniciando importação de transações...");
                        int savedCount = await transactionInsertService.ImportAndSaveMonthlyCycleAsync();
                        _logger.LogInformation("Importação de transações concluída. {SavedCount} novas transações salvas.", savedCount);

                        
                        _logger.LogInformation("Iniciando o pré-cálculo das comissões mensais..."); 
                        await commissionCalculationService.CalculateAndSaveForPeriod(today.Year, today.Month); 
                        _logger.LogInformation("Pré-cálculo das comissões mensais concluído."); 
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ocorreu um erro na execução da rotina agendada.");
                    }
                }
            }
            else
            {
                _logger.LogInformation("Verificação diária: Hoje não é dia 26. Nenhuma importação agendada será executada.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de Agendamento de Importação de Comissões está parando.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}