using System.Diagnostics.CodeAnalysis;
using Aiursoft.DbTools;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Entities;

[ExcludeFromCodeCoverage]
public abstract class EmployeeCenterDbContext(DbContextOptions options) : IdentityDbContext<User>(options), ICanMigrate
{
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<SshKey> SshKeys => Set<SshKey>();
    public DbSet<Password> Passwords => Set<Password>();
    public DbSet<PasswordShare> PasswordShares => Set<PasswordShare>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentComment> IncidentComments => Set<IncidentComment>();
    public DbSet<BankCardChangeLog> BankCardChangeLogs => Set<BankCardChangeLog>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();
    public DbSet<OnboardingTask> OnboardingTasks => Set<OnboardingTask>();
    public DbSet<OnboardingTaskLog> OnboardingTaskLogs => Set<OnboardingTaskLog>();
    public DbSet<CompanyEntity> CompanyEntities => Set<CompanyEntity>();
    public DbSet<CompanyEntityLog> CompanyEntityLogs => Set<CompanyEntityLog>();
    public DbSet<FinanceAccount> FinanceAccounts => Set<FinanceAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<AssetModel> AssetModels => Set<AssetModel>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetHistory> AssetHistories => Set<AssetHistory>();
    public DbSet<IntangibleAsset> IntangibleAssets => Set<IntangibleAsset>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractFolder> ContractFolders => Set<ContractFolder>();
    public DbSet<ContractOcrResult> ContractOcrResults => Set<ContractOcrResult>();
    public DbSet<PromotionHistory> PromotionHistories => Set<PromotionHistory>();
    public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();
    public DbSet<WeeklyReportRequirement> WeeklyReportRequirements => Set<WeeklyReportRequirement>();
    public DbSet<Notepad> Notepads => Set<Notepad>();
    public DbSet<Blueprint> Blueprints => Set<Blueprint>();
    public DbSet<Requirement> Requirements => Set<Requirement>();
    public DbSet<RequirementComment> RequirementComments => Set<RequirementComment>();
    public DbSet<DnsProvider> DnsProviders => Set<DnsProvider>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Server> Servers => Set<Server>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<CustomerRelationship> CustomerRelationships => Set<CustomerRelationship>();
    public DbSet<MarketChannel> MarketChannels => Set<MarketChannel>();

    public DbSet<SignalQuestion> SignalQuestions => Set<SignalQuestion>();
    public DbSet<SignalQuestionnaire> SignalQuestionnaires => Set<SignalQuestionnaire>();
    public DbSet<SignalQuestionnaireQuestion> SignalQuestionnaireQuestions => Set<SignalQuestionnaireQuestion>();
    public DbSet<SignalResponse> SignalResponses => Set<SignalResponse>();
    public DbSet<SignalQuestionResponse> SignalQuestionResponses => Set<SignalQuestionResponse>();

    public DbSet<AdjustedHoliday> AdjustedHolidays => Set<AdjustedHoliday>();
    public DbSet<Reimbursement> Reimbursements => Set<Reimbursement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ContractOcrResult>()
            .HasIndex(r => r.ContractId)
            .IsUnique();

        builder.Entity<ContractFolder>()
            .HasIndex(f => new { f.ParentFolderId, f.Name })
            .IsUnique();
    }

    public virtual Task MigrateAsync(CancellationToken cancellationToken) =>
        Database.MigrateAsync(cancellationToken);

    public virtual Task<bool> CanConnectAsync() =>
        Database.CanConnectAsync();
}
