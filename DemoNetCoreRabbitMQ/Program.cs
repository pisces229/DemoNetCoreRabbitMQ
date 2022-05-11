using DemoNetCoreRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

Console.WriteLine(args);

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddConsole();
});
services.AddSingleton<IConnectionFactory, ConnectionFactory>(provider => 
{ 
    return new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "user",
        Password = "123"
    };
});
services.AddSingleton<RunnerQueueDeclare>();
services.AddSingleton<RunnerExchangeDeclare>();
var serviceProvider = services.BuildServiceProvider();
try
{
    //serviceProvider.GetRequiredService<RunnerQueueDeclare>().Run();
    //serviceProvider.GetRequiredService<RunnerExchangeDeclare>().Run();
}
finally
{
    serviceProvider.Dispose();
}