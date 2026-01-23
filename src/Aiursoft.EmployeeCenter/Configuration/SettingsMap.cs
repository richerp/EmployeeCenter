using Aiursoft.EmployeeCenter.Models;

namespace Aiursoft.EmployeeCenter.Configuration;

public class SettingsMap
{
    public const string AllowUserAdjustNickname = "Allow_User_Adjust_Nickname";
    public const string AnnualLeavePerYear = "Annual_Leave_Per_Year";
    public const string DefaultPayrollCurrency = "Default_Payroll_Currency";

    public class FakeLocalizer
    {
        public string this[string name] => name;
    }

    private static readonly FakeLocalizer Localizer = new();

    public static readonly List<GlobalSettingDefinition> Definitions = new()
    {
        new GlobalSettingDefinition
        {
            Key = AllowUserAdjustNickname,
            Name = Localizer["Allow User Adjust Nickname"],
            Description = Localizer["Allow users to adjust their nickname in the profile management page."],
            Type = SettingType.Bool,
            DefaultValue = "True"
        },
        new GlobalSettingDefinition
        {
            Key = AnnualLeavePerYear,
            Name = Localizer["Annual Leave Per Year"],
            Description = Localizer["The number of paid annual leave days allocated to each employee every year."],
            Type = SettingType.Number,
            DefaultValue = "12"
        },
        new GlobalSettingDefinition
        {
            Key = DefaultPayrollCurrency,
            Name = Localizer["Default Payroll Currency"],
            Description = Localizer["The default currency to use when issuing a new payroll."],
            Type = SettingType.Choice,
            DefaultValue = "CNY",
            ChoiceOptions = new Dictionary<string, string>
            {
                { "CNY", "人民币 (CNY)" },
                { "JPY", "日元 (JPY)" },
                { "HKD", "港币 (HKD)" },
                { "USD", "美元 (USD)" }
            }
        }
    };
}
