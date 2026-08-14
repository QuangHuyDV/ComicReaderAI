# Secret Management Events

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Secret Management
> **Document:** Events
> **Path:** `03-infrastructure/secret-management/EVENTS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines events emitted by the Secret Management module.

**Critical rule:** No event may contain a secret value, partial secret value, or any information that could be used to reconstruct a secret value.

Events are advisory. Consumers must not use events as a substitute for explicit `ResolveSecret()` calls.

---

## 2. Event Conventions

All Secret Management events follow the CRAI Event Convention:

- past-tense naming;
- no secret values in payload;
- metadata only (IDs, timestamps, status);
- published via Event Bus infrastructure.

---

## 3. Module Lifecycle Events

### SecretManagementInitialized

Published when the module successfully completes initialization.

```text
SecretManagementInitialized {
    timestamp
    backendType     LOCAL_KEYSTORE | OS_KEYCHAIN | REMOTE_VAULT
}
```

### SecretManagementShutdown

Published when the module has completed graceful shutdown.

```text
SecretManagementShutdown {
    timestamp
}
```

### SecretManagementFailed

Published when the module enters a failed state.

```text
SecretManagementFailed {
    timestamp
    reason          BACKEND_UNAVAILABLE | KEY_LOAD_FAILED | INTERNAL_ERROR
    retryScheduled  boolean
}
```

---

## 4. Secret Lifecycle Events

### SecretStored

Published when a secret value has been successfully stored.

```text
SecretStored {
    secretId
    timestamp
    hasExpiration   boolean
}
```

Note: value is never included.

### SecretRevoked

Published when a secret has been revoked.

```text
SecretRevoked {
    secretId
    timestamp
    revokedBy   ModuleIdentity
}
```

### SecretRotated

Published when a secret has been successfully rotated.

```text
SecretRotated {
    secretId
    timestamp
    rotatedBy   ModuleIdentity | SCHEDULED
    generation  integer
}
```

---

## 5. Expiration Events

### SecretExpirationWarning

Published when a secret is approaching expiration, within the configured warning threshold.

```text
SecretExpirationWarning {
    secretId
    expiresAt
    warningThreshold    duration
    timestamp
}
```

### SecretExpired

Published when a secret has passed its expiration time.

```text
SecretExpired {
    secretId
    expiredAt
    timestamp
}
```

---

## 6. Access Events

### SecretAccessDenied

Published when an unauthorized module attempts to resolve a secret.

```text
SecretAccessDenied {
    secretId
    callerModuleId
    timestamp
}
```

Note: this is a security-relevant event and should be routed to audit logging.

---

## 7. Backend Events

### SecretBackendConnected

Published when the backend connection is established or restored.

```text
SecretBackendConnected {
    backendType
    timestamp
}
```

### SecretBackendDisconnected

Published when the backend connection is lost.

```text
SecretBackendDisconnected {
    backendType
    timestamp
    reason
}
```
