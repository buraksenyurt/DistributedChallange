using System.Text.Json;
using GamersWorld.AppEvents;
using GamersWorld.EventHost.Factory;
using GamersWorld.EventHost.Reflection;
using GamersWorld.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GamersWorld.EventHost;

public class EventConsumer
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumer> _logger;

    public EventConsumer(IConnectionFactory connectionFactory, IServiceProvider serviceProvider,
        ILogger<EventConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Run()
    {
        // RabitMq bağlantısı tesis edilir ve bir kanal oluşturulur
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        // reports_event_queue isimli bir kuyruk tanımlanır
        channel.QueueDeclare(queue: "report_events_queue", durable: false, exclusive: false, autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        // Gelen mesajların yakalandığı olay metodu
        // Lambda operatörü üzerinden anonymous function olarak event handler temsilcisini bağlanır
        consumer.Received += async (model, args) =>
        {
            var message = args.Body.ToArray();
            var eventType =
                args.BasicProperties.Type; // Publish edilecek mesajı type property değerinden yakalayabiliriz

            // Kuyruktan yakalanan mesaj değerlendirilmek üzere Handle operasyonuna gönderilir
            await Handle(eventType, message);
        };

        channel.BasicConsume(queue: "report_events_queue", autoAck: true, consumer: consumer);

        Console.WriteLine("Kuyruk mesajları dinleniyor. Çıkmak için bir tuşa basın.");
        Console.ReadLine();
    }

    private async Task Handle(string eventType, byte[] eventMessage)
    {
        // _logger.LogInformation("Event: #{} , Message: {}", eventType, eventMessage);

        using var scope = _serviceProvider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<EventHandlerFactory>();

        // Kuyruktan yakalanan Event ve mesaj içeriği burada değerlendirlir
        // eventType türüne göre JSON formatından döndürülen mesaj içeriği
        // factory nesnesi üzerinden uygun business nesnesinin execute fonksiyonuna kadar gönderilir

        var type = EventTypeLoader.ReflectionLoad(eventType);
        var obj = JsonSerializer.Deserialize(eventMessage, type);
        await factory.ReflectionInvoke(type, obj);
    }
}
