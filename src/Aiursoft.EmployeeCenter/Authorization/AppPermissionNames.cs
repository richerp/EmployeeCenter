namespace Aiursoft.EmployeeCenter.Authorization;

/// <summary>
/// Defines all permission keys as constants. This is the single source of truth.
/// </summary>
public static class AppPermissionNames
{
    // User Management
    public const string CanReadUsers = nameof(CanReadUsers);
    public const string CanDeleteUsers = nameof(CanDeleteUsers);
    public const string CanAddUsers = nameof(CanAddUsers);
    public const string CanEditAndViewDetailsOfUsers = nameof(CanEditAndViewDetailsOfUsers);
    public const string CanAssignRoleToUser = nameof(CanAssignRoleToUser);
    public const string CanViewUserOperationLog = nameof(CanViewUserOperationLog);

    // Role Management
    public const string CanReadRoles = nameof(CanReadRoles);
    public const string CanDeleteRoles = nameof(CanDeleteRoles);
    public const string CanAddRoles = nameof(CanAddRoles);
    public const string CanEditRoles = nameof(CanEditRoles);

    // System Management
    public const string CanViewSystemContext = nameof(CanViewSystemContext);
    public const string CanRebootThisApp = nameof(CanRebootThisApp);

    // Payroll Management
    public const string CanManagePayroll = nameof(CanManagePayroll);

    // Password Management
    public const string CanAddGlobalPassword = nameof(CanAddGlobalPassword);
    public const string CanManageAnyPassword = nameof(CanManageAnyPassword);

    // SSH Key Management
    public const string CanManageSshKeys = nameof(CanManageSshKeys);

    // Report Line Management
    public const string CanViewReportLine = nameof(CanViewReportLine);

    // Leave Management
    public const string CanApproveAnyLeave = nameof(CanApproveAnyLeave);

    // Onboarding Task Management
    public const string CanManageOnboarding = nameof(CanManageOnboarding);

    // Certificate Printing
    public const string CanPrintCertificates = nameof(CanPrintCertificates);
}
