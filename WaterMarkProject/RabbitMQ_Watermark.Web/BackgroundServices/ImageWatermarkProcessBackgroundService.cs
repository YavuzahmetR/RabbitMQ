
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
            const int maxRetryAttempts = 3;
            int retryAttempts = 0;
            bool processedSuccessfully = false;

            while (retryAttempts < maxRetryAttempts && !processedSuccessfully)
            {
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

                    await _channel.BasicAckAsync(@event.DeliveryTag, false);
                    processedSuccessfully = true;
                }
                catch (FileNotFoundException ex)
                {
                    _logger.LogError(ex, "File not found: {FileName}", @event.Body.ToArray());
                    break; // No point in retrying if the file is not found
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
                    break; // No point in retrying if the message is malformed
                }
                catch (Exception ex)
                {
                    retryAttempts++;
                    _logger.LogError(ex, "Error processing message. Attempt {RetryAttempts} of {MaxRetryAttempts}", retryAttempts, maxRetryAttempts);

                    if (retryAttempts >= maxRetryAttempts)
                    {
                        // Send to dead letter queue or log for further inspection
                        _logger.LogError("Message moved to dead letter queue: {Message}", Encoding.UTF8.GetString(@event.Body.ToArray()));
                        await _channel.BasicNackAsync(@event.DeliveryTag, false, false);
                    }
                    else
                    {
                        // Wait before retrying
                        await Task.Delay(1000);
                    }
                }
            }

            await Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
    }
}
