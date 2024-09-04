namespace HddFancontrol.ConsoleApp.Services.Interfaces;

public interface IPwmManagerService
{
    Task UpdatePwmFileAsync(int pwm, string name);
    Task<IEnumerable<int>> GetCurrentPwmsAsync();
    IEnumerable<PwmDto> CalculatePwms(int hddTemp);
}