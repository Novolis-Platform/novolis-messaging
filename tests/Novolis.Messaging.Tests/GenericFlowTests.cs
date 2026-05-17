using FluentAssertions;
using Novolis.Messaging;
using Novolis.Messaging.Internal;

namespace Novolis.Messaging.Tests;

public sealed class GenericFlowTests
{
    [Test]
    public void CanHandle_matches_exact_runtime_type_only()
    {
        var handler = new EchoHandler();
        var flow = new GenericFlow<TestPulse, EchoHandler>(handler);

        flow.CanHandle(typeof(TestPulse)).Should().BeTrue();
        flow.CanHandle(typeof(DerivedPulse)).Should().BeFalse();
        flow.CanHandle(typeof(object)).Should().BeFalse();
    }

    [Test]
    public async Task HandleAsync_invokes_handler_with_typed_pulse()
    {
        var handler = new EchoHandler();
        var flow = new GenericFlow<TestPulse, EchoHandler>(handler);
        var pulse = new TestPulse { Value = 7 };

        await flow.HandleAsync(pulse, CancellationToken.None);

        handler.Last.Should().BeSameAs(pulse);
    }

    [Test]
    public async Task HandleAsync_with_incompatible_pulse_throws_IncompatibleFlowException()
    {
        var handler = new EchoHandler();
        var flow = new GenericFlow<TestPulse, EchoHandler>(handler);

        await Assert.ThrowsAsync<IncompatibleFlowException>(async () =>
            await flow.HandleAsync(new OtherPulse(), CancellationToken.None));
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
