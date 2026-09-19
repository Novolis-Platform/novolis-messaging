<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.SecureText

Transport-neutral message envelopes and endpoint sessions for secure-text v1.

## Install

```bash
dotnet add package Novolis.Messaging.SecureText
```

## What this package provides

- Canonical binary `SecureTextEnvelope` serialization.
- Header authenticated data that binds routing, timestamp, key epoch, and counter.
- A bounded replay window and persistable counter state.
- `SecureTextSession` to protect and open UTF-8 text at endpoints.

The relay transports serialized envelopes as opaque bytes. It may validate size and routing fields,
but it does not decrypt content.

## Usage

```csharp
using Novolis.Messaging.SecureText;
using Novolis.Security.SecureText;

var localIdentity = SecureTextDeviceIdentity.Create();
var localBundle = SecureTextPublicBundle.Create(localIdentity);
var trustedPeer = new SecureTextTrustedPeer(peerBundle);

using var session = new SecureTextSession(
    localIdentity,
    localBundle,
    peerBundle,
    trustedPeer,
    SecureTextConversationId.New());

var envelope = session.Protect("authenticated text");
byte[] wireBytes = SecureTextEnvelopeCodec.Serialize(envelope);
```

Persist `session.State.Snapshot()` before sending and after each successful receive. A reset
counter weakens replay protection. Store private identities with a protected implementation of
`ISecureTextKeyStore`; never place them in an ordinary file or configuration.

## Limits

Secure-text v1 is one pinned device pair. It is not group messaging or an audited ratchet
protocol, and it makes no forward-secrecy or post-compromise-security claim.
