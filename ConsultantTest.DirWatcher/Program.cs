using ConsultantTest.DirWatcher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ConsultantTest.DirWatcher.Abstractions;
using Microsoft.Extensions.Logging;


if (args.Length <= 1)
{
	WriteHelp();
	Console.ReadKey();
	return;
}

if (!args[0].StartsWith("src=") && !args[1].StartsWith("src=") 
    || !args[0].StartsWith("res=") && !args[1].StartsWith("res="))
{
	WriteHelp();
	Console.ReadKey();
	return;
}

if(args[0].Substring(args[0].IndexOf("=")) == args[1].Substring(args[1].IndexOf("=")))
{
	Console.WriteLine("Исходный путь и путь результата не должны совпадать");
	WriteHelp();
	Console.ReadKey();
	return;
}

var host = new HostBuilder()
	.ConfigureHostConfiguration(configHost =>
	{
		configHost.AddCommandLine(args);
	})
	.ConfigureServices((hostContext, services) => {
		services.AddHostedService<FileWatcher>();
		services.AddLogging(b =>
		{
			b.AddConsole();
			b.SetMinimumLevel(LogLevel.Information);
		});
		services.AddSingleton<IFileLettersCounter, FileLettersCounter>();
	})
	.UseConsoleLifetime()
	.Build();

host.Run();


void WriteHelp()
{
	Console.WriteLine("Укажите исходную папку в параметре src");
	Console.WriteLine("Укажите папку результатов в параметре res");
	Console.WriteLine("Например, ConsultantTest.DirWatchAndCount src=D:\\test res=D:\\test\\res");
}