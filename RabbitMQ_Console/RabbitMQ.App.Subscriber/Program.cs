using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory
{
    Uri = new Uri("amqps://ooparaka:ax-HHpY3d-7GXX2237tka-5qQFhc8NEM@sparrow.rmq.cloudamqp.com/ooparaka")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
//await channel.QueueDeclareAsync("hello-queue", durable: true, exclusive: false, autoDelete: false); write or do not doesn't matter 




await channel.BasicQosAsync(0, 1, false); // 0 : size could be anything , 1: once at a time send to subscriber , false: do not split messages evenly.
//false wont require new instance of rmq, it pulls from cache


var consumer = new AsyncEventingBasicConsumer(channel);
await channel.BasicConsumeAsync(queue: "hello-queue", autoAck: false, consumer: consumer); //autoAck : instantly delete / See the result first then delete

consumer.ReceivedAsync += async (object sender, BasicDeliverEventArgs @event) =>
{
    var message = Encoding.UTF8.GetString(@event.Body.ToArray());
    await Task.Delay(2000);
    Console.WriteLine("Gelen Mesaj : " + message);
   await  channel.BasicAckAsync(deliveryTag: @event.DeliveryTag, multiple: true);
    //return Task.CompletedTask;
};

Console.ReadLine();


//Console.WriteLine("Mesaj Gönderildi");
