using System.Threading.Tasks;

namespace NewClient.Controllers
{
    public interface IEnvironmentController
    {
        Task SetFanState(int fanId, bool isOn);
        Task SetHeaterLevel(int heaterId, int level);
        Task<bool> GetFanState(int id);
        Task<int> GetHeaterLevel(int id);
        Task<double> GetSensorTemperature(int sensorId);
        Task<double> GetAverageTemperature();
        Task SetAllHeaters(int level);
        Task SetAllFans(bool state);
        Task GetAllStates();
        Task<double> HoldTemperature(double currentTemperature, double targetTemperature, int durationSeconds);
        Task<double> AdjustTemperature(double currentTemperature, double targetTemperature, int durationSeconds);
        Task<HttpResponseMessage> ResetEnvironment();
    }
} 