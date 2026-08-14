# Secret Management

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Secret Management

## Purpose

Secret Management là module hạ tầng chịu trách nhiệm lưu trữ, truy xuất và bảo vệ các thông tin nhạy cảm mà CRAI cần để hoạt động.

Thông tin nhạy cảm điển hình:

```text
API Keys (Translation provider, AI provider)
OAuth Tokens
Database Credentials
Encryption Keys
Plugin Credentials
```

Module này đảm bảo các secret không bị log, không bị expose qua Event Bus, và chỉ được truy cập bởi các module có quyền.

---

# Responsibilities

Secret Management chịu trách nhiệm:

- lưu trữ secret an toàn (encrypted at rest)
- cung cấp API truy xuất secret theo định danh
- quản lý vòng đời secret (rotation, expiration)
- kiểm soát access theo module/scope
- phát hiện secret hết hạn
- hỗ trợ multiple backend (local keystore, OS keychain, vault)
- không log giá trị secret dưới bất kỳ hình thức nào

Không chịu trách nhiệm:

- business logic
- OCR / Translation
- Authentication / Authorization với provider (chỉ cung cấp credential)
- Network calls
- Logging nội dung secret

---

# Public APIs

- `ResolveSecret(secretId)` → SecretValue
- `StoreSecret(secretId, value, policy)` → void
- `RevokeSecret(secretId)` → void
- `RotateSecret(secretId)` → void
- `CheckExpiration(secretId)` → ExpirationStatus

---

# Internal Components

```text
Secret Registry
Secret Store Adapter
Encryption Layer
Access Policy Enforcer
Expiration Monitor
Rotation Manager
```

---

# Lifecycle

```text
Initialize
    ↓
Load Encryption Key / Connect Backend
    ↓
Register Secrets
    ↓
Running
    ↓ (Resolve / Store / Rotate)
Shutdown
    ↓
Flush / Lock
```

---

# State Model

Các state chính được định nghĩa tại:

- `STATES.md`

Bao gồm:

- Secret Management Module
- Individual Secret lifecycle
- Backend Connection

---

# Event Model

Xem:

- `EVENTS.md`

Secret Management phát các nhóm sự kiện:

- lifecycle (init, shutdown)
- secret rotation
- secret expiration warning
- access denied

**Không bao giờ** phát event chứa giá trị secret.

---

# Error Model

Xem:

- `ERRORS.md`

Nguyên tắc:

- Secret not found ≠ Access denied (phân biệt rõ ràng)
- Lỗi backend phải được wrapped thành CRAI error types
- Không log chi tiết lỗi có thể tiết lộ secret identity

---

# Integration

Secret Management tích hợp với:

- Configuration (để load backend config)
- Logging (structured, không log secret values)
- Telemetry (metrics về access count, rotation, expiry)
- Scheduler (scheduled rotation check)

Business modules chỉ gọi `ResolveSecret()` — không biết backend cụ thể.

---

# Security Principles

- Secrets must never appear in logs.
- Secrets must never be published via Event Bus.
- Secrets must never be stored in plain text.
- Access must be scoped to the requesting module.
- Rotation must be possible without application restart.
- Expiration must be monitored proactively.

---

# MVP Scope

MVP bao gồm:

- Local encrypted keystore
- API key storage for translation providers
- API key storage for AI providers
- Manual rotation support
- Expiration tracking

Chưa bao gồm:

- HashiCorp Vault integration
- Hardware Security Module (HSM)
- Distributed secret management
- Automatic rotation with provider

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md

Kiến trúc liên quan:

- `doc/01-architecture/core/CAPABILITY_MAP.md`
- `doc/03-infrastructure/configuration/MODULE.md`
