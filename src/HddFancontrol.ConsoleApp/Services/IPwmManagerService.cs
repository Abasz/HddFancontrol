namespace HddFancontrol.ConsoleApp.Services.Interfaces;

public interface IPwmManagerService
{
    Task SetPwmControllerProfile(string name, PwmControllerProfiles profile);
    Task UpdatePwmFileAsync(int pwm, string name);
    Task<IEnumerable<int>> GetCurrentPwmsAsync();
    IEnumerable<PwmDto> CalculatePwms(int hddTemp);
}