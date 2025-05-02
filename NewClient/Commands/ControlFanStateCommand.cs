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
				if ( stateInput?.ToLower() == "on" || stateInput?.ToLower() == "off" )
				{
					bool isOn = stateInput.ToLower() == "on";
					await _environmentController.SetFanState(fanId, isOn);
				}
				else
				{
					Console.WriteLine("Invalid input. Please enter 'on' or 'off'.");
				}
			}
			else
			{
				Console.WriteLine("Invalid Fan Number.");
			}
		}
	}
}
