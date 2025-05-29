using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace ManutencaoPreditiva.Api.Infrastructure.Mqtt;
public class MqttService
{
    private readonly IMqttClient _client;
    private readonly MqttFactory _factory;

    public MqttService()
    {
        _factory = new MqttFactory();
        _client = _factory.CreateMqttClient();
    }

    public async Task ConnectAsync()
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("mosquitto", 1883)
            .WithClientId("ApiClient-" + Guid.NewGuid().ToString())
            .Build();

        try
        {
            await _client.ConnectAsync(options);
            Console.WriteLine("Connected to MQTT broker");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
            throw;
        }
    }

    public async Task SubscribeAsync(string topic, Func<string, Task> onMessageReceived)
    {
        try
        {
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            Console.WriteLine($"Subscribed to topic: {topic}");

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                Console.WriteLine($"Received message on {topic}: {message}");
                await onMessageReceived(message);
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to subscribe to topic {topic}: {ex.Message}");
            throw;
        }
    }
}
