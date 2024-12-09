using RabbitMQ.Client;
using System.Text;



var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync("logs-topic", ExchangeType.Topic, durable: true); //durable : true means exchange will not be deleted after running the program.

var logNames = Enum.GetNames(typeof(LogNames)).ToList();

Random rnd = new Random();
foreach (var x in Enumerable.Range(1, 50))
{
    
    LogNames log1 = (LogNames)rnd.Next(1, 5);
    LogNames log2 = (LogNames)rnd.Next(1, 5);
    LogNames log3 = (LogNames)rnd.Next(1, 5);

    string message = $"log-type: {log1}-{log2}-{log3}";

    var messageBody = Encoding.UTF8.GetBytes(message);

    var routeKey = $"{log1}.{log2}.{log3}"; //mandatory spelling syntax 

    await channel.BasicPublishAsync(exchange: "logs-topic", routingKey: routeKey, body: messageBody);

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

//foreach (var logName in logNames)
//{
//    Random rnd = new Random();

//    var queueName = $"direct-queue-{logName}";

//    await channel.QueueDeclareAsync(queueName, true, false, false);
//    await channel.QueueBindAsync(queueName, "logs-direct", routeKey, null);
//}
