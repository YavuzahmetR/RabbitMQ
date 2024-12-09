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
//await channel.ExchangeDeclareAsync("logs-fanout", ExchangeType.Fanout, durable: true); write or do not doesn't matter 


//var randomQueueName = "logs-database-save-queue";
//await channel.QueueDeclareAsync(randomQueueName, true, false, false); permanent queue - will be deleted after program stops working. - created by consumer

var queueResult = await channel.QueueDeclareAsync();//temporary queue - will be deleted after program stops working. - created by consumer
var randomQueueName = queueResult.QueueName;

await channel.QueueBindAsync(randomQueueName, "logs-fanout", "", null);

await channel.BasicQosAsync(0, 1, false); // 0 : size could be anything , 1: once at a time send to subscriber , false: do not split messages evenly.
//false wont require new instance of rmq, it pulls from cache

Console.WriteLine("Logs Listening...");

var consumer = new AsyncEventingBasicConsumer(channel);
await channel.BasicConsumeAsync(queue: randomQueueName, autoAck: false, consumer: consumer); //autoAck : instantly delete / See the result first then delete

consumer.ReceivedAsync += async (object sender, BasicDeliverEventArgs @event) =>
{
    var message = Encoding.UTF8.GetString(@event.Body.ToArray());
    await Task.Delay(1000);
    Console.WriteLine("Gelen Mesaj : " + message);
    await  channel.BasicAckAsync(deliveryTag: @event.DeliveryTag, multiple: true);
};

Console.ReadLine();

