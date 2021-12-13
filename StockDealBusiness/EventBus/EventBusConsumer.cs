using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockDealDal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockDealBusiness.EventBus
{
    public interface IEventBusConsumer { }


    public class EventBusConsumer : IHostedService, IEventBusConsumer
    {
        private readonly ILogger _logger;

        private static IConnection _connection;
        private static IModel _channel;

        private string consumerTag = "";
        private string currentQueue;
        private string currentRouting;

#if DEBUG
        private const bool _autoDelete = true;
#else
        private const bool _autoDelete = false;
#endif

        public EventBusConsumer()
        {
            var loggerFactory = LoggerFactory.Create(e => e.AddConsole().AddFile("Logs/logs.txt"));
            _logger = loggerFactory.CreateLogger<EventBusConsumer>();
        }



        public async Task OnReceiverRequest(object model, BasicDeliverEventArgs ea)
        {
            var topic = ea.RoutingKey.Split(".");
            _logger.LogInformation($"------ OnReceiverRequest ----- {topic}");

            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            string sendKey = "";

            try
            {
                var message = Encoding.UTF8.GetString(body);

                var method = topic[2];
                // SECURITY.CALENDAR.DoSomething.REQUEST
                // CALENDAR.SECURITY.DoSomething.RESPONSE
                sendKey = string.Format("{0}.{1}.{2}.{3}", topic[1], topic[0], topic[2], "RESPONSE");

                _logger.LogInformation($"------ Handle request ----- {ea.RoutingKey}");
                _logger.LogInformation($"------ Handle response ----- {sendKey}");

                var _rabbitHandleMessage = new EventBusHandleMessage();
                Byte[] responseBytes = await _rabbitHandleMessage.ResponseResult(method, message);

                // send Result to rabbitMQ
                // response to service
                if (props.ReplyTo != null)
                {
                    _logger.LogInformation($"------ response successful ----- {props.ReplyTo} {sendKey} responseBytes.Length: {responseBytes.Length}");
                    SendResult(props, sendKey, responseBytes);
                }
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                var errorMessage = new BaseResponse
                {
                    StatusCode = 400,
                    Message = ex.Message
                };
                Byte[] responseError = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errorMessage));

                if (props != null && props.ReplyTo != null)
                {
                    SendResult(props, sendKey, responseError);
                }
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }

        }



        private void SendResult(IBasicProperties props, string sendKey, Byte[] responseBytes)
        {
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            _channel.BasicPublish(exchange: props.ReplyTo, routingKey: sendKey, basicProperties: replyProps, body: responseBytes);
        }



        public Task StartAsync(CancellationToken cancellationToken)
        {

            currentQueue = $"Q_{ConstEventBus.CURRENT_SERVICE}_{ConstEventBus.REQUEST_METHOD}";
            currentRouting = $"*.{ConstEventBus.CURRENT_SERVICE}.*.{ConstEventBus.REQUEST_METHOD}";

            var connectionFactory = new ConnectionFactory() { DispatchConsumersAsync = true };
            ConstEventBus._configuration.GetSection("EventBusConnection").Bind(connectionFactory);

            _logger.LogInformation("Consumer StartAsync");

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: ConstEventBus.CURRENT_EXCHANGE, type: "topic");

            // tao Queue
            _channel.QueueDeclare(queue: currentQueue, durable: false, exclusive: false, autoDelete: _autoDelete);

            _channel.BasicQos(0, 1, false);

            // Bind Queue to Exchange & RoutingKey
            _channel.QueueBind(queue: currentQueue, exchange: ConstEventBus.CURRENT_EXCHANGE, routingKey: currentRouting);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += OnReceiverRequest;

            consumerTag = _channel.BasicConsume(queue: currentQueue, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.BasicCancel(consumerTag);
            _channel.QueueUnbind(currentQueue, ConstEventBus.CURRENT_EXCHANGE, currentRouting);
            _channel.QueueDelete(currentQueue);

            _channel?.Close();
            _connection?.Close();

            return Task.CompletedTask;
        }

    }
}
