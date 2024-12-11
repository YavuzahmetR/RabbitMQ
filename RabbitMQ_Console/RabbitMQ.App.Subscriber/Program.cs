using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System;
using ClassLibrary;
using System.Text.Json;

var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};


using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
//await channel.ExchangeDeclareAsync("header-exchange", ExchangeType.Headers, durable: true); write or do not doesn't matter 


await channel.BasicQosAsync(0, 1, false); // 0 : size could be anything , 1: once at a time send to subscriber , false: do not split messages evenly.
//false wont require new instance of rmq, it pulls from cache

var queue = await channel.QueueDeclareAsync();

var randomQueueName = queue.QueueName;

Dictionary<string, object> headers = new Dictionary<string, object>();
headers.Add("format", "pdf");
headers.Add("shape", "triangle");

headers.Add("x-match", "all"); // There needs to be an exact match in genres between what is sent from the publisher and what the consumer expects.
//headers.Add("x-match", "any");  At least 1 key value pair of types must match between what is sent from the publisher and what the consumer expects.

await channel.QueueBindAsync(randomQueueName, "header-exchange",string.Empty,headers!);

var consumer = new AsyncEventingBasicConsumer(channel);

await channel.BasicConsumeAsync(queue: randomQueueName, autoAck: false, consumer: consumer);

//autoAck : instantly delete / See the result first then delete

Console.WriteLine("Logs Listening...");

consumer.ReceivedAsync += async (object sender, BasicDeliverEventArgs @event) =>
{
    var message = Encoding.UTF8.GetString(@event.Body.ToArray());

    Product product = JsonSerializer.Deserialize<Product>(message)!;

    await Task.Delay(1500);

    Console.WriteLine($"Gelen Mesaj : {product.Id} - {product.Name} - {product.Stock} - {product.Price}");
    await channel.BasicAckAsync(deliveryTag: @event.DeliveryTag, multiple: false);
};

Console.ReadLine();

