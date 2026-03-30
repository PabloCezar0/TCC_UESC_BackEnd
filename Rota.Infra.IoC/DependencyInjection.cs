using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rota.Application.Interfaces;
using Rota.Application.Mappings;
using Rota.Application.Services;
using Rota.Domain.Interfaces;
using Rota.Infra.Data;
using Rota.Infra.Data.Repositories;
using Rota.Infra.Data.Services;
using Rota.Infra.Data.Interceptors;
using Rota.Infrastructure.Persistence; 

namespace Rota.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddScoped<AuditingInterceptor>();

            services.AddScoped<FormulaEvaluatorService>();

            services.AddScoped<CommissionRuleService>();

            services.AddScoped<ICommissionRuleRepository, CommissionRuleRepository>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string is null or empty");
            }

            
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var auditableInterceptor = sp.GetRequiredService<AuditingInterceptor>();

                options.UseMySql(
                    connectionString, 
                    ServerVersion.AutoDetect(connectionString),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                )
                .AddInterceptors(auditableInterceptor);
            });

           
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();
            
           
            services.AddScoped<ITransactionRepository, TransactionRepository>(); 
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            
            
            services.AddScoped<IAuthenticate, AuthenticateService>();
            services.AddScoped<CreateAdmin>();
            
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAgencyService, AgencyService>();

            services.AddAutoMapper(typeof(DomainToDTOMappingProfile).Assembly);

            return services;
        }
    }
}