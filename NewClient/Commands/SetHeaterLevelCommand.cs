using NewClient.Controllers;
using NewClient.Interfaces;

namespace NewClient.Commands
{
	public class SetHeaterLevelCommand : ICommand
	{
		private readonly EnvironmentController _environmentController;

		public SetHeaterLevelCommand(EnvironmentController environmentController)
		{
			_environmentController = environmentController;
		}

		public async Task ExecuteAsync()
		{
			Console.Write("Enter Heater Number: ");
			if ( int.TryParse(Console.ReadLine(), out int heaterId) )
			{
				Console.Write("Set Heater Level (0-5): ");
				if ( int.TryParse(Console.ReadLine(), out int level) && level >= 0 && level <= 5 )
				{
					try
					{
						await _environmentController.SetHeaterLevel(heaterId, level);
						Console.WriteLine($"Heater {heaterId} level set to {level}.");
					}
					catch ( Exception ex )
					{
						Console.WriteLine($"Error: {ex.Message}");
					}
				}
				else
				{
					Console.WriteLine("Invalid Heater Level. Please enter a value between 0 and 5.");
				}
			}
			else
			{
				Console.WriteLine("Invalid Heater Number.");
			}
		}
	}
}
