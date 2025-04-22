using NewClient.Controllers;
using NewClient.Interfaces;

namespace NewClient.Commands
{
	public class ControlSimulationCommand : ICommand
	{
		private readonly EnvironmentController _environmentController;

		public ControlSimulationCommand(EnvironmentController environmentController)
		{
			_environmentController = environmentController;
		}

		public async Task ExecuteAsync()
		{
			Console.WriteLine("Starting temperature control algorithm...");
			Console.Write("Provide a final Temp Value: ");
			double currentTemperature = await _environmentController.GetAverageTemperature();
			while ( true )
			{
				// Phase 1: Gradually increase to 20°C over 30 seconds
				currentTemperature = await _environmentController.AdjustTemperature(currentTemperature, 20.0, 30);

				// Phase 2: Rapidly cool to 16°C
				currentTemperature = await _environmentController.AdjustTemperature(currentTemperature, 16.0, 10);

				// Phase 3: Hold at 16°C for 10 seconds
				currentTemperature = await _environmentController.HoldTemperature(currentTemperature, 16.0, 10);

				// Phase 4: Gradually return to 18°C and maintain
				currentTemperature = await _environmentController.AdjustTemperature(currentTemperature, 18.0, 20);
				currentTemperature = await _environmentController.HoldTemperature(currentTemperature, 18.0, int.MaxValue); // Maintain until exit
			}
		}
	}
}
