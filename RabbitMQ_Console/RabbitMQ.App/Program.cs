using RabbitMQ.Client;
using System.Text;



var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync(); 


await channel.QueueDeclareAsync("hello-queue", durable: true, exclusive: false, autoDelete: false);

Enumerable.Range(1, 50).ToList().ForEach(async x =>
{
    string message = "hello world";
    var messageBody = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello-queue", body: messageBody);

    Console.WriteLine("Mesaj Gönderildi");
});

Console.ReadLine();
