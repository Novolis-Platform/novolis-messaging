using Novolis.Messaging;
using Novolis.Messaging.Internal;
using TUnit.Core;

namespace Novolis.Messaging.Tests;

public sealed class GenericFlowTests
{
    [Test]
    public async Task CanHandle_matches_exact_runtime_type_only()
    {
        var handler = new EchoHandler();
        var flow = new GenericFlow<TestPulse, EchoHandler>(handler);

        await Assert.That(flow.CanHandle(typeof(TestPulse))).IsTrue();
        await Assert.That(flow.CanHandle(typeof(DerivedPulse))).IsFalse();
        await Assert.That(flow.CanHandle(typeof(object))).IsFalse();
    }

    [Test]
    public async Task HandleAsync_invokes_handler_with_typed_pulse()
    {
        var handler = new EchoHandler();
        var flow = new GenericFlow<TestPulse, EchoHandler>(handler);
        var pulse = new TestPulse { Value = 7 };

        await flow.HandleAsync(pulse, CancellationToken.None);

        await Assert.That(ReferenceEquals(handler.Last, pulse)).IsTrue();
    }

    [Test]
    public async Task HandleAsync_with_incompatible_pulse_throws_IncompatibleFlowException()
    {
        var handler = new EchoHandler();
        var flow = new GenericFlow<TestPulse, EchoHandler>(handler);

        await Assert.That(async () =>
            await flow.HandleAsync(new OtherPulse(), CancellationToken.None))
            .Throws<IncompatibleFlowException>();
    }

    private class TestPulse : BasePulse
    {
        public int Value { get; init; }
    }

    private sealed class DerivedPulse : TestPulse;

    private sealed class OtherPulse : BasePulse;

    private sealed class EchoHandler : IPulseHandler<TestPulse>
    {
        public TestPulse? Last { get; private set; }

        public Task HandleAsync(TestPulse pulse, CancellationToken cancellationToken)
        {
            Last = pulse;
            return Task.CompletedTask;
        }
    }
}
