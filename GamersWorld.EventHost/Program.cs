﻿using GamersWorld.EventHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// RabbitMq ayarlarını da ele alacağımız için appSettings konfigurasyonu için bir builder nesnesi örnekledik
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
    .Build();

var services = new ServiceCollection();

// DI Servis eklemeleri
services.AddSingleton<IConfiguration>(configuration);

// Olay sürücüleri, RabbitMq ve Loglama gibi bileşenler DI servislerine yüklenir
services.AddEventDrivers();
services.AddRabbitMq(configuration);
services.AddLogging(cfg => cfg.AddConsole());

var serviceProvider = services.BuildServiceProvider();
// Mesaj kuyruğunu dinleyecek nesne örneklenir
var eventConsumer = serviceProvider.GetService<EventConsumer>();

// Dinleme fonksiyonu başlatılır
eventConsumer.Run();