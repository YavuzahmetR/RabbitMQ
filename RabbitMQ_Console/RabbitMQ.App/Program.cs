using RabbitMQ.Client;
using System.Text;



var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();


//Direct Exchange Needs Route for specifying which queue will it bind to itself for filtering.
await channel.ExchangeDeclareAsync("logs-direct", ExchangeType.Direct, durable: true); //durable : true means exchange will not be deleted after running the program.


var logNames = Enum.GetNames(typeof(LogNames)).ToList();

foreach (var logName in logNames)
{
    var routeKey = $"route-{logName}";
    var queueName = $"direct-queue-{logName}";

    await channel.QueueDeclareAsync(queueName, true, false, false);
    await channel.QueueBindAsync(queueName, "logs-direct", routeKey, null);
}




foreach (var x in Enumerable.Range(1, 50))
{
    LogNames log = (LogNames)new Random().Next(1, 5);
    string message = $"log-type: {log}";
    var messageBody = Encoding.UTF8.GetBytes(message);
    var routeKey = $"route-{log}";
    await channel.BasicPublishAsync(exchange: "logs-direct", routingKey: routeKey, body: messageBody);

    Console.WriteLine($"Log gönderilmiştir : {message}");
}

Console.ReadLine();

public enum LogNames
{
    Critical = 1,
    Error = 2,
    Warning = 3,
    Info = 4
}