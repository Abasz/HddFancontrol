using System.Collections.Generic;
using System.Threading.Tasks;

namespace HddFancontrol.ConsoleApp.Services.Interfaces
{
    public interface IHddTempService
    {
        Task<IEnumerable<int>> GetAllHddTempsAsync();
        Task<int?> GetHddTempAsync(string diskPath);
    }
}