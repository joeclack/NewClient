using NewClient.Interfaces;

namespace NewClient.UI
{
	// command pattern implementation
	// keeps the menu logic separate from the actual commands
	public class MenuInvoker
	{
		private readonly Dictionary<string, ICommand> _commands = [];

		// maps menu options to their commands
		public void SetCommand(string option, ICommand command)
		{
			_commands[option] = command;
		}

		// basic menu loop - keeps running until program ends
		// might want to add an exit option at some point
		public async Task RunMenuAsync()
		{
			while(true)
			{
				Console.WriteLine("\n=== Simulation Control Menu ===");
				Console.WriteLine(" 1. Control Fan");
				Console.WriteLine(" 2. Control Heater");
				Console.WriteLine(" 3. Read Temperature");
				Console.WriteLine(" 4. Display State of All Devices");
				Console.WriteLine(" 5. Control Simulation");
				Console.WriteLine(" 6. Reset Simulation");
				Console.WriteLine(" 7. Exit");
				Console.WriteLine("===============================");
				Console.Write("Select an option: ");

				var input = Console.ReadLine();

				if(_commands.TryGetValue(input, out var command))
				{
					await command.ExecuteAsync();
				}
				if(input == "7") {
					break;
				}
			}
		}
	}
}
