﻿using ClassLibrary;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;



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
basicProperties.Persistent = true; //ensures that messages are not lost if the broker restarts

Product product = new Product(1, "Test", 200, 15);
string productJsonString = JsonSerializer.Serialize(product);
byte[] message = Encoding.UTF8.GetBytes(productJsonString);

await channel.BasicPublishAsync(exchange:"header-exchange",routingKey:string.Empty,
    mandatory:false,basicProperties:basicProperties,body:message);

Console.WriteLine("mesaj gönderilmiştir");

Console.ReadLine();




