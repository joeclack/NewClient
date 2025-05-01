using NewClient.Commands;
using NewClient.Controllers;
using NewClient.UI;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using NewClient.Interfaces;
using NewClient.Factories;
using IHttpClientFactory = NewClient.Interfaces.IHttpClientFactory;

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

		var serviceProvider = new ServiceCollection()
			.AddSingleton<IConfiguration>(configuration)
			.AddSingleton<IHttpClientFactory, HttpClientFactory>()
			.AddLogging(builder =>
			{
				builder.AddFile(Path.Combine(logPath, "newclient-{Date}.txt"), LogLevel.Information);
				builder.SetMinimumLevel(LogLevel.Information);
			})
			.AddSingleton<EnvironmentController>()
			.AddSingleton<GetAllStatesCommand>()
			.AddSingleton<SetHeaterLevelCommand>()
			.AddSingleton<GetTemperatureCommand>()
			.AddSingleton<ControlFanStateCommand>()
			.AddSingleton<ResetEnvironmentCommand>()
			.AddSingleton<ControlSimulationCommand>()
			.AddSingleton<MenuInvoker>()
			.BuildServiceProvider();

		var menu = serviceProvider.GetRequiredService<MenuInvoker>();
		var environment = serviceProvider.GetRequiredService<EnvironmentController>();

		menu.SetCommand("1", serviceProvider.GetRequiredService<ControlFanStateCommand>());
		menu.SetCommand("2", serviceProvider.GetRequiredService<SetHeaterLevelCommand>());
		menu.SetCommand("3", serviceProvider.GetRequiredService<GetTemperatureCommand>());
		menu.SetCommand("4", serviceProvider.GetRequiredService<GetAllStatesCommand>());
		menu.SetCommand("5", serviceProvider.GetRequiredService<ControlSimulationCommand>());
		menu.SetCommand("6", serviceProvider.GetRequiredService<ResetEnvironmentCommand>());

		await menu.RunMenuAsync();
	}
}

