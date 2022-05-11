using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DemoNetCoreRabbitMQ
{
    public class RunnerExchangeDeclare
    {
        // Exchange模式
        // Fanout:釋出訂閱模式
        // 將資料倒給所有綁定在此Exchange的Queue，如果沒有綁定Queue會自動丟棄進來的資料
        // Direct:路由模式
        // 依照Routing Key將資料倒給對應的Queue，如果沒有對應的Routing Key會自動丟棄進來的資料
        // Topic:萬用字元模式
        // 同direct，支援 #(多字串) 或 *(單一字串) 的模糊搜尋
        // Headers:
        // 根據資料的header做導向
        private readonly ILogger<RunnerExchangeDeclare> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _queueName1 = "RunnerExchangeDeclareQueue1";
        private readonly string _queueName2 = "RunnerExchangeDeclareQueue2";
        private readonly string _exchangeName = "RunnerExchangeDeclare";
        private readonly string _producerRoutingKey = "RunnerExchangeDeclare";
        private readonly string _consumerRoutingKeyDirect = "RunnerExchangeDeclare";
        private readonly string _consumerRoutingKeyTopic = "RunnerExchange*";
        public RunnerExchangeDeclare(ILogger<RunnerExchangeDeclare> logger,
            IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }
        public void Run()
        {
            // 宣告
            using var connectionDeclare = _connectionFactory.CreateConnection();
            using var channelDeclare = connectionDeclare.CreateModel();
            // 宣告交換機 ExchangeType
            channelDeclare.ExchangeDeclare(_exchangeName, ExchangeType.Fanout);
            //channelDeclare.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
            //channelDeclare.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            // 宣告佇列
            channelDeclare.QueueDeclare(_queueName1, false, false, false, null);
            channelDeclare.QueueDeclare(_queueName2, false, false, false, null);
            // 將佇列與交換機進行繫結
            channelDeclare.QueueBind(_queueName1, _exchangeName, "", null);
            channelDeclare.QueueBind(_queueName2, _exchangeName, "", null);
            var basicGetResult1 = channelDeclare.BasicGet(_queueName1, false);
            _logger.LogInformation($"{_queueName1}:{(basicGetResult1 != null ? basicGetResult1.MessageCount : 0)}");
            var basicGetResult2 = channelDeclare.BasicGet(_queueName2, false);
            _logger.LogInformation($"{_queueName2}:{(basicGetResult2 != null ? basicGetResult2.MessageCount : 0)}");

            // 消費者
            // 消費者1
            using var connectionConsumer1 = _connectionFactory.CreateConnection();
            using var channelConsumer1 = connectionConsumer1.CreateModel();
            var eventingBasicConsumer1 = new EventingBasicConsumer(channelConsumer1);
            eventingBasicConsumer1.Received += (sender, eventArgs) =>
            {
                _logger.LogInformation($"Consumer1 Receive:{Encoding.UTF8.GetString(eventArgs.Body.ToArray())}");
            };
            channelConsumer1.BasicConsume(_queueName1, false, eventingBasicConsumer1);
            // 消費者2
            using var connectionConsumer2 = _connectionFactory.CreateConnection();
            using var channelConsumer2 = connectionConsumer2.CreateModel();
            var eventingBasicConsumer2 = new EventingBasicConsumer(channelConsumer2);
            eventingBasicConsumer2.Received += (sender, eventArgs) =>
            {
                _logger.LogInformation($"Consumer2 Receive:{Encoding.UTF8.GetString(eventArgs.Body.ToArray())}");
            };
            channelConsumer2.BasicConsume(_queueName2, false, eventingBasicConsumer2);

            // 生產者
            using var connectionProducer = _connectionFactory.CreateConnection();
            using var channelProducer = connectionProducer.CreateModel();
            var message = "start";
            do
            {
                if (!string.IsNullOrEmpty(message))
                {
                    // Publish
                    Console.WriteLine("Publish:");
                    message = Console.ReadLine()!;
                    var binary = Encoding.UTF8.GetBytes(message);
                    channelProducer.BasicPublish(_exchangeName, "", null, binary);
                    _logger.LogInformation($"Producer Pubish:{message}");
                }
            }
            while (!string.IsNullOrEmpty(message));
        }
    }
}
