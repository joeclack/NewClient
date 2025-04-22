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
				try
				{
					double temperature = await _environmentController.GetSensorTemperature(sensorId);
					Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
				}
				catch ( Exception ex )
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Invalid Sensor Number.");
			}
		}
	}
}
