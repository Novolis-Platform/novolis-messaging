using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging;
using Novolis.Messaging.Channels;

namespace Novolis.Messaging.Channels.Tests;

public sealed class ChannelPublisherTests
{
    [Test]
    public async Task ChannelMessagePublisher_writes_to_channel()
    {
        var channel = Channel.CreateUnbounded<Message<MyDto>>();
        var publisher = new ChannelMessagePublisher<MyDto>(channel.Writer);
        var message = new Message<MyDto> { Payload = new MyDto { Name = "published" } };

        await publisher.PublishAsync(message);

        await Assert.That(await channel.Reader.WaitToReadAsync()).IsTrue();
        var read = await channel.Reader.ReadAsync();
        await Assert.That(read.Payload!.Name).IsEqualTo("published");
    }

    [Test]
    public async Task AddBoundedChannel_registers_factory_path()
    {
        var services = new ServiceCollection();
        services.AddChannel<MyDto>(ChannelType.Bounded, new ChannelSettings { BoundedCapacity = 4 });
        var provider = services.BuildServiceProvider();

        var channel = provider.GetRequiredService<Channel<MyDto>>();
        await Assert.That(channel).IsNotNull();
    }

    [Test]
    public async Task AddChannel_with_type_overload_registers_bounded_channel()
    {
        var services = new ServiceCollection();
        services.AddChannel<MyDto>(ChannelType.Bounded);
        var provider = services.BuildServiceProvider();
        await Assert.That(provider.GetRequiredService<Channel<MyDto>>()).IsNotNull();
    }

    [Test]
    public async Task AddChannel_throws_when_registered_twice()
    {
        var services = new ServiceCollection();
        services.AddChannel<MyDto>();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            services.AddChannel<MyDto>();
            return Task.CompletedTask;
        });
    }
}
