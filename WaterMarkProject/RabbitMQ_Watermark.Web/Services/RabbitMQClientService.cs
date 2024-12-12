using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace RabbitMQ_Watermark.Web.Services
{
    public class RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger) : IAsyncDisposable
    {

        private IConnection? _connection;
        private IChannel? _channel;
        public static readonly string ExchangeName = "ImageDirectExchange";
        public static readonly string RoutingWatermark = "watermark-route-image";
        public static readonly string QueueName = "queue-watermark-image";
       
        public async Task<IChannel> Connect()
        {
            _connection = await connectionFactory.CreateConnectionAsync();


            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(ExchangeName, type: "direct", true, false);

            await _channel.QueueDeclareAsync(QueueName, true, false, false, null);


            await _channel.QueueBindAsync(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark);

            logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");


            return _channel;

        }
        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            logger.LogInformation("RabbitMQ ile bağlantı koptu...");
        }
    }
}
