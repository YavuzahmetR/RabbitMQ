using ClassLibrary;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMQ_Excel.Web.Services
{
    public class RabbitMQ_PublisherService
    {
        private readonly RabbitMQClientService _rabbitmqClientService;

        public RabbitMQ_PublisherService(RabbitMQClientService rabbitmqClientService)
        {
            _rabbitmqClientService = rabbitmqClientService;
        }

        public async void PublishAsync(CreateExcelMessage createExcelMessage)
        {
            var channel = await _rabbitmqClientService.Connect();
            var bodyString = JsonSerializer.Serialize(createExcelMessage);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

            var properties = new BasicProperties();
            properties.Persistent = true;

            await channel.BasicPublishAsync(exchange: RabbitMQClientService.ExchangeName,
                routingKey: RabbitMQClientService.RoutingExcel, mandatory: true, basicProperties: properties
                , body: bodyBytes);

        }
    }
}
