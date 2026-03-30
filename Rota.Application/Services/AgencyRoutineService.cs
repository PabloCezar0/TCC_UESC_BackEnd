using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Rota.Application.Services
{
    public class AgencyRoutineService : BackgroundService

    {
        private readonly IServiceProvider _serviceProvider;

        public AgencyRoutineService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope(); 
                var AgencyInsertService = scope.ServiceProvider.GetRequiredService<AgencyInsertService>(); 
                try
                {
                    var saved = await AgencyInsertService.ImportAndSaveAsync();
                    Console.WriteLine($"{DateTime.UtcNow}: {saved} agencies imported and saved."); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.UtcNow}: Error importing agencies - {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromDays(30), stoppingToken); 
            }
        }


    }
}