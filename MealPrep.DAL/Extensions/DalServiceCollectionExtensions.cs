using MealPrep.DAL.Data;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPrep.DAL.Extensions;

public static class DalServiceCollectionExtensions
{
    public static IServiceCollection AddDalServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection"
    )
    {
        var connectionString =
            configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' not found."
            );

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            );
        });

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
