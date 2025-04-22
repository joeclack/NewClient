using NewClient.Controllers;
using NewClient.Interfaces;
using System.Text.Json;
namespace NewClient.Commands
{
	public class ResetEnvironmentCommand : ICommand
	{
		private readonly EnvironmentController _environmentController;

		public ResetEnvironmentCommand(EnvironmentController environmentController)
		{
			_environmentController = environmentController;
		}

		public async Task ExecuteAsync()
		{
			Console.WriteLine("Resetting client state...");

			try
			{
				// Send a POST request to the reset endpoint
				var response = await _environmentController.ResetEnvironment();

				if ( response.IsSuccessStatusCode )
				{
					Console.WriteLine("Client state has been successfully reset.");
					Console.WriteLine("Fetching the state of all devices...");

					await _environmentController.GetAllStates();
				}
				else
				{
					Console.WriteLine($"Failed to reset client state: {response.ReasonPhrase}");
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine($"Error while resetting client state: {ex.Message}");
			}
		}
	}
}
