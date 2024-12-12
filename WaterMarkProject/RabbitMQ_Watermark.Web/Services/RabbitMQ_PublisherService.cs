using RabbitMQ.Client;
using RabbitMQ_Watermark.Web.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMQ_Watermark.Web.Services
{
    public class RabbitMQ_PublisherService
    {
        private readonly RabbitMQClientService _rabbitmqClientService;

        public RabbitMQ_PublisherService(RabbitMQClientService rabbitmqClientService)
        {
            _rabbitmqClientService = rabbitmqClientService;
        }

        public async void PublishAsync(ProductImageCreatedEvent productImageCreatedEvent)
        {
            var channel = await _rabbitmqClientService.Connect();
            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

            var properties = new BasicProperties();
            properties.Persistent = true;

            await channel.BasicPublishAsync(exchange: RabbitMQClientService.ExchangeName,
                routingKey: RabbitMQClientService.RoutingWatermark, mandatory: true, basicProperties: properties
                , body: bodyBytes);

        }
    }
}
