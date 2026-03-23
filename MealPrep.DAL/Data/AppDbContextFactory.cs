using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace MealPrep.DAL.Data;

/// <summary>
/// Allows `dotnet ef migrations` to create AppDbContext at design time
/// without needing to run the full web host.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        const string connStr =
            "Host=localhost;Port=5432;Database=mealprep_db;Username=postgres;Password=12345;";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
        var dataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            dataSource,
            npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}
