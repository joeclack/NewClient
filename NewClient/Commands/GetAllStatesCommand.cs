using NewClient.Controllers;
using NewClient.Interfaces;
using NewClient.Models;
using System.Text.Json;

namespace NewClient.Commands
{
	public class GetAllStatesCommand : ICommand
	{
		private readonly EnvironmentController _environmentController;

		public GetAllStatesCommand(EnvironmentController environmentController)
		{
			_environmentController = environmentController;
		}

		public async Task ExecuteAsync()
		{
			await _environmentController.GetAllStates();
		}
	}
}
