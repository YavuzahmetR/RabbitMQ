using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System;

var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};


using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
//await channel.ExchangeDeclareAsync("logs-fanout", ExchangeType.Direct, durable: true); write or do not doesn't matter 


var queueName = "direct-queue-Critical";
await channel.BasicQosAsync(0, 1, false); // 0 : size could be anything , 1: once at a time send to subscriber , false: do not split messages evenly.
//false wont require new instance of rmq, it pulls from cache



var consumer = new AsyncEventingBasicConsumer(channel);

await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

//autoAck : instantly delete / See the result first then delete
Console.WriteLine("Logs Listening...");
int messageCounter = 0;
consumer.ReceivedAsync += async (object sender, BasicDeliverEventArgs @event) =>
{
    var message = Encoding.UTF8.GetString(@event.Body.ToArray());
    //await Task.Delay(1000);
    messageCounter++;
    Console.WriteLine("Gelen Mesaj : " + message +" "+ messageCounter);
    // File.AppendAllText("log-critical.txt", message+ "\n"); writing text file
    await channel.BasicAckAsync(deliveryTag: @event.DeliveryTag, multiple: false);
};

Console.ReadLine();

