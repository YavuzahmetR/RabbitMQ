
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ_Watermark.Web.Events;
using RabbitMQ_Watermark.Web.Services;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace RabbitMQ_Watermark.Web.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IChannel? _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = await _rabbitMQClientService.Connect();
            await _channel.BasicQosAsync(0, 1, false, cancellationToken);
            await base.StartAsync(cancellationToken);

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            await _channel!.BasicConsumeAsync(queue: RabbitMQClientService.QueueName, autoAck: false,
                consumer: consumer, cancellationToken: stoppingToken);

            consumer.ReceivedAsync += Consumer_ReceivedAsync;

            await Task.CompletedTask;

        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
        {
            Task.Delay(1000).Wait();

            try
            {
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(
                    Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images",
                    productImageCreatedEvent!.ImageName);

                var siteName = "www.rabiask.com";

                using var image = Image.FromFile(path);
                using var graphic = Graphics.FromImage(image);

                var font = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Italic, GraphicsUnit.Pixel);

                var textSize = graphic.MeasureString(siteName, font);

                var color = Color.FromArgb(128, 255, 255, 255);

                var brush = new SolidBrush(color);

                var position = new Point(image.Width - ((int)textSize.Width + 30), image.Height - ((int)textSize.Height + 30));

                graphic.DrawString(siteName, font, brush, position);

                image.Save("wwwroot/images/watermarks/" + productImageCreatedEvent.ImageName);

                image.Dispose();
                graphic.Dispose();

                await _channel.BasicAckAsync(@event.DeliveryTag, false);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
            }
            await Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
    }
}
