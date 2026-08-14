# Secret Management Contract

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Secret Management
> **Document:** Contract
> **Path:** `03-infrastructure/secret-management/CONTRACT.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines the public interface of the Secret Management module: the operations it exposes, the types it uses, and the rules callers must follow.

---

## 2. Core Principles

The Secret Management contract guarantees:

- every secret has a unique, stable identity;
- secret values never leave the module boundary in observable form;
- callers receive a typed value handle, not a raw string where possible;
- unauthorized access is rejected explicitly;
- expired secrets are detected proactively, not on first failed API call.

---

## 3. Secret Identity

```text
SecretId = {domain}.{type}.{qualifier}

Examples:
    translation.api-key.deepl
    ai.api-key.openai
    storage.encryption-key.v1
    provider.oauth-token.google
```

Secret IDs are case-insensitive and must be unique within the CRAI secret registry.

---

## 4. Operations

### 4.1 ResolveSecret

```text
ResolveSecret(secretId: SecretId, caller: ModuleIdentity)
    → SecretValue
    | SecretNotFoundError
    | SecretAccessDeniedError
    | SecretExpiredError
    | SecretStoreUnavailableError
```

Returns the decrypted secret value to an authorized caller.

The caller must supply its module identity for access scope validation.

### 4.2 StoreSecret

```text
StoreSecret(secretId: SecretId, value: PlaintextSecret, policy: SecretPolicy, caller: ModuleIdentity)
    → void
    | SecretAccessDeniedError
    | SecretStorageFailedError
    | SecretStoreUnavailableError
```

Stores or updates a secret value with an associated policy.

The plaintext value must not be retained after encryption.

### 4.3 RevokeSecret

```text
RevokeSecret(secretId: SecretId, caller: ModuleIdentity)
    → void
    | SecretNotFoundError
    | SecretAccessDeniedError
    | SecretRevocationFailedError
```

Marks a secret as revoked. Revoked secrets cannot be resolved.

### 4.4 RotateSecret

```text
RotateSecret(secretId: SecretId, newValue: PlaintextSecret, caller: ModuleIdentity)
    → void
    | SecretNotFoundError
    | SecretAccessDeniedError
    | SecretRotationFailedError
```

Replaces the current secret value with a new value.

Emits `SecretRotated` event after successful rotation.

### 4.5 CheckExpiration

```text
CheckExpiration(secretId: SecretId)
    → ExpirationStatus
    | SecretNotFoundError
```

Returns the expiration status of a secret without resolving its value.

---

## 5. Types

### 5.1 SecretValue

```text
SecretValue {
    secretId    SecretId
    value       sensitive string — must not be logged
    resolvedAt  timestamp
    expiresAt   optional timestamp
}
```

### 5.2 SecretPolicy

```text
SecretPolicy {
    expiresAt          optional timestamp
    rotationInterval   optional duration
    accessScope        list of ModuleIdentity
    storageMode        LOCAL_KEYSTORE | OS_KEYCHAIN | REMOTE_VAULT
}
```

### 5.3 ExpirationStatus

```text
ExpirationStatus {
    secretId
    status      VALID | EXPIRING_SOON | EXPIRED | REVOKED
    expiresAt   optional timestamp
    warningAt   optional timestamp
}
```

---

## 6. Security Contract

The following rules are invariant:

```text
1. Secret values must never be included in log messages.
2. Secret values must never be published as Event Bus payloads.
3. Secret values must never be stored in plaintext.
4. SECRET_ACCESS_DENIED must be returned for unauthorized access — never SECRET_NOT_FOUND.
5. Expired secrets must not be silently returned; SecretExpiredError must be raised.
6. Revoked secrets must not be resolvable.
```

---

## 7. Event Notifications

Secret Management emits notification events (see `EVENTS.md`).

Events never contain secret values.

Events are advisory; consumers must not depend on events for correct secret resolution behavior.

---

## 8. Error Codes

Full error taxonomy is in `ERRORS.md`.

Summary:

| Error Code | Meaning |
|---|---|
| `SECRET_NOT_FOUND` | The requested secret identity does not exist |
| `SECRET_ACCESS_DENIED` | The caller is not authorized for this secret |
| `SECRET_EXPIRED` | The secret exists but has passed its expiration time |
| `SECRET_REVOKED` | The secret has been explicitly revoked |
| `SECRET_STORE_UNAVAILABLE` | The backend is temporarily unreachable |
| `SECRET_STORAGE_FAILED` | The backend could not persist the secret |
| `SECRET_ROTATION_FAILED` | The rotation operation did not complete |
| `SECRET_REVOCATION_FAILED` | The revocation operation did not complete |
