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
			try
			{
				Console.Write("Enter Sensor Number: ");
				if ( int.TryParse(Console.ReadLine(), out int sensorId) )
				{
					if ( sensorId < 1 || sensorId > 3 )
					{
						throw new ArgumentOutOfRangeException(nameof(sensorId), "Sensor ID must be between 1 and 3");
					}
					double temperature = await _environmentController.GetSensorTemperature(sensorId);
				}
			} catch 
			{
				Console.WriteLine("Invalid Sensor Number.");
			}
		}
	}
}
