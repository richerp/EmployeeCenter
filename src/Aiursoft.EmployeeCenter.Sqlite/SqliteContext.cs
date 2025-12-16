using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;


namespace Aiursoft.EmployeeCenter.Sqlite;

public class SqliteContext(DbContextOptions<SqliteContext> options) : TemplateDbContext(options)
{
    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
