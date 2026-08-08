using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Novolis.Testing.TUnit;
using Novolis.Testing.TestBases;
using TUnit.Core;

namespace Novolis.Messaging.Tests;

public class PulseFlowTests : HostApplicationTestBase
{
    private readonly TestPulseContainer _container = new();

    public PulseFlowTests() : base(LogLevel.Information)
    {
    }

    protected override Task SetupAsync(HostApplicationBuilder builder)
    {
        builder.Services.AddPulseFlow(x => x.AddFlow<BlueOutputFlow>());
        builder.Services.AddPulseFlow<RedOutputFlow>();
        builder.Services.AddPulseFlow<TimerPulse, TimerHandler>();
        builder.Services.AddPulseFlow<TimerPulse, TimerHandler2>();
        builder.Services.AddHostedService<MyService>();
        builder.Services.AddSingleton(_container);

        TestContext.Current?.WriteTable(builder.Services.Select(x => new
        {
            Service = x.ServiceType.Name,
            Implementation = x.ImplementationType?.Name,
            x.Lifetime
        }).OrderBy(x => x.Service));
        return Task.CompletedTask;
    }

    [Test]
    public async Task Test1()
    {
        await WaitUntilAsync(() =>
            _container.BlueMessages.Count > 0
            && _container.RedMessages.Count > 0
            && _container.TimerPulses.Count > 0
            && _container.TimerPulses2.Count > 0);

        var overview = new[]
        {
            new { Name = "Blue", Count = _container.BlueMessages.Count },
            new { Name = "Red", Count = _container.RedMessages.Count },
            new { Name = "Timer", Count = _container.TimerPulses.Count },
            new { Name = "Timer2", Count = _container.TimerPulses2.Count },
        };

        TestContext.Current?.WriteTable(overview);

        await Assert.That(_container.BlueMessages).IsNotEmpty();
        await Assert.That(_container.RedMessages).IsNotEmpty();
        await Assert.That(_container.TimerPulses).IsNotEmpty();
        await Assert.That(_container.TimerPulses2).IsNotEmpty();
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, int timeoutMs = 2000)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (predicate())
                return;
            await Task.Delay(15);
        }
    }

    private class BlueOutputFlow(TestPulseContainer container) : IFlow
    {
        public async Task HandleAsync(IPulse pulse, CancellationToken cancellationToken)
        {
            if (pulse is MyMessage thing)
                container.BlueMessages.Add(thing);
            await Task.CompletedTask;
        }

        public bool CanHandle(Type pulseType) => pulseType == typeof(MyMessage);
    }

    private class RedOutputFlow(TestPulseContainer container) : IFlow
    {
        public async Task HandleAsync(IPulse pulse, CancellationToken cancellationToken)
        {
            if (pulse is MyMessage thing)
                container.RedMessages.Add(thing);
            await Task.CompletedTask;
        }

        public bool CanHandle(Type pulseType) => pulseType == typeof(MyMessage);
    }

    private class MyService(IConduit conduit) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var stopWatch = Stopwatch.StartNew();
            while (!stoppingToken.IsCancellationRequested && stopWatch.Elapsed < TimeSpan.FromSeconds(1))
            {
                await conduit.SendAsync(new MyMessage("Hello, World! " + stopWatch.Elapsed.ToString("c")), stoppingToken);
                await conduit.SendAsync(new TimerPulse { Elapsed = stopWatch.Elapsed }, stoppingToken);
            }
        }
    }

    private class TimerHandler(TestPulseContainer container) : IPulseHandler<TimerPulse>
    {
        public async Task HandleAsync(TimerPulse pulse, CancellationToken cancellationToken)
        {
            container.TimerPulses.Add(pulse);
            await Task.CompletedTask;
        }
    }

    private class TimerHandler2(TestPulseContainer container) : IPulseHandler<TimerPulse>
    {
        public async Task HandleAsync(TimerPulse pulse, CancellationToken cancellationToken)
        {
            container.TimerPulses2.Add(pulse);
            await Task.CompletedTask;
        }
    }

    private class MyMessage(string message) : BasePulse
    {
        public string Message { get; set; } = message;

        public override string ToString() => $"MyMessage: {Message}";
    }

    private class TimerPulse : BasePulse
    {
        public TimeSpan Elapsed { get; set; }
    }

    private class TestPulseContainer
    {
        public List<MyMessage> BlueMessages { get; } = [];
        public List<MyMessage> RedMessages { get; } = [];
        public List<TimerPulse> TimerPulses { get; } = [];
        public List<TimerPulse> TimerPulses2 { get; } = [];
    }
}
