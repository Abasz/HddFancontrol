using System.Collections.Generic;
using System.Threading.Tasks;

namespace HddFancontrol.ConsoleApp.Services.Interfaces
{
    public interface IPwmManagerService
    {
        Task UpdatePwmFileAsync(int pwm, string name);
        Task<IEnumerable<int>> GetCurrentPwmsAsync();
        IEnumerable<int> CalculatePwms(int hddTemp);
    }
}