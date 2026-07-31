# Runtime Resource Lifecycle

> Project: CRAI
> Version: 0.1
> Status: Architecture Draft

---

# 1. Purpose

This document defines how runtime resources are created, owned, transferred, retained, released, and disposed.

Resources include:

- revisions
- artifacts
- buffers
- provider requests
- workers
- UI models
- cache entries
- leases
- native handles
- GPU resources

The lifecycle model ensures that every resource has a clear owner and a deterministic end of life.

---

# 2. Lifecycle Goals

The lifecycle system must:

- define one logical owner
- avoid ownership ambiguity
- prevent premature disposal
- prevent resource leaks
- support cancellation
- support cache promotion
- support artifact sharing
- support deterministic cleanup

---

# 3. Lifecycle Philosophy

CRAI treats resources as owned objects.

Ownership is explicit.

Sharing occurs through references or leases.

Disposal occurs only after every owner has released the resource.

---

# 4. Resource Categories

Examples:

Application Resource

↓

Session Resource

↓

Revision Resource

↓

Artifact Resource

↓

Worker Resource

↓

Provider Resource

↓

UI Resource

↓

Temporary Resource

---

# 5. Generic Lifecycle

Every resource conceptually follows:

```text
Create
    ↓
Register
    ↓
Publish
    ↓
Acquire
    ↓
Use
    ↓
Release
    ↓
Eligible for Disposal
    ↓
Disposed
```

Not every resource passes through every phase.

However, ownership must always be explicit.

---

# 6. Creation

The creator owns the resource immediately after creation.

Example:

```text
Capture Worker
    ↓
Creates Image Buffer
```

Capture Worker is the owner.

---

# 7. Registration

Large runtime resources should be registered before sharing.

Example:

```text
Image Buffer
    ↓
Artifact Store
```

Registration assigns:

- identity
- ownership scope
- lifecycle metadata

---

# 8. Publication

Publication makes a resource visible to other runtime components.

Before publication:

Only creator can access.

After publication:

Other components may acquire references.

Publication must be atomic.

---

# 9. Ownership Transfer

Ownership may move.

Example:

```text
Capture Worker

↓

Artifact Store

↓

Cache

↓

Disposed
```

Only one logical owner exists at a time.

---

# 10. Lease

Workers never own shared artifacts.

Workers acquire leases.

```text
Acquire

↓

Read

↓

Release
```

Lease expiration never disposes the artifact directly.

---

# 11. Temporary Resources

Temporary resources include:

- image resize buffers
- request payloads
- tensors
- decoded images

They never enter shared ownership.

Worker creates.

Worker releases.

---

# 12. Shared Resources

Shared resources include:

- OCR artifact
- Layout artifact
- Translation artifact
- Presentation artifact

Shared resources always belong to:

Artifact Store

or

Cache

---

# 13. Cache Promotion

Artifact

↓

Revision Scope

↓

Validated

↓

Promoted

↓

Cache Ownership

Promotion changes ownership.

It should not duplicate payload.

---

# 14. Cache Eviction

Eviction means:

Cache ownership removed.

If another owner exists:

Artifact survives.

Otherwise:

Artifact becomes disposable.

---

# 15. Revision Disposal

Revision disposal:

Remove revision metadata.

Release revision ownership.

Do not immediately delete artifacts still leased.

---

# 16. Artifact Disposal

Artifact disposal requires:

No Revision owner

No Cache owner

No UI owner

No Worker lease

Then:

Dispose payload.

---

# 17. Session Shutdown

Session shutdown:

Cancel work

↓

Release session resources

↓

Dispose revisions

↓

Release presentation

↓

Release providers if unused

---

# 18. Application Shutdown

Application shutdown:

Stop scheduler

↓

Cancel sessions

↓

Drain workers

↓

Dispose cache

↓

Unload providers

↓

Dispose remaining resources

---

# 19. Provider Lifecycle

Provider lifecycle:

Create client

↓

Initialize

↓

Ready

↓

Idle

↓

Unload

↓

Disposed

---

# 20. Local AI Model Lifecycle

Local model:

Load

↓

Ready

↓

Inference

↓

Idle

↓

Unload

↓

Disposed

---

# 21. Worker Lifecycle

Worker:

Created

↓

Idle

↓

Assigned

↓

Running

↓

Completed

↓

Idle

↓

Disposed

Workers should be reused.

---

# 22. Native Resource Lifecycle

Native resources require explicit disposal.

Examples:

GPU textures

Window handles

OCR handles

Capture surfaces

---

# 23. Resource Dependencies

Example:

Revision

↓

OCR Artifact

↓

Translation Artifact

↓

Presentation Artifact

Disposing parent must respect child leases.

---

# 24. Disposal Order

Recommended order:

Presentation

↓

Translation

↓

Layout

↓

OCR

↓

Source Image

↓

Revision

---

# 25. Cleanup Failures

Cleanup failure:

Log

↓

Retry if safe

↓

Mark diagnostics

↓

Never restore ownership

---

# 26. Lifecycle Events

Examples:

artifact.created

artifact.published

artifact.released

artifact.disposed

revision.disposed

provider.loaded

provider.unloaded

worker.started

worker.idle

---

# 27. Metrics

Track:

Active resources

Disposed resources

Average lifetime

Lease count

Disposal latency

Native resources

GPU resources

---

# 28. MVP Policy

MVP requires:

Revision ownership

Artifact Store ownership

Worker leases

Cache promotion

Deterministic disposal

No complex reference graph

---

# 29. Architecture Invariants

- One logical owner.
- Immutable artifacts.
- Lease before read.
- Publish before share.
- Dispose only when owner and leases are gone.
- Cache never owns canceled work forever.
- UI never owns runtime state.
- Workers never dispose shared artifacts.

---

# 30. Related Documents

- MEMORY_MODEL.md
- CACHE_POLICY.md
- THREADING_MODEL.md
- CANCELLATION.md

---

# 31. Summary

Every resource follows ownership rather than lifetime.

The model is:

```text
Create
    ↓
Register
    ↓
Publish
    ↓
Acquire
    ↓
Release
    ↓
Dispose
```

Ownership is explicit.

Sharing uses leases.

Disposal occurs only after ownership and active references disappear.