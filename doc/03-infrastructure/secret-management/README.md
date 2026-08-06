# Secret Management

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Secret Management

## Purpose

Secret Management is the single owner of secret identity, secret lifecycle, secure storage,
resolution, lease management, rotation, revocation, and safe exposure boundaries.

The module guarantees that raw secret material never crosses ordinary architecture boundaries.

## Responsibilities

- Register, replace, rotate and remove secrets.
- Resolve `SecretReference` into bounded `SecretLease` / `SecretHandle`.
- Enforce consumer, purpose and policy checks.
- Manage secure storage backends.
- Validate secret material.
- Prevent secret leakage in logs, events, telemetry and diagnostics.
- Publish safe lifecycle events.

## Module Boundaries

Owns:

- Secret identity
- Secret descriptors
- Secret revisions
- Secret leases
- Backend abstraction
- Rotation and migration
- Redaction
- Secret-safe diagnostics

Does not own:

- Provider selection
- Runtime scheduling
- UI
- Configuration precedence
- Translation logic

## Public Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md

## Architecture

```text
Configuration
      │
SecretReference
      │
      ▼
Secret Management
      │
      ├── Policy
      ├── Backend
      ├── Lease
      ├── Validation
      ├── Rotation
      └── Redaction
      │
      ▼
Provider Management
      │
      ▼
Provider Adapter
```

## Core Principles

1. Secret identity is separate from secret material.
2. References are public; material is private.
3. Every access is policy checked.
4. Every lease is consumer-bound and purpose-bound.
5. Rotation creates a new revision.
6. Unknown outcomes require reconciliation.
7. Security failures fail closed.
8. Secret values never appear in logs, events or errors.

## State Summary

```text
REGISTERING
ACTIVE
SUSPENDED
ROTATING
MIGRATING
REVOKING
REMOVING
REMOVED
TOMBSTONED
```

## Typical Flow

```text
Register Secret
      ↓
Store Securely
      ↓
Resolve Reference
      ↓
Acquire Lease
      ↓
Use Secret
      ↓
Release Lease
      ↓
Rotate / Revoke / Remove
```

## Integration

Configuration stores only `SecretReference`.

Provider Management requests leases but never owns persistent secret material.

Runtime only observes normalized failures and availability.

Presentation never receives raw secret material.

## Security Guarantees

- No raw secret serialization.
- No secret values in diagnostics.
- Restricted exports only.
- Mandatory redaction.
- Safe error normalization.
- Least-privilege access.

## MVP Scope

Supported:

- OS secure store
- Memory backend
- Environment reference backend
- Secret registration
- Resolution
- Lease management
- Rotation
- Validation
- Redaction

Deferred:

- Cloud secret managers
- HSM
- Multi-device sync
- Encrypted backup
- Enterprise policy service

## Related Documents

```text
MODULE.md
CONTRACT.md
STATES.md
EVENTS.md
ERRORS.md

Configuration/*
Provider Management/*
runtime/ERROR_MODEL.md
runtime/RETRY_POLICY.md
runtime/RESOURCE_LIFECYCLE.md
```
