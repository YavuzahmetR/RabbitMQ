using RabbitMQ.Client;
using System.Text;



var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync("header-exchange", ExchangeType.Headers, durable: true); //durable : true means exchange will not be deleted after running the program.

Dictionary<string,object> headers = new Dictionary<string, object>();
headers.Add("format", "pdf");
headers.Add("shape", "triangle");

BasicProperties basicProperties = new BasicProperties();
basicProperties.Headers = headers;

var message = Encoding.UTF8.GetBytes("Header Mesajım");

await channel.BasicPublishAsync(exchange:"header-exchange",routingKey:string.Empty,
    mandatory:false,basicProperties:basicProperties,body:message);
Console.WriteLine("selam");

Console.ReadLine();




