using RabbitMQ.Client;
using System.Text;



var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();


await channel.ExchangeDeclareAsync("logs-fanout", ExchangeType.Fanout, durable: true); //durable : true means exchange will not be deleted after running the program.


foreach (var x in Enumerable.Range(1, 50))
{
    string message = $"log {x}";
    var messageBody = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync(exchange: "logs-fanout", routingKey: "", body: messageBody);

    Console.WriteLine($"Mesaj Gönderildi {message}");
}

Console.ReadLine();
