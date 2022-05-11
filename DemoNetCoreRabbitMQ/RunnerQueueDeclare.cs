using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DemoNetCoreRabbitMQ
{
    public class RunnerQueueDeclare
    {
        private readonly ILogger<RunnerQueueDeclare> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _queueName = "RunnerQueueDeclareQueue";
        public RunnerQueueDeclare(ILogger<RunnerQueueDeclare> logger,
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
            // 宣告佇列
            // queue: "",          //訊息佇列名稱
            // durable: false,     //是否持久化,true持久化,佇列會儲存磁碟,伺服器重啟時可以保證不丟失相關資訊。
            // exclusive: false,   //是否排他,true排他的,如果一個佇列宣告為排他佇列,該佇列僅對首次宣告它的連線可見,並在連線斷開時自動刪除.
            // autoDelete: false,  //是否自動刪除。true是自動刪除。自動刪除的前提是：致少有一個消費者連線到這個佇列，之後所有與這個佇列連線的消費者都斷開時,才會自動刪除.
            // arguments: null     //設定佇列的一些其它引數
            channelDeclare.QueueDeclare(_queueName, false, false, false, null);
            var basicGetResult = channelDeclare.BasicGet(_queueName, false);
            _logger.LogInformation($"{_queueName}:{(basicGetResult != null ? basicGetResult.MessageCount : 0)}");

            // 消費者
            using var connectionConsumer = _connectionFactory.CreateConnection();
            using var channelConsumer = connectionConsumer.CreateModel();
            var eventingBasicConsumer = new EventingBasicConsumer(channelConsumer);
            eventingBasicConsumer.Received += (sender, eventArgs) =>
            {
                //Thread.Sleep(3000);
                _logger.LogInformation($"Consumer Receive:{Encoding.UTF8.GetString(eventArgs.Body.ToArray())}");
                // durable: true
                // 收到回覆後，RabbitMQ會直接在佇列中刪除這條訊息
                //channelConsumer.BasicAck(eventArgs.DeliveryTag, true); 
            };
            // 每次只能向消費者傳送一條資訊,再消費者未確認之前,不再向他傳送資訊
            //channelConsumer.BasicQos(0, 1, false);
            // 將autoAck設定 false 關閉自動確認
            channelConsumer.BasicConsume(_queueName, false, eventingBasicConsumer);
            //channelConsumer.BasicConsume(_queueName, true, eventingBasicConsumer);

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
                    channelProducer.BasicPublish("", _queueName, null, binary);
                    _logger.LogInformation($"Producer Pubish:{message}");
                }
            } 
            while (!string.IsNullOrEmpty(message));
        }
    }
}
