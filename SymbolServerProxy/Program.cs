using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using static SymbolServerProxy.SymbolServerProxy;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient(SymbolServerCfg, (serviceProvider, httpClient) =>
        {
            //var settings = serviceProvider.GetRequiredService<IOptions<SomeThink>>().Value;
            httpClient.BaseAddress = new Uri("https://artifacts.dev.azure.com");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        });
    })
    .Build();

host.Run();
