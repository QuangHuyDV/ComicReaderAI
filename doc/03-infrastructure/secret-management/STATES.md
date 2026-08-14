# Secret Management States

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Secret Management
> **Document:** State Model
> **Path:** `03-infrastructure/secret-management/STATES.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines the state machines for:

- the Secret Management module itself;
- individual managed secrets;
- the secret store backend connection.

---

## 2. Secret Management Module States

```text
Uninitialized
    ↓ Initialize()
Initializing
    ↓ backend connected, encryption key loaded
Ready
    ↓ Shutdown()
ShuttingDown
    ↓ flush, lock
Shutdown

Initializing → Failed  (if backend unavailable or key missing)
Failed       → Retrying → Ready  (if recovery configured)
```

| State | Meaning |
|---|---|
| `Uninitialized` | Module not yet started |
| `Initializing` | Connecting to backend, loading encryption key |
| `Ready` | Accepting resolve, store, revoke, rotate operations |
| `ShuttingDown` | Flushing pending writes, locking keystore |
| `Shutdown` | Module is stopped |
| `Failed` | Critical initialization failure |
| `Retrying` | Attempting backend reconnection |

---

## 3. Individual Secret States

```text
Registered
    ↓ StoreSecret()
Stored (Active)
    ↓ ExpiresAt reached
ExpirationWarning
    ↓ ExpiresAt passed
Expired

Stored (Active)
    ↓ RevokeSecret()
Revoked

Stored (Active)
    ↓ RotateSecret()
Rotating → Active (new generation)
```

| State | Meaning |
|---|---|
| `Registered` | Secret identity registered but no value stored yet |
| `Active` | Secret has a valid stored value |
| `ExpirationWarning` | Secret will expire within the warning threshold |
| `Expired` | Secret has passed its expiration time; cannot be resolved |
| `Revoked` | Secret has been explicitly revoked; cannot be resolved |
| `Rotating` | New value is being stored atomically |

---

## 4. Secret Store Backend States

```text
Disconnected
    ↓ Connect()
Connecting
    ↓ success
Connected
    ↓ Disconnect()
Disconnecting → Disconnected

Connecting  → ConnectionFailed
Connected   → ConnectionFailed → Reconnecting → Connected
```

| State | Meaning |
|---|---|
| `Disconnected` | Not connected to backend |
| `Connecting` | Establishing backend connection |
| `Connected` | Backend available |
| `Disconnecting` | Graceful disconnect in progress |
| `ConnectionFailed` | Backend unreachable |
| `Reconnecting` | Attempting to restore connection |

---

## 5. Transition Rules

- A secret in `Expired` state must not transition to `Active` through normal resolution; rotation creates a new generation.
- A secret in `Revoked` state cannot be restored; a new secret with the same ID may be registered only after explicit cleanup.
- Module must not accept `StoreSecret` or `RotateSecret` while in `ShuttingDown` or `Shutdown` state.
- `ResolveSecret` must be rejected with `SECRET_STORE_UNAVAILABLE` when backend is in `ConnectionFailed` state.
