using NewClient.Controllers;
using NewClient.Interfaces;
namespace NewClient.Commands
{
	public class GetTemperatureCommand : ICommand
	{
		private readonly EnvironmentController _environmentController;

		public GetTemperatureCommand(EnvironmentController environmentController)
		{
			_environmentController = environmentController;
		}

		public async Task ExecuteAsync()
		{
			Console.Write("Enter Sensor Number: ");
			if ( int.TryParse(Console.ReadLine(), out int sensorId) )
			{
				double temperature = await _environmentController.GetSensorTemperature(sensorId);
			}
			else
			{
				Console.WriteLine("Invalid Sensor Number.");
			}
		}
	}
}
