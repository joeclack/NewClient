using System.Threading.Tasks;

namespace NewClient.Interfaces
{
	public interface IDevice
	{
		int Id { get; }
		Task<DeviceStateResult> GetState();
		Task SetState(bool state);
		Task<int> GetLevel();
		Task SetLevel(int level);
		Task<double> GetTemperature();
	}

	public class DeviceStateResult
	{
		public bool HasError { get; set; }
		public string? ErrorMessage { get; set; }
		public bool IsOn { get; set; }
		public double? Temperature { get; set; }
	}
}
