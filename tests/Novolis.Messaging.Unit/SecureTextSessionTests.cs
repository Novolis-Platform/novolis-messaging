using System.Security.Cryptography;
using Novolis.Messaging.SecureText;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.Unit;

public sealed class SecureTextSessionTests
{
    [Test]
    public async Task Endpoints_round_trip_canonical_envelope()
    {
        var aliceIdentity = SecureTextDeviceIdentity.Create();
        var bobIdentity = SecureTextDeviceIdentity.Create();
        var aliceBundle = SecureTextPublicBundle.Create(aliceIdentity);
        var bobBundle = SecureTextPublicBundle.Create(bobIdentity);
        var conversation = SecureTextConversationId.New();

        using var alice = new SecureTextSession(
            aliceIdentity,
            aliceBundle,
            bobBundle,
            new SecureTextTrustedPeer(bobBundle),
            conversation);
        using var bob = new SecureTextSession(
            bobIdentity,
            bobBundle,
            aliceBundle,
            new SecureTextTrustedPeer(aliceBundle),
            conversation);

        var envelope = alice.Protect("Hello, 世界");
        var encoded = SecureTextEnvelopeCodec.Serialize(envelope);
        var decoded = SecureTextEnvelopeCodec.Deserialize(encoded);
        var received = bob.Open(decoded);

        await Assert.That(received.Text).IsEqualTo("Hello, 世界");
        await Assert.That(received.SenderDeviceId).IsEqualTo(alice.LocalDeviceId);
        await Assert.That(encoded.AsSpan().IndexOf("Hello, 世界"u8) < 0).IsTrue();
    }

    [Test]
    public async Task Tampered_ciphertext_and_replay_are_rejected()
    {
        var (alice, bob) = CreatePair();
        using (alice)
        using (bob)
        {
            var envelope = alice.Protect("integrity");
            var ciphertext = envelope.ExportCiphertext();
            ciphertext[0] ^= 0x01;
            var tampered = new SecureTextEnvelope(
                envelope.Header,
                envelope.ExportNonce(),
                ciphertext,
                envelope.ExportAuthenticationTag());

            await Assert.That(() => bob.Open(tampered)).Throws<CryptographicException>();
            var first = bob.Open(envelope);
            await Assert.That(first.Text).IsEqualTo("integrity");
            await Assert.That(() => bob.Open(envelope)).Throws<CryptographicException>();
        }
    }

    [Test]
    public async Task Replay_window_rejects_implausible_counter_jump()
    {
        var state = new SecureTextConversationState();

        await Assert.That(state.TryAcceptInboundCounter(SecureTextConversationState.MaximumCounterGap + 1)).IsFalse();
        await Assert.That(state.TryAcceptInboundCounter(1)).IsTrue();
        await Assert.That(state.TryAcceptInboundCounter(1)).IsFalse();
        await Assert.That(state.TryAcceptInboundCounter(2)).IsTrue();
    }

    [Test]
    public async Task Session_rejects_a_valid_envelope_from_an_unpinned_device()
    {
        var aliceIdentity = SecureTextDeviceIdentity.Create();
        var bobIdentity = SecureTextDeviceIdentity.Create();
        var malloryIdentity = SecureTextDeviceIdentity.Create();
        var aliceBundle = SecureTextPublicBundle.Create(aliceIdentity);
        var bobBundle = SecureTextPublicBundle.Create(bobIdentity);
        var malloryBundle = SecureTextPublicBundle.Create(malloryIdentity);
        var expectedConversation = SecureTextConversationId.DeriveForPair(
            SecureTextDeviceId.FromGuid(aliceIdentity.DeviceId),
            SecureTextDeviceId.FromGuid(bobIdentity.DeviceId));

        using var bob = new SecureTextSession(
            bobIdentity,
            bobBundle,
            aliceBundle,
            new SecureTextTrustedPeer(aliceBundle),
            expectedConversation);
        using var mallory = new SecureTextSession(
            malloryIdentity,
            malloryBundle,
            bobBundle,
            new SecureTextTrustedPeer(bobBundle),
            SecureTextConversationId.DeriveForPair(
                SecureTextDeviceId.FromGuid(malloryIdentity.DeviceId),
                SecureTextDeviceId.FromGuid(bobIdentity.DeviceId)));

        var impostorEnvelope = mallory.Protect("not from the pinned peer");

        await Assert.That(() => bob.Open(impostorEnvelope)).Throws<CryptographicException>();
    }

    private static (SecureTextSession Alice, SecureTextSession Bob) CreatePair()
    {
        var aliceIdentity = SecureTextDeviceIdentity.Create();
        var bobIdentity = SecureTextDeviceIdentity.Create();
        var aliceBundle = SecureTextPublicBundle.Create(aliceIdentity);
        var bobBundle = SecureTextPublicBundle.Create(bobIdentity);
        var conversation = SecureTextConversationId.New();

        return (
            new SecureTextSession(
                aliceIdentity,
                aliceBundle,
                bobBundle,
                new SecureTextTrustedPeer(bobBundle),
                conversation),
            new SecureTextSession(
                bobIdentity,
                bobBundle,
                aliceBundle,
                new SecureTextTrustedPeer(aliceBundle),
                conversation));
    }
}
