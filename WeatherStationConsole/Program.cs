// See https://aka.ms/new-console-template for more information

using Cirrus.Extensions;
using Cirrus.Models;
using Cirrus.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

static IHostBuilder CreateHostBuilder() =>
    Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((context, builder) =>
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("connections.json", optional: false, reloadOnChange: false)
                .Build();
        })
        .ConfigureServices((context, collection) =>
        {
            var apiKeys = context.Configuration.GetSection(nameof(CirrusConfig.ApiKeys)).Get<List<string>>();
            var applicationKey = context.Configuration.GetValue<string>(nameof(CirrusConfig.ApplicationKey));
            var macAddress = context.Configuration.GetValue<string?>(nameof(CirrusConfig.MacAddress));

            collection
                .Configure<CirrusConfig>(context.Configuration)
                .AddCirrusServices(cirrusConfig =>
                {
                    cirrusConfig.ApiKeys = apiKeys;
                    cirrusConfig.ApplicationKey = applicationKey;
                    cirrusConfig.MacAddress = macAddress;
                });
        });

static async Task WriteFileAsync(string dir, string file, string content)
{
    using (StreamWriter outputFile = new StreamWriter(Path.Combine(dir, file)) )
    {
        await outputFile.WriteAsync(content);
    }
}

var host = CreateHostBuilder().Build();
var service = host.Services.GetRequiredService<ICirrusRestWrapper>();

var now = DateTime.Today;
var workingTime = now.AddDays(1);

var maxDaysBack = 30;

for (int i = 0; i < maxDaysBack; i++)
{
    
    var results = await service.FetchDeviceDataAsJsonAsync(workingTime);

    var filename = $"output_{workingTime.AddDays(-1):yyyy-MM-dd}_.json";

    await WriteFileAsync(".", filename, results);

    workingTime = workingTime.AddDays(-1);
    await Task.Delay(2 * 1000);

}
