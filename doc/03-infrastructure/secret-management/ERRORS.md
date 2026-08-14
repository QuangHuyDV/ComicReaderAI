# Secret Management Errors

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Secret Management
> **Document:** Errors
> **Path:** `03-infrastructure/secret-management/ERRORS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines the error taxonomy, error codes, causes, severity levels, and handling strategies for the Secret Management module.

---

## 2. Error Principles

- All errors are CRAI-typed; backend-specific exceptions are wrapped at the adapter boundary.
- `SECRET_ACCESS_DENIED` must always be returned for unauthorized access — never `SECRET_NOT_FOUND`, to prevent secret enumeration.
- Secret values must never appear in error messages or structured error metadata.
- Errors are classified by recoverability.

---

## 3. Error Taxonomy

### 3.1 Not Found Errors

#### SECRET_NOT_FOUND

```text
Code:       SECRET_NOT_FOUND
Severity:   WARNING
Cause:      The requested SecretId does not exist in the registry.
Recovery:   Caller should verify the SecretId and check registration.
```

Note: This code is returned only when the caller is authorized to know the secret exists.
Unauthorized callers always receive `SECRET_ACCESS_DENIED`.

---

### 3.2 Access Errors

#### SECRET_ACCESS_DENIED

```text
Code:       SECRET_ACCESS_DENIED
Severity:   WARNING (security audit)
Cause:      The caller's ModuleIdentity is not in the secret's accessScope.
Recovery:   Caller must request access scope update through configuration.
```

---

### 3.3 Expiration Errors

#### SECRET_EXPIRED

```text
Code:       SECRET_EXPIRED
Severity:   ERROR
Cause:      The secret exists but has passed its expiresAt timestamp.
Recovery:   Secret must be rotated or replaced.
```

#### SECRET_REVOKED

```text
Code:       SECRET_REVOKED
Severity:   ERROR
Cause:      The secret has been explicitly revoked.
Recovery:   A new secret must be registered.
```

---

### 3.4 Storage Errors

#### SECRET_STORAGE_FAILED

```text
Code:       SECRET_STORAGE_FAILED
Severity:   ERROR
Cause:      The backend could not persist the new secret value.
Recovery:   Retry after backend status check.
```

#### SECRET_STORE_UNAVAILABLE

```text
Code:       SECRET_STORE_UNAVAILABLE
Severity:   ERROR
Cause:      The backend is temporarily unreachable.
Recovery:   Wait for backend reconnection. Operations queue until recovery.
```

---

### 3.5 Operation Errors

#### SECRET_ROTATION_FAILED

```text
Code:       SECRET_ROTATION_FAILED
Severity:   ERROR
Cause:      The rotation operation could not complete atomically.
Recovery:   Previous value should remain valid. Retry rotation.
```

#### SECRET_REVOCATION_FAILED

```text
Code:       SECRET_REVOCATION_FAILED
Severity:   ERROR
Cause:      The revocation operation could not complete.
Recovery:   Retry revocation. Secret remains in previous state.
```

---

### 3.6 Module Errors

#### SECRET_MANAGEMENT_NOT_READY

```text
Code:       SECRET_MANAGEMENT_NOT_READY
Severity:   CRITICAL
Cause:      An operation was attempted before module initialization completed.
Recovery:   Wait for SecretManagementInitialized event.
```

#### SECRET_ENCRYPTION_FAILED

```text
Code:       SECRET_ENCRYPTION_FAILED
Severity:   CRITICAL
Cause:      Encryption layer could not process the secret value.
Recovery:   Check encryption key availability and backend health.
```

---

## 4. Error Handling Rules

| Scenario | Expected Behavior |
|---|---|
| Unauthorized access | Return `SECRET_ACCESS_DENIED`, emit `SecretAccessDenied` event |
| Expired secret resolved | Return `SECRET_EXPIRED`, emit `SecretExpired` event if not already emitted |
| Backend unavailable | Return `SECRET_STORE_UNAVAILABLE`, emit `SecretBackendDisconnected` |
| Secret not found (authorized caller) | Return `SECRET_NOT_FOUND` |
| Secret not found (unauthorized caller) | Return `SECRET_ACCESS_DENIED` |
| Rotation failure mid-write | Return `SECRET_ROTATION_FAILED`, preserve previous value |
