using NewClient.Controllers;
using NewClient.Interfaces;

namespace NewClient.Commands
{
	public class ControlFanStateCommand : ICommand
	{
		private readonly EnvironmentController _environmentController;

		public ControlFanStateCommand(EnvironmentController environmentController)
		{
			_environmentController = environmentController;
		}

		public async Task ExecuteAsync()
		{
			Console.Write("Enter Fan Number: ");
			if ( int.TryParse(Console.ReadLine(), out int fanId) )
			{
				Console.Write("Turn Fan On or Off? (on/off): ");
				var stateInput = Console.ReadLine();
				bool isOn = stateInput?.ToLower() == "on";

				try
				{
					await _environmentController.SetFanState(fanId, isOn);
					Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
				}
				catch ( Exception ex )
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Invalid Fan Number.");
			}
		}
	}
}
