namespace HddFancontrol.ConsoleApp.Services.Interfaces;

public interface IHddTempService
{
    Task<IEnumerable<int>> GetAllHddTempsAsync();
    Task<int?> GetHddTempAsync(string diskPath);
}