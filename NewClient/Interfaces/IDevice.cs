using System.Threading.Tasks;

namespace NewClient.Interfaces
{
	public interface IDevice
	{
		int Id { get; }
		Task<bool> GetState();
		Task SetState(bool state);
		Task<int> GetLevel();
		Task SetLevel(int level);
		Task<double> GetTemperature();
	}
}
