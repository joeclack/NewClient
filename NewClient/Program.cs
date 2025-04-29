using NewClient.Commands;
using NewClient.Controllers;
using NewClient.UI;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.Json;
using System.IO;

static class Program
{
	static async Task Main(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.Build();

		var logPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			"NewClient",
			"Logs"
		);
		Directory.CreateDirectory(logPath);

		var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddFile(Path.Combine(logPath, "newclient-{Date}.txt"), LogLevel.Information);
			builder.SetMinimumLevel(LogLevel.Information);
		});
		var logger = loggerFactory.CreateLogger<EnvironmentController>();


		logger.LogInformation("Application started at {Time}", DateTime.Now);

		var baseAddress = configuration.GetValue<string>("Environment:BaseAddress");

		HttpClient client = new()
		{
			BaseAddress = new Uri(baseAddress),
		};
		client.DefaultRequestHeaders.Add("X-Api-Key", "API_KEY_CLIENT_3");

		EnvironmentController environment = new(client, logger, configuration);

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

