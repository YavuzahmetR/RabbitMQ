using RabbitMQ.Client;

namespace RabbitMQ_Excel.Web.Services
{
    public class RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger) : IAsyncDisposable
    {

        private IConnection? _connection;
        private IChannel? _channel;
        public static readonly string ExchangeName = "ExcelDirectExchange";
        public static readonly string RoutingExcel = "excel-route-file";
        public static readonly string QueueName = "queue-excel-file";

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


            await _channel.QueueBindAsync(exchange: ExchangeName, queue: QueueName, routingKey: RoutingExcel);

            logger.LogInformation("A Connection Established With RabbitMQ");


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

            logger.LogInformation("Connection Lost With RabbitMQ");
        }
    }
}
