using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Application.Services;
using Monivise.Infrastructure.Auth;
using Monivise.Infrastructure.Data;
using Monivise.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration config)
        {
            // EF Core + PostgreSQL
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBudgetCycleRepository, BudgetCycleRepository>();
            services.AddScoped<IBucketRepository, BucketRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IIntakeProfileRepository, IntakeProfileRepository>();
            services.AddScoped<IGoalRepository, GoalRepository>();


            // Financial services
            services.AddScoped<IFinancialCalculationService, FinancialCalculationService>();
            services.AddScoped<IAllocationRecommendationService, AllocationRecommendationService>();
            services.AddScoped<ISurplusSweepService, SurplusSweepService>();

            // Auth
            services.AddSingleton<JwtTokenService>();
            return services;
        }
    }
}
