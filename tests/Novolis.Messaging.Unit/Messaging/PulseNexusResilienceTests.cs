using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Novolis.Testing.TestBases;
using TUnit.Core;

namespace Novolis.Messaging.Tests;

public sealed class PulseNexusResilienceTests : HostApplicationTestBase
{
    private readonly PulseDispatchRecorder _recorder = new();

    public PulseNexusResilienceTests() : base(LogLevel.Information)
    {
    }

    protected override Task SetupAsync(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(_recorder);
        builder.Services.AddPulseFlow<ThrowOnFirstPulseFlow>();
        builder.Services.AddPulseFlow<RecordingFlow>();
        builder.Services.AddHostedService<TwoPulseSender>();
        return Task.CompletedTask;
    }

    [Test]
    public async Task Subsequent_pulses_are_processed_after_a_flow_throws()
    {
        await Task.Delay(800);
        await Assert.That(_recorder.Labels.ToArray()).IsEquivalentTo(["first", "second"]);
    }

    private sealed class LabelPulse : BasePulse
    {
        public required string Label { get; init; }
    }

    private sealed class PulseDispatchRecorder
    {
        public List<string> Labels { get; } = [];
    }

    private sealed class ThrowOnFirstPulseFlow : IFlow
    {
        private int _count;

        public Task HandleAsync(IPulse pulse, CancellationToken cancellationToken)
        {
            if (++_count == 1)
                throw new InvalidOperationException("first pulse fails");
            return Task.CompletedTask;
        }

        public bool CanHandle(Type pulseType) => pulseType == typeof(LabelPulse);
    }

    private sealed class RecordingFlow(PulseDispatchRecorder recorder) : IFlow
    {
        public Task HandleAsync(IPulse pulse, CancellationToken cancellationToken)
        {
            if (pulse is LabelPulse label)
                recorder.Labels.Add(label.Label);
            return Task.CompletedTask;
        }

        public bool CanHandle(Type pulseType) => pulseType == typeof(LabelPulse);
    }

    private sealed class TwoPulseSender(IConduit conduit) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await conduit.SendAsync(new LabelPulse { Label = "first" }, stoppingToken);
            await conduit.SendAsync(new LabelPulse { Label = "second" }, stoppingToken);
        }
    }
}
