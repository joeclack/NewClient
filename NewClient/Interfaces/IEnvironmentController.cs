using System.Threading.Tasks;
using System.Net.Http;
using NewClient.Interfaces;

namespace NewClient.Interfaces
{
    public interface IEnvironmentController
    {
        Task SetFanState(int fanId, bool isOn);
        Task SetHeaterLevel(int heaterId, int level);
        Task<DeviceStateResult> GetFanState(int id);
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