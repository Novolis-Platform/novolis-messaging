using Novolis.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TUnit.Core;

namespace Novolis.Messaging.Tests;

/// <summary>
/// TDD: optional diagnostics must not change default behavior; when configured, unmatched pulses and flow faults are observable.
/// </summary>
public sealed class PulseFlowDiagnosticsTests
{
    [Test]
    public async Task Unmatched_pulse_invokes_UnmatchedPulse_when_configured()
    {
        var unmatched = new List<PulseUnmatchedContext>();
        var builder = Host.CreateApplicationBuilder([]);
        builder.Services.ConfigurePulseFlowDiagnostics(o =>
        {
            o.UnmatchedPulse = unmatched.Add;
        });
        builder.Services.AddPulseFlow<MatchesNothingFlow>();
        builder.Services.AddHostedService<SingleOrphanSender>();

        var host = builder.Build();
        await host.StartAsync();
        try
        {
            await Task.Delay(300);
        }
        finally
        {
            await host.StopAsync();
        }

        await Assert.That(unmatched.Count).IsEqualTo(1);
        await Assert.That(unmatched[0].Pulse).IsTypeOf<OrphanPulse>();
        await Assert.That(unmatched[0].PulseType).IsEqualTo(typeof(OrphanPulse));
    }

    [Test]
    public async Task Flow_fault_invokes_FlowFault_when_configured()
    {
        var faults = new List<PulseFlowFaultContext>();
        var builder = Host.CreateApplicationBuilder([]);
        builder.Services.ConfigurePulseFlowDiagnostics(o =>
        {
            o.FlowFault = faults.Add;
        });
        builder.Services.AddPulseFlow<ThrowingFlow>();
        builder.Services.AddHostedService<SingleFaultPulseSender>();

        var host = builder.Build();
        await host.StartAsync();
        try
        {
            await Task.Delay(300);
        }
        finally
        {
            await host.StopAsync();
        }

        await Assert.That(faults.Count).IsEqualTo(1);
        await Assert.That(faults[0].FlowType).IsEqualTo(typeof(ThrowingFlow));
        await Assert.That(faults[0].Pulse).IsTypeOf<FaultPulse>();
        await Assert.That(faults[0].Exception).IsTypeOf<InvalidOperationException>();
        await Assert.That(faults[0].Exception!.Message).IsEqualTo("Simulated fault");
    }

    [Test]
    public async Task Default_without_callbacks_host_still_processes_matched_pulses()
    {
        var handled = new List<Guid>();
        var builder = Host.CreateApplicationBuilder([]);
        // No Configure<PulseFlowDiagnosticsOptions>
        builder.Services.AddPulseFlow<RecordingFlow>();
        builder.Services.AddSingleton(handled);
        builder.Services.AddHostedService<SingleOkSender>();

        var host = builder.Build();
        await host.StartAsync();
        try
        {
            await Task.Delay(300);
        }
        finally
        {
            await host.StopAsync();
        }

        await Assert.That(handled.Count).IsEqualTo(1);
    }

    private sealed class OrphanPulse : BasePulse;

    private sealed class FaultPulse : BasePulse;

    private sealed class OkPulse : BasePulse;

    private sealed class MatchesNothingFlow : IFlow
    {
        public bool CanHandle(Type pulseType) => false;

        public Task HandleAsync(IPulse pulse, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class ThrowingFlow : IFlow
    {
        public bool CanHandle(Type pulseType) => pulseType == typeof(FaultPulse);

        public Task HandleAsync(IPulse pulse, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Simulated fault");
    }

    private sealed class RecordingFlow(List<Guid> ids) : IFlow
    {
        public bool CanHandle(Type pulseType) => pulseType == typeof(OkPulse);

        public Task HandleAsync(IPulse pulse, CancellationToken cancellationToken)
        {
            ids.Add(pulse.Id);
            return Task.CompletedTask;
        }
    }

    private sealed class SingleOrphanSender(IConduit conduit) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
            await conduit.SendAsync(new OrphanPulse(), stoppingToken);
    }

    private sealed class SingleFaultPulseSender(IConduit conduit) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
            await conduit.SendAsync(new FaultPulse(), stoppingToken);
    }

    private sealed class SingleOkSender(IConduit conduit) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
            await conduit.SendAsync(new OkPulse(), stoppingToken);
    }
}
