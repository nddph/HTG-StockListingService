using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockDealBusiness.EventBus
{

    public interface IEventBusPublisher
    {
        public Task<string> SendAsync(string topic, string messagestring, string exchangeName, bool isReply);
    }


    public class EventBusPublisher : IHostedService, IEventBusPublisher
    {
        private readonly ILogger _logger;

        private static readonly IConnection _connection = InitializeConnection();
        private static readonly IModel _channel = InitializeChannel();

        private string consumerTag;
        private string currentQueue;
        private string currentRouting;

        private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> pendingMessageQueues = new();

#if DEBUG
        private const bool _autoDelete = true;
#else
        private const bool _autoDelete = false;
#endif


        static IConnection InitializeConnection()
        {
            var connectionFactory = new ConnectionFactory() { DispatchConsumersAsync = true };
            ConstEventBus._configuration.GetSection("EventBusConnection").Bind(connectionFactory);
            return connectionFactory.CreateConnection();
        }


        static IModel InitializeChannel()
        {
            return _connection.CreateModel();
        }



        public EventBusPublisher()
        {
            var loggerFactory = LoggerFactory.Create(e => e.AddConsole().AddFile("Logs/logs.txt"));
            _logger = loggerFactory.CreateLogger<EventBusPublisher>();
        }



        public async Task OnReceiverResult(object model, BasicDeliverEventArgs ea)
        {
            try
            {
                var props = ea.BasicProperties;
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);

                var correlationId = props.CorrelationId;
                pendingMessageQueues.TryGetValue(correlationId, out TaskCompletionSource<string> pendingMessage);

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                if (pendingMessage == null)
                {
                    _logger.LogError($"RabbitMQPublisher OnReceiverResult {correlationId} tcs is null");
                    return;
                }

                pendingMessage.SetResult(response);

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }

        }



        public async Task<string> SendAsync(string topic, string messagestring, string exchangeName, bool isReply)
        {
            string res = "";

            var correlationId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            pendingMessageQueues.TryAdd(correlationId, task);

            // send a new request to server
            Publish(topic, messagestring, exchangeName, correlationId, isReply);

            if (isReply) res = await task.Task;

            pendingMessageQueues.TryRemove(correlationId, out _);
            return res;
        }



        private void Publish(string topic, string messagestring, string exchangeName, string correlationId, bool isReply)
        {
            try
            {
                // define a new CorrelationId
                var props = _channel.CreateBasicProperties();
                props.CorrelationId = correlationId;
                // if true then response
                props.ReplyTo = isReply ? ConstEventBus.CURRENT_EXCHANGE : null;

                var messageBytes = string.IsNullOrWhiteSpace(messagestring) ? null : Encoding.UTF8.GetBytes(messagestring);

                _channel.BasicPublish(exchange: exchangeName, routingKey: topic, basicProperties: props, body: messageBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }



        public static async Task<string> CallEventBusAsync(string routingKey, string messagestring, string exchangeName, bool isReply)
        {
            var rabbitMQPublisher = new EventBusPublisher();
            return await rabbitMQPublisher.SendAsync(routingKey, messagestring, exchangeName, isReply);
        }



        public Task StartAsync(CancellationToken cancellationToken)
        {

            currentQueue = $"Q_{ConstEventBus.CURRENT_SERVICE}_{ConstEventBus.RESPONSE_METHOD}";
            currentRouting = $"*.{ConstEventBus.CURRENT_SERVICE}.*.{ConstEventBus.RESPONSE_METHOD}";

            var _connectionFactory = new ConnectionFactory() { DispatchConsumersAsync = true };
            ConstEventBus._configuration.GetSection("EventBusConnection").Bind(_connectionFactory);

            _logger.LogInformation("Publisher StartAsync");

            _channel.ExchangeDeclare(exchange: ConstEventBus.CURRENT_EXCHANGE, type: "topic");

            _channel.QueueDeclare(
                queue: currentQueue,
                durable: false,
                exclusive: false,
                autoDelete: _autoDelete,
                arguments: null
            );

            _channel.QueueBind(currentQueue, ConstEventBus.CURRENT_EXCHANGE, currentRouting);
            _channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnReceiverResult;

            consumerTag = _channel.BasicConsume(currentQueue, false, consumer);

            return Task.CompletedTask;

        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.BasicCancel(consumerTag);
            _channel.QueueUnbind(currentQueue, ConstEventBus.CURRENT_EXCHANGE, currentRouting);
            _channel.QueueDelete(currentQueue);

            _channel?.Close();
            _connection?.Close();

            pendingMessageQueues.Clear();

            return Task.CompletedTask;
        }

    }
}
