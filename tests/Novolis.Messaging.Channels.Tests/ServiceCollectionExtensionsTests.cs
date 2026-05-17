using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Testing.TestBases;

namespace Novolis.Messaging.Channels.Tests;

public class ServiceCollectionExtensionsTests : HostApplicationTestBase
{
    private readonly List<MyDto> _dtos = [];

    protected override Task SetupAsync(HostApplicationBuilder builder)
    {
        builder.Services.AddChannel<MyDto>();
        builder.Services.AddSingleton(_dtos);
        builder.Services.AddHostedService<MyChannelListener>();
        return Task.CompletedTask;
    }

    [Test]
    public async Task AddChannel_registers_reader_and_writer()
    {
        var channel = GetServices.GetRequiredService<ChannelWriter<MyDto>>();
        var myDto = new MyDto { Name = "Test" };

        for (var i = 0; i < 100; i++)
            await channel.WriteAsync(myDto);

        await Task.Delay(100);
        _dtos.Should().Contain(myDto);
        TestContext.Current?.OutputWriter.WriteLine($"_dtos.Count: {_dtos.Count}");
    }

    private class MyChannelListener(ChannelReader<MyDto> channelReader, List<MyDto> dtos) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await channelReader.WaitToReadAsync(stoppingToken))
            {
                var dto = await channelReader.ReadAsync(stoppingToken);
                dtos.Add(dto);
            }
        }
    }
}
