using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Services;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class GlobalSettingsTests : TestBase
{
    [TestMethod]
    public async Task TestAdminManageSettings()
    {
        // 1. Login as admin
        await LoginAsAdmin();

        // 2. Access Global Settings Index
        var settingsResponse = await Http.GetAsync("/GlobalSettings/Index");
        settingsResponse.EnsureSuccessStatusCode();
        var settingsHtml = await settingsResponse.Content.ReadAsStringAsync();
        Assert.Contains("Global Settings", settingsHtml);
        Assert.Contains(SettingsMap.AllowUserAdjustNickname, settingsHtml);

        // 3. Change setting via UI
        var editResponse = await PostForm("/GlobalSettings/Edit", new Dictionary<string, string>
        {
            { "Key", SettingsMap.AllowUserAdjustNickname },
            { "Value", "False" }
        }, tokenUrl: "/GlobalSettings/Index");
        Assert.AreEqual(HttpStatusCode.Found, editResponse.StatusCode);

        // 4. Verify setting changed in DB
        using (var scope = Server!.Services.CreateScope())
        {
            var settingsService = scope.ServiceProvider.GetRequiredService<GlobalSettingsService>();
            var value = await settingsService.GetBoolSettingAsync(SettingsMap.AllowUserAdjustNickname);
            Assert.IsFalse(value);
        }
    }

    [TestMethod]
    public async Task TestGlobalSettingsServiceValidation()
    {
        using var scope = Server!.Services.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<GlobalSettingsService>();

        // Test non-defined setting
        try
        {
            await settingsService.UpdateSettingAsync("InvalidKey", "Value");
            Assert.Fail("Should have thrown InvalidOperationException");
        }
        catch (InvalidOperationException) { }

        // Test Bool validation
        try
        {
            await settingsService.UpdateSettingAsync(SettingsMap.AllowUserAdjustNickname, "NotABool");
            Assert.Fail("Should have thrown InvalidOperationException");
        }
        catch (InvalidOperationException) { }
    }
}
