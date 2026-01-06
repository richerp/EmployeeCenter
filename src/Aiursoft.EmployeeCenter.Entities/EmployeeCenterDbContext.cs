using Aiursoft.DbTools;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Entities;

public abstract class EmployeeCenterDbContext(DbContextOptions options) : IdentityDbContext<User>(options), ICanMigrate
{
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<SshKey> SshKeys => Set<SshKey>();
    public DbSet<Password> Passwords => Set<Password>();
    public DbSet<PasswordShare> PasswordShares => Set<PasswordShare>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentComment> IncidentComments => Set<IncidentComment>();
    public DbSet<BankCardChangeLog> BankCardChangeLogs => Set<BankCardChangeLog>();

    public virtual Task MigrateAsync(CancellationToken cancellationToken) =>
        Database.MigrateAsync(cancellationToken);

    public virtual Task<bool> CanConnectAsync() =>
        Database.CanConnectAsync();
}
