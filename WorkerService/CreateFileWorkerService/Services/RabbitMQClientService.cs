using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateFileWorkerService.Services
{
    public class RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger) : IAsyncDisposable
    {

        private IConnection? _connection;
        private IChannel? _channel;

        public static readonly string QueueName = "queue-excel-file";

        public async Task<IChannel> Connect()
        {
            _connection = await connectionFactory.CreateConnectionAsync();


            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = await _connection.CreateChannelAsync();

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
