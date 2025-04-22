using NewClient.Commands;
using NewClient.Controllers;
using NewClient.UI;
using System.Net.NetworkInformation;
static class Program
{
	static async Task Main(string[] args)
	{
		var baseAddress = "https://localhost:44351/api";

		HttpClient client = new()
		{
			BaseAddress = new Uri(baseAddress),
		};
		client.DefaultRequestHeaders.Add("X-Api-Key", "API_KEY_CLIENT_3");

		EnvironmentController       environment = new (client);

		GetAllStatesCommand         getAllStates          = new(environment);
		SetHeaterLevelCommand       setHeaterLevel        = new(environment);
		GetTemperatureCommand       getTemperature        = new(environment);
		ControlFanStateCommand      controlFanState       = new(environment);
		ResetEnvironmentCommand     resetEnvironment      = new(environment);
		ControlSimulationCommand    controlSimulation     = new(environment);

		MenuInvoker menu = new();

		menu.SetCommand("1", controlFanState);
		menu.SetCommand("2", setHeaterLevel);
		menu.SetCommand("3", getTemperature);
		menu.SetCommand("4", getAllStates);
		menu.SetCommand("5", controlSimulation);
		menu.SetCommand("6", resetEnvironment);

		await menu.RunMenuAsync();
	}
}

