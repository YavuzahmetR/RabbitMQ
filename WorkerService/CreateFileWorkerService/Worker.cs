using ClassLibrary;
using ClosedXML.Excel;
using CreateFileWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace CreateFileWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, RabbitMQClientService rabbitMQClientService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rabbitMQClientService = rabbitMQClientService;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channel = await _rabbitMQClientService.Connect();
                await _channel.BasicQosAsync(0, 1, false);
                _logger.LogInformation("RabbitMQ channel connected successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to RabbitMQ.");
                throw;
            }
            await base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            await _channel!.BasicConsumeAsync(queue: RabbitMQClientService.QueueName, autoAck: false, consumer: consumer
                ,stoppingToken);

            consumer.ReceivedAsync += Consumer_ReceivedAsync;

            _logger.LogInformation("RabbitMQ consumer started.");

            await Task.CompletedTask;

        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var memoryStream = new MemoryStream();

            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(await GetTableAsync("Products"));

            wb.Worksheets.Add(ds);

            wb.SaveAs(memoryStream);

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new ByteArrayContent(memoryStream.ToArray()), "file", Guid.NewGuid().ToString() + ".xlsx" }
            };

            var baseUrl = "https://localhost:7199/api/files";

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{baseUrl}?fileId={createExcelMessage!.FileId}", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Excel file created successfully.");
                    await _channel!.BasicAckAsync(@event.DeliveryTag, false);
                }
                else
                {
                    _logger.LogError("Error while creating Excel file.");
                }
            }

        }

        private async Task<DataTable> GetTableAsync(string tableName)
        {
            DataTable dataTable = new DataTable { TableName = tableName };

            dataTable.Columns.Add("ProductId", typeof(int));
            dataTable.Columns.Add("Name", typeof(String));
            dataTable.Columns.Add("ProductNumber", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));

            try
            {

                await using var scope = _serviceProvider.CreateAsyncScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<CreateFileWorkerService.Models.AdventureWorks2022Context>();
                var productList = await dbContext.Products.Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.ProductNumber,
                    p.Color
                }).ToListAsync();

                foreach (var item in productList)
                {
                    dataTable.Rows.Add(item.ProductId, item.Name, item.ProductNumber, item.Color);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching products for DataTable.");
                throw;
            }

            return dataTable;
        }
    }
}
