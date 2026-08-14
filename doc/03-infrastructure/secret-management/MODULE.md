# Secret Management Module

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Secret Management
> **Document:** Module Architecture
> **Path:** `03-infrastructure/secret-management/MODULE.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14
> **Source of Truth:**
>
> - `doc/01-architecture/core/CAPABILITY_MAP.md`
> - `doc/01-architecture/core/DATA_FLOW.md`
> - `doc/03-infrastructure/configuration/MODULE.md`
> - `doc/03-infrastructure/logging/MODULE.md`
> - `doc/03-infrastructure/telemetry/MODULE.md`

---

## 1. Purpose

The Secret Management module provides CRAI with a unified, secure mechanism for storing and retrieving sensitive credentials and configuration values.

It abstracts the underlying secret storage backend so that business modules and other infrastructure modules can access secrets through a stable interface without depending on a specific keystore implementation.

---

## 2. Module Goal

The module must:

- prevent secrets from appearing in logs, events, or observable outputs;
- allow secrets to be stored with associated policies (expiration, rotation);
- allow secrets to be retrieved by authorized modules only;
- support backend replacement without changing consumer code;
- support proactive expiration detection;
- support manual and scheduled rotation.

The primary optimization target is:

```text
secure, auditable credential access
without coupling business logic to a specific secret storage technology
```

---

## 3. Architectural Position

```text
Business Module (e.g., Translation, Provider Management)
    ↓ ResolveSecret(secretId)
Secret Management
    ↓ delegates to
Secret Store Adapter
    ↓ implements
Local Keystore / OS Keychain / Remote Vault
```

Composition Root owns:

- Secret Management construction;
- backend adapter wiring;
- startup decryption key injection;
- shutdown flush and lock.

---

## 4. Terminology

### 4.1 Secret

A `Secret` is a named, protected value that must not be exposed outside the module boundary.

Examples:

```text
translation.api-key.deepl
ai.api-key.openai
storage.encryption-key.v1
provider.oauth-token.google
```

### 4.2 Secret Identity

A `SecretId` uniquely identifies a secret within the CRAI secret registry.

Convention:

```text
{domain}.{type}.{qualifier}
```

Examples:

```text
translation.api-key.deepl
ai.api-key.openai
provider.oauth-token.google
storage.encryption-key.v1
```

### 4.3 Secret Value

A `SecretValue` is the resolved, decrypted credential value.

It must be treated as sensitive throughout its lifetime.

It must not be logged, serialized to disk, or published as an event.

### 4.4 Secret Policy

A `SecretPolicy` describes the lifecycle rules for a secret:

```text
SecretPolicy {
    expiresAt          optional expiration time
    rotationInterval   optional scheduled rotation period
    accessScope        which modules may resolve this secret
    storageMode        LOCAL_KEYSTORE | OS_KEYCHAIN | REMOTE_VAULT
}
```

### 4.5 Secret Store Adapter

A `SecretStoreAdapter` is the implementation interface connecting Secret Management to a specific backend:

```text
LocalEncryptedKeystore
OSKeychain (Windows Credential Manager / macOS Keychain)
RemoteVault (HashiCorp Vault / AWS Secrets Manager)
```

---

## 5. Responsibilities

### 5.1 Secret Registration

Secret Management allows authorized callers to register a secret definition before its value is stored.

Registration includes:

- secret identity;
- description;
- policy.

### 5.2 Secret Storage

Secret Management encrypts and persists the secret value to the configured backend.

The plaintext value must not be retained in memory longer than necessary.

### 5.3 Secret Resolution

Secret Management decrypts and returns the secret value to authorized callers.

Resolution must:

- verify the caller's access scope;
- check expiration;
- return a typed `SecretValue` not a raw string.

### 5.4 Secret Revocation

Secret Management deletes or marks a secret as revoked.

Revoked secrets must not be resolvable.

### 5.5 Expiration Monitoring

Secret Management tracks expiration timestamps and emits `SecretExpirationWarning` events before expiry.

Proactive monitoring reduces runtime failures caused by unexpected secret expiration.

### 5.6 Rotation Support

Secret Management supports manual rotation via `RotateSecret()` and scheduled rotation via Scheduler integration.

Rotation must:

- generate or accept a new value;
- update the stored secret atomically where possible;
- emit a `SecretRotated` event;
- not interrupt active usage of the old value within a configured grace period.

### 5.7 Access Control

Every `ResolveSecret()` call must be validated against the `accessScope` defined in the secret policy.

Unauthorized access must be rejected with `SECRET_ACCESS_DENIED`, not `SECRET_NOT_FOUND`.

### 5.8 Backend Abstraction

Secret Management delegates all persistence operations to a `SecretStoreAdapter`.

Consumers have no direct access to the adapter.

---

## 6. Non-Responsibilities

### 6.1 Authentication logic

Secret Management provides credentials.

It does not authenticate with external providers.

Authentication belongs to the consuming module.

### 6.2 Network calls

Secret Management does not call translation or AI APIs.

### 6.3 Configuration management

Application configuration belongs to the Configuration module.

Secret Management stores only sensitive values.

### 6.4 Business rules

Secret Management has no knowledge of how credentials are used in business workflows.

---

## 7. Core Components

```text
Secret Registry
Secret Store Adapter (interface)
Local Encrypted Keystore (default adapter)
OS Keychain Adapter (optional)
Remote Vault Adapter (future)
Encryption Layer
Access Policy Enforcer
Expiration Monitor
Rotation Manager
Secret Audit Log (structured, value-free)
```

---

## 8. Data Model

Conceptual structure:

```text
SecretDefinition {
    secretId
    description
    policy
    createdAt
    updatedAt
    status     ACTIVE | EXPIRED | REVOKED
}

SecretValue {
    secretId
    value      (sensitive, in-memory only)
    resolvedAt
    expiresAt
}
```

---

## 9. Security Invariants

The following must hold at all times:

```text
1. Secret values never appear in log output.
2. Secret values never appear in Event Bus payloads.
3. Secret values are never written to disk in plaintext.
4. Access to a secret requires a valid, authorized caller identity.
5. Revoked secrets cannot be resolved.
6. Expired secrets trigger a warning before expiry, not a silent failure.
```

---

## 10. MVP Scope

MVP includes:

- local encrypted keystore;
- manual `StoreSecret` and `ResolveSecret`;
- API key support for translation and AI providers;
- expiration warning;
- structured audit log (no values);
- Scheduler integration for expiration checks.

Deferred:

- OS Keychain adapter;
- Remote Vault adapter;
- automatic secret rotation;
- hardware security module support;
- distributed secret management.

---

## 11. Dependencies

Required:

- Configuration (load backend config, encryption key source)
- Logging (structured audit, no values)
- Telemetry (access count, expiration metrics)

Optional:

- Scheduler (periodic expiration and rotation checks)

---

## 12. Used By

- Provider Management (resolve provider API keys)
- Translation (resolve translation provider credentials)
- Storage (resolve database encryption key)
- AI (resolve AI provider API key)
- Plugin system (resolve plugin credentials)
