# CRAI Build and Packaging

Status: Technology Baseline / Evidence-Gated Packaging
Version: 0.1.0
Updated: 2026-08-14
Path: 04-technology/BUILD_AND_PACKAGING.md
Depends On:
- 04-technology/TECH_STACK.md
- 04-technology/PERSISTENCE.md
- 04-technology/WINDOWS_PLATFORM.md
- 04-technology/OCR_CANDIDATES.md
- 04-technology/TRANSLATION_CANDIDATES.md
- 04-technology/FEASIBILITY_RESULTS.md

## 1. Purpose

Tài liệu này xác định build, publish, dependency, asset, signing, installation, update và distribution strategy cho CRAI.

Tài liệu này khóa những phần có thể quyết định độc lập với OCR runtime và giữ những phần còn lại ở trạng thái evidence-gated.

Canonical rule:

```text
Source
    ↓
Restore
    ↓
Build
    ↓
Test
    ↓
Publish
    ↓
Assemble Runtime Assets
    ↓
Package
    ↓
Sign
    ↓
Install / Upgrade Test
    ↓
Release Artifact
```

Packaging format cuối cùng chưa được chọn.

Lý do:

```text
OCR Engine
    → not selected

OCR Runtime
    → not selected

Worker requirement
    → not selected

Model asset footprint
    → not selected
```

Final Packaging Decision thuộc Gate 7.

## 2. Current Locked Baseline

Locked:

```text
Primary OS:
    Windows

Primary Architecture:
    x64

Core Runtime:
    .NET 10 LTS

Application Language:
    C#

Desktop UI:
    Avalonia

Persistence:
    SQLite

Build System:
    dotnet / MSBuild

Dependency Management:
    NuGet

Source Control:
    Git
```

Evidence-gated:

```text
Capture native dependencies
OCR runtime
OCR model assets
Python worker
Local Translation runtime
Local Translation models
Package identity
Installer format
Update mechanism
Code-signing service
Store distribution
```

## 3. Build Philosophy

Build must be:

```text
Reproducible
Versioned
Automatable
Fail-fast
Architecture-aware
Independent from developer machine state
```

A valid release build must not depend on:

- globally installed Python unless explicitly selected as product dependency;
- globally installed OCR engine;
- developer-local model path;
- Visual Studio-only manual steps;
- machine-specific absolute paths;
- untracked native DLLs;
- plaintext secrets.

## 4. Repository Build Boundary

Recommended conceptual structure:

```text
src/
tests/
benchmarks/
build/
packaging/
assets/
docs/
```

Exact repository layout is implementation-plan work.

Build concerns must remain outside Business Modules.

No module may own:

```text
installer
signing
release channel
NuGet restore policy
CI runner configuration
```

## 5. Solution Build

Primary build entry:

```text
dotnet build
```

Release pipeline uses explicit configuration:

```text
Release
```

and explicit target/runtime where required.

Recommended invariant:

```text
one canonical repository-level build command
```

which orchestrates all required projects rather than developers manually building selected projects.

## 6. Target Framework

Baseline:

```text
net10.0
```

Windows-specific projects may require a Windows-qualified target framework when APIs/packages require it.

That choice must remain localized to Windows/platform implementation projects.

Business/Core projects should not acquire Windows-specific target frameworks merely for convenience.

Canonical dependency direction:

```text
Core / Business
    → platform-neutral where architecture requires

Windows Adapter
    → Windows-specific TFM/API allowed
```

## 7. Runtime Identifier

Initial release RID:

```text
win-x64
```

Do not publish one generic artifact and assume native dependencies work everywhere.

Future:

```text
win-arm64
```

is a separate feasibility/release target, not implied by x64 success.

## 8. Build Configurations

Minimum:

```text
Debug
Release
```

Optional benchmark-specific configuration may exist only if needed for instrumentation.

Production performance benchmarks must use Release-like optimization.

Do not compare OCR/Capture runtimes using Debug binaries and treat result as release evidence.

## 9. Versioning

CRAI needs independent identities for:

```text
Application Version
Build/Commit Identity
Schema Version
Plugin/API Contract Version
OCR Model Version
Translation Profile/Prompt Version
Benchmark Dataset Version
```

Application version must not be reused as model version.

Recommended application version semantics:

```text
Major.Minor.Patch
```

Package formats may require conversion to their own numeric representation.

That conversion belongs Packaging.

## 10. Build Metadata

Release artifact should be traceable to:

```text
Application Version
Git Commit
Build Date
Target RID
Build Configuration
Dependency Lock State
```

Do not put secrets or user-specific machine information into build metadata.

## 11. NuGet Dependency Management

NuGet is the baseline .NET dependency manager.

Requirements:

- package versions explicit;
- transitive dependency changes observable;
- restore deterministic enough for CI;
- dependency ownership reviewable;
- native-runtime packages identified;
- licenses reviewable.

Central package version management may be adopted during implementation if it reduces drift across projects.

## 12. Dependency Locking

Release pipeline should use dependency locking or equivalent reproducibility control.

Goal:

```text
same source
+
same lock state
+
same toolchain
    ↓
same dependency graph
```

Unreviewed dependency upgrades must not silently enter release builds.

## 13. SDK Pinning

Repository should pin the intended .NET SDK line.

Recommended mechanism:

```text
global.json
```

Exact SDK patch can be updated deliberately.

CI and developer setup should fail clearly when an incompatible SDK is used rather than silently building with arbitrary future SDK behavior.

## 14. Native Dependencies

Native dependencies must be explicitly inventoried.

Potential sources:

```text
Avalonia native/platform components
SQLite native runtime
Windows capture/native interop
ONNX/Windows ML runtime
OCR runtime
GPU execution provider
Python/native OCR libraries
Local LLM runtime
```

For each native dependency record:

```text
Name
Version
Architecture
Origin
License
Redistribution Rights
Load Strategy
Expected Location
Checksum where appropriate
```

## 15. Native Library Loading

Do not depend on arbitrary system `PATH` for product-owned native libraries.

Preferred:

```text
Application-owned runtime layout
    ↓
deterministic load
```

OS-owned DLLs remain OS dependencies.

Any custom native search path must be explicit and security-reviewed.

## 16. Publish Model

Initial publish candidate:

```text
Self-contained .NET publish
```

Reason:

- avoids requiring user to install matching .NET runtime separately;
- gives CRAI control over runtime version;
- simplifies clean-machine testing.

This is a build/publish baseline candidate, not yet the final installer format.

Gate 7 must measure resulting size and dependency behavior.

## 17. Framework-Dependent Publish

Status:

```text
Secondary Candidate
```

Potential benefit:

- smaller application payload.

Cost:

- target machine runtime dependency;
- installation prerequisite;
- runtime-version support complexity.

Use only if Gate 7 demonstrates material benefit.

## 18. Single-File Publish

Status:

```text
Optional / Not Selected
```

.NET supports single-file publishing for framework-dependent and self-contained applications.

CRAI does not select it by default.

Reason:

```text
CRAI may contain:
    model files
    native libraries
    worker executables
    SQLite assets
    mutable configuration
```

A single application executable does not eliminate those external product assets.

Single-file must therefore be benchmarked as a publish optimization, not treated as Packaging architecture.

## 19. Trimming

Status:

```text
Not Selected
```

Trimming can reduce .NET publish size but may affect reflection/dynamic behavior and dependencies.

Do not enable release trimming until:

- Avalonia path verified;
- DI/reflection usage verified;
- SQLite verified;
- plugin loading verified;
- OCR/runtime bindings verified;
- integration tests pass on trimmed artifact.

Package size alone does not justify runtime risk.

## 20. Native AOT

Status:

```text
Deferred
```

Native AOT is not required for MVP baseline.

Potential benefits:

- startup;
- deployment characteristics.

Potential risks:

- reflection;
- dynamic loading;
- plugin architecture;
- native ML bindings;
- library compatibility;
- build complexity.

Do not reshape CRAI architecture to obtain Native AOT compatibility.

## 21. ReadyToRun

Status:

```text
Optional Benchmark Candidate
```

May be evaluated if startup performance becomes material.

Not required before Gate 1.

Any package-size increase must be measured against startup benefit.

## 22. Build Output vs Product Package

Distinguish:

```text
Build Output
    → compiler/runtime artifacts

Publish Output
    → runnable application payload

Package
    → install/distribution representation

Release
    → signed, tested deliverable
```

A successful `dotnet publish` is not equivalent to a validated installer.

## 23. Product Asset Classes

Assets must be classified:

```text
Immutable Application Assets
Model Assets
Mutable User Data
Cache
Temporary Artifacts
Secrets
Logs
Benchmark/Test Assets
```

These classes must not be mixed into one directory.

## 24. Immutable Application Assets

Examples:

- UI assets;
- bundled configuration defaults;
- provider metadata;
- native product libraries;
- static dictionaries required by selected OCR configuration.

Installed application assets should be treated as read-only at runtime.

## 25. Model Assets

OCR/local-AI models require explicit lifecycle.

Each model must have:

```text
Model ID
Version
Provider/Origin
Checksum
License Metadata
Runtime Compatibility
Language Capability
```

Do not identify models only by filename.

## 26. Bundled vs Downloaded Models

Decision remains open.

Candidate A:

```text
Bundle model in installer
```

Benefits:

- first run works offline;
- deterministic release.

Costs:

- installer size;
- every app update may carry large assets depending on package/update mechanism.

Candidate B:

```text
Download model after install
```

Benefits:

- smaller base installer;
- independent model lifecycle possible.

Costs:

- first-run network;
- integrity verification;
- retry/resume;
- model hosting;
- offline-first impact.

Gate 3 model size and Gate 7 measurements decide.

## 27. Model Integrity

Downloaded model assets must be integrity-verified.

At minimum:

```text
Expected Model Identity
+
Cryptographic Checksum
```

Transport security alone does not replace asset identity verification.

Failed verification must reject the model.

## 28. Mutable User Data

User-owned persistent data must live outside installation directory.

Includes:

- SQLite database;
- preferences;
- glossary;
- Translation Memory;
- session/history when enabled;
- mutable provider settings;
- user-created data.

Exact Windows paths are implementation detail of Persistence/Platform configuration.

Installer upgrade must not replace user DB.

## 29. Cache

Cache is disposable.

Installer must not rely on existing cache for correctness.

Upgrade can invalidate cache when compatibility changes.

Cache deletion must not delete authoritative user data.

## 30. Temporary Files

Temporary artifacts must:

- use appropriate user/temp location;
- be bounded;
- have cleanup policy;
- not contain secrets unnecessarily;
- not become implicit persistence.

OCR worker temp-image transfer, if selected, must follow this rule.

## 31. Secrets

Secrets are not package assets.

Never bundle production:

- Translation API keys;
- OCR cloud credentials;
- signing private keys;
- CI secrets.

Runtime user/provider credentials must use selected secret-storage mechanism.

Signing credentials remain release infrastructure secrets.

## 32. OCR Runtime Dependency

Packaging remains primarily blocked by Gate 3.

Potential outcomes:

### Outcome A - In-process ONNX / Windows ML

Package may need:

```text
CRAI binaries
.NET runtime
ONNX/Windows ML dependencies
OCR models
dictionaries
```

### Outcome B - Python OCR Worker

Package may need:

```text
CRAI binaries
.NET runtime
OCR worker
Python runtime
Python packages/native libraries
OCR models
dictionaries
```

### Outcome C - Native OCR Runtime

Package may need:

```text
CRAI binaries
.NET runtime
native OCR runtime
models
dependent native DLLs
```

### Outcome D - OS OCR

Package requirements may depend on:

```text
Windows API
language/runtime availability
package identity
```

Therefore installer choice cannot precede Gate 3.

## 33. Python Worker Packaging

Status:

```text
Conditional
```

If Gate 3 selects Python worker:

Do not require end users to manually install Python/pip.

The product must own a reproducible worker runtime.

Candidate strategies:

```text
Embedded/isolated Python runtime
Frozen worker executable
Other reproducible worker bundle
```

Exact strategy must be tested.

## 34. Python Environment Isolation

If Python exists:

```text
CRAI Python Runtime
    ≠
User Python
```

Do not modify:

- global pip;
- global Python PATH;
- user's virtual environments.

Worker dependencies must be pinned.

## 35. Python Worker Version Compatibility

Main process and worker must have a compatibility contract.

Record:

```text
Worker Protocol Version
Worker Build Version
OCR Model Compatibility
```

On mismatch:

```text
Fail clearly
```

not undefined IPC behavior.

## 36. GPU Runtime Packaging

GPU acceleration must remain optional unless minimum system requirements explicitly require it.

Do not bundle large CUDA/runtime dependencies before Gate 3 proves value.

Required behavior:

```text
Acceleration unavailable
    ↓
supported CPU baseline
```

unless product requirements later decide otherwise.

## 37. Local Translation Packaging

If Gate 4 selects local Translation:

Gate 7 must include:

- model size;
- inference runtime;
- RAM/VRAM requirement;
- model download/bundle;
- update;
- integrity;
- license.

Remote Translation does not introduce those local model dependencies.

## 38. Plugin Packaging

Plugin System architecture must remain independent from installer format.

MVP may ship only built-in providers/plugins.

Future third-party plugin packaging must define:

- manifest;
- version compatibility;
- trust;
- install location;
- dependency isolation;
- update/removal.

Do not expose arbitrary DLL drop-in loading merely because it is easy.

## 39. Package Identity

Status:

```text
Evidence-Gated
```

Windows package identity can unlock platform capabilities unavailable to an unpackaged desktop process.

CRAI must determine whether selected APIs actually require identity.

Do not select full MSIX merely because identity may be useful.

Possible:

```text
Unpackaged application

Full MSIX

Existing installer
+
Package with External Location
```

Gate 7 chooses based on actual selected dependencies/features.

## 40. Full MSIX

Status:

```text
Primary Packaging Candidate
```

Potential benefits:

- package identity;
- clean install/uninstall model;
- Windows integration;
- standardized deployment/update paths;
- Store/enterprise compatibility.

Constraints to test:

- signing/trust;
- immutable installed package behavior;
- worker/native runtime behavior;
- model asset strategy;
- filesystem assumptions;
- selected OCR compatibility.

MSIX must be tested with the actual runtime topology, not an empty shell.

## 41. Traditional Installer

Status:

```text
Primary Packaging Candidate
```

Representative technology may include:

```text
WiX/MSI
or
Inno Setup
```

Exact tool is not selected yet.

Potential benefits:

- conventional Win32 deployment;
- control over install layout;
- flexible external runtimes/workers.

Potential costs:

- updater must be solved separately;
- package identity absent unless added separately;
- more installer ownership.

## 42. Package with External Location

Status:

```text
Conditional Candidate
```

Windows supports granting package identity to an existing desktop application while keeping its normal external installation layout.

This may become relevant if CRAI needs:

```text
traditional installer flexibility
+
Windows package identity
```

Do not add this complexity unless a selected Windows capability requires or materially benefits from identity.

## 43. Portable ZIP

Status:

```text
Development / Diagnostic Candidate
```

Useful for:

- local testing;
- benchmark builds;
- internal smoke tests.

Not automatically suitable as production distribution because it lacks:

- installer lifecycle;
- signing/trust UX;
- update integration;
- structured uninstall.

It can remain a secondary artifact if useful.

## 44. Installer Decision Matrix

Gate 7 must compare at least realistic surviving options.

| Dimension | Full MSIX | Traditional Installer | External-Location Identity |
| --- | --- | --- | --- |
| Package Identity | Native | No by default | Yes |
| Install Layout Flexibility | Lower | High | High |
| Clean Servicing | Strong | Installer-owned | Mixed |
| Signing | Required | Recommended/format-dependent | Identity package requires signing |
| Worker Flexibility | Test | Strong | Strong |
| Large Model Assets | Test | Flexible | Flexible |
| Store Path | Strong | Separate | Scenario-dependent |
| Update | Strong options | Must design | Must design |
| OCR Runtime Fit | Unknown | Unknown | Unknown |

No winner until Gate 7.

## 45. Signing

Production release should be code-signed.

If MSIX is selected, signing is required for deployment and the certificate must be trusted by the target device.

Development may use test/self-signed certificates.

Production signing credentials must never reside in repository.

Signing occurs after package assembly and before release validation.

## 46. Artifact Signing Boundary

Release pipeline conceptually:

```text
Unsigned Build
    ↓
Package Assembly
    ↓
Signing Service / Protected Signing Environment
    ↓
Signed Artifact
    ↓
Signature Verification
    ↓
Release
```

Developer workstations must not be the canonical production-signing system.

Exact signing service remains open.

## 47. Release Channels

Initial candidate channels:

```text
Development
Preview/Beta
Stable
```

MVP may initially use fewer channels.

Channel identity must not corrupt user data when switching/upgrading.

Separate package identity/channel strategy is Gate 7 detail.

## 48. Update Strategy

Status:

```text
Not Selected
```

Possible models:

```text
Manual download/install
MSIX/App Installer update
Store-managed update
Installer-specific updater
Custom updater
```

Do not implement custom updater before packaging decision.

MVP can start with deliberate manual update if product requirements permit.

## 49. Update Invariants

Regardless of mechanism:

```text
New App Version
    ↓
Validate compatibility
    ↓
Install
    ↓
Migrate persistent schema safely
    ↓
Start
```

Must preserve:

- user data;
- glossary;
- preferences;
- Translation Memory;
- history according to policy.

Must not preserve incompatible ephemeral cache blindly.

## 50. Database Migration and Upgrade

Application upgrade and database migration are related but not identical.

Installer must not edit SQLite schema directly.

Canonical:

```text
Installer
    → installs application

Application Persistence Layer
    → performs controlled schema migration
```

Migration must support failure handling defined by Persistence architecture.

## 51. Model Upgrade

App upgrade must not silently replace model and make benchmark identity unknowable.

Model update records:

```text
Old Model ID/Version
New Model ID/Version
Compatibility
Reason
```

If model is independently downloaded, model update lifecycle may be independent from application update.

## 52. Rollback

Gate 7 must test realistic rollback constraints.

Important distinction:

```text
Binary rollback
    ≠
Database schema rollback
```

Do not promise automatic application downgrade if migrated user data is incompatible with older schema.

Forward-compatible migration strategy is preferred where practical.

## 53. Clean Install Test

Must run on clean supported Windows environment.

No developer prerequisites may be assumed.

Verify:

- install;
- launch;
- persistence;
- Capture dependency;
- OCR;
- Translation connectivity/local runtime;
- uninstall.

## 54. Upgrade Test

At minimum:

```text
Version N
    ↓
create representative user state
    ↓
install Version N+1
    ↓
verify state
    ↓
run smoke tests
```

Include model/runtime changes when relevant.

## 55. Uninstall Test

Verify:

- application binaries removed;
- installer registrations removed;
- helper processes removed;
- scheduled/update components removed;
- no orphan worker;
- behavior for user data follows explicit policy.

Do not silently delete valuable user data unless product uninstall policy explicitly says so.

## 56. Repair Test

If selected installer supports repair, test:

- missing application file;
- damaged immutable asset;
- model ownership behavior;
- user DB preservation.

Repair must not overwrite mutable user data.

## 57. First-Run Test

If dependencies/models download on first run:

Test:

- no network;
- slow network;
- interrupted download;
- checksum mismatch;
- insufficient disk;
- cancellation;
- restart/resume;
- proxy/firewall behavior where relevant.

First-run failure must not corrupt installation.

## 58. Disk Space

Gate 7 records:

```text
Installer Size
Installed Binary Size
Bundled Model Size
Downloaded Model Size
Cache Growth
Temporary Installation Space
```

Do not report only compressed installer size.

## 59. Process Topology Validation

Package test must match selected runtime topology.

Potential:

```text
CRAI.exe
```

or:

```text
CRAI.exe
    +
OCR.Worker.exe
```

or:

```text
CRAI.exe
    +
LocalAI.Worker.exe
```

Installer must own all product processes and their version compatibility.

## 60. Worker Launch

If helper process exists:

- deterministic executable path;
- no shell string construction;
- bounded privileges;
- inherited environment minimized;
- explicit IPC endpoint;
- process lifetime owned;
- clean shutdown;
- crash recovery.

Do not launch arbitrary executable from mutable user/plugin path.

## 61. Privilege

Baseline:

```text
Standard User
```

CRAI should not require administrator privileges at runtime.

Installer elevation may depend on installation scope.

Any runtime feature requiring elevation must be treated as a major feasibility constraint and explicitly justified.

## 62. Install Scope

Decision remains open:

```text
Per-user
vs
Per-machine
```

MVP preference can be evaluated toward per-user if it avoids unnecessary elevation, but Gate 7 must validate selected installer/runtime.

Do not lock install scope before package choice.

## 63. Windows Minimum Version

Final minimum Windows version must be locked after:

- Capture API decision;
- Overlay API decision;
- OCR runtime;
- package identity requirements;
- Windows AI use if any.

Do not derive minimum OS solely from Avalonia.

Gate 7 release manifest/installer must enforce the actual supported floor.

## 64. Architecture Target

Initial:

```text
x64 only
```

This simplifies:

- native dependency matrix;
- OCR runtime;
- model runtime;
- packaging;
- benchmark reproducibility.

ARM64 support requires its own:

- native dependency validation;
- performance benchmark;
- installer artifact;
- Gate evidence.

## 65. CI Build Stages

Recommended:

```text
Checkout
    ↓
Toolchain Validation
    ↓
Restore
    ↓
Compile
    ↓
Static / Architecture Checks
    ↓
Unit Tests
    ↓
Integration Tests
    ↓
Publish win-x64
    ↓
Assemble Assets
    ↓
Package
    ↓
Sign
    ↓
Install Smoke Test
    ↓
Artifact Publication
```

Benchmark jobs may be separate from every-commit CI due to hardware/API cost.

## 66. Pull Request Build

PR validation should not require production credentials.

Use:

- mocks;
- local test providers;
- recorded/non-sensitive fixtures;
- contract tests.

Cloud Translation/OCR live tests should run only in controlled jobs where needed.

## 67. Release Build

Release build requires:

- clean checkout;
- pinned SDK;
- locked dependency state;
- Release configuration;
- test pass;
- version;
- asset inventory;
- license inventory;
- package;
- signing;
- clean-machine smoke test.

No manual file copying after signing.

## 68. Benchmark Build

Benchmark artifact must be traceable to code commit and configuration.

Do not benchmark locally modified uncommitted binary and later treat numbers as canonical without recording state.

OCR/Translation benchmark datasets may remain private, but their version identity must be recorded.

## 69. Build Cache

CI cache may accelerate:

- NuGet restore;
- tool restore;
- model retrieval where licensing/security permits.

Cache is never source of truth.

Release must be reproducible after cache miss.

## 70. Supply Chain

Before release:

- inventory direct dependencies;
- review critical transitive/native dependencies;
- record licenses;
- avoid abandoned packages where practical;
- verify model provenance;
- scan known vulnerabilities using chosen tooling;
- protect release credentials.

Exact SBOM/scanning tooling can be selected during CI implementation.

## 71. SBOM

Status:

```text
Recommended
```

Release pipeline should generate or retain machine-readable dependency inventory when practical.

It should cover more than NuGet if CRAI ships:

- Python packages;
- native DLLs;
- OCR models;
- local LLM runtime.

Model provenance may require separate metadata from software SBOM.

## 72. License Notices

Release process must assemble required notices for redistributed components.

Do not assume:

```text
open source
    → no attribution obligation
```

Engine code license and model license are reviewed separately.

## 73. Debug Symbols

Release strategy should retain symbols for diagnostics where appropriate without necessarily distributing all symbols to end users.

Symbols must match exact released build.

Do not depend on rebuilding later to recreate diagnostic identity.

## 74. Crash Diagnostics

Packaging must not automatically enable sensitive dump/upload behavior.

Crash diagnostics policy must account for possible:

- OCR text;
- Translation text;
- image buffers;
- API-related memory.

Diagnostic collection follows Privacy/Logging architecture.

## 75. Configuration

Default immutable configuration can ship with application.

Mutable user configuration belongs user-data location.

Environment-specific secrets do not belong appsettings committed into release.

Provider endpoint/model defaults must be versionable and overrideable through controlled configuration.

## 76. Development Assets

Development-only assets must not enter production package:

- benchmark corpora;
- test screenshots;
- fake credentials;
- local certificates;
- debug model copies;
- developer scripts not required at runtime.

Package assembly should use allow-list style ownership where practical.

## 77. Packaging Manifest

Regardless of installer technology, maintain a conceptual package manifest/inventory:

```text
Application Binary
UI Assets
Native Dependencies
OCR Runtime
OCR Models
Translation Runtime if local
Licenses
Configuration Defaults
Worker Binaries
```

This enables comparing Gate 7 candidates with identical payload.

## 78. Package Identity and Windows Features

Package identity decision must be driven by actual required features.

If selected technology requires package identity:

```text
Gate 7
    → must provide it
```

If not:

```text
Identity remains optional
```

Do not force application architecture to depend on packaging context.

Where necessary, platform adapter may detect capability/identity and expose a normalized capability result.

## 79. MSIX Constraint: Installed Files

If full MSIX is selected, runtime must respect package immutability/servicing behavior.

Therefore mutable data and downloaded models should not be designed as arbitrary writes into installed package directories.

This aligns with CRAI's existing separation:

```text
Application Assets
    ≠
Mutable Persistence
```

## 80. Signing and Trust Test

Gate 7 must test:

```text
Unsigned development artifact
Signed development package
Production-like trusted signed package
```

where applicable.

A package that installs only after manually disabling trust protections is not a valid production result.

## 81. Distribution

Potential future distribution:

```text
Direct Download
Microsoft Store
Enterprise Distribution
```

Initial distribution channel remains open.

Packaging decision should avoid unnecessary lock-in where reasonable, but current MVP needs outweigh hypothetical channels.

## 82. Store Distribution

Status:

```text
Deferred
```

Do not make Store certification a Gate 1–6 blocker.

If Store becomes target, add:

- Store package validation;
- policy review;
- identity;
- update behavior;
- native/worker compliance;
- privacy disclosures.

## 83. Portable/Internal Builds

Internal benchmark builds may use unpackaged publish folders.

This does not determine production packaging.

Gate evidence must clearly label:

```text
Unpackaged Development Build
```

versus:

```text
Production Packaging Candidate
```

## 84. Packaging Failure Categories

Normalize failures:

```text
BUILD_FAILURE
RESTORE_FAILURE
TEST_FAILURE
PUBLISH_FAILURE
ASSET_MISSING
NATIVE_DEPENDENCY_MISSING
MODEL_INVALID
PACKAGE_FAILURE
SIGNING_FAILURE
INSTALL_FAILURE
UPGRADE_FAILURE
UNINSTALL_FAILURE
RUNTIME_START_FAILURE
```

These are build/release diagnostics, not Business errors.

## 85. Gate 7 Evidence Record

For each candidate:

```text
Candidate ID:
Package Format:
Installer Tool:
Publish Mode:
RID:
Package Identity:
Signing:
OCR Runtime:
OCR Models:
Translation Runtime:
Installed Size:
Installer Size:
Clean Install:
Upgrade:
Uninstall:
First Run:
Offline Behavior:
Runtime Smoke Test:
Known Constraints:
Evidence IDs:
Result:
```

## 86. Gate 7 Candidate A

```text
Candidate:
    Full MSIX

Status:
    NOT RUN

Prerequisite:
    Gate 3 OCR Runtime
```

## 87. Gate 7 Candidate B

```text
Candidate:
    Traditional Installer

Tool:
    TBD
    WiX/MSI or Inno Setup candidate

Status:
    NOT RUN

Prerequisite:
    Gate 3 OCR Runtime
```

## 88. Gate 7 Candidate C

```text
Candidate:
    Traditional Installer
    +
    Package with External Location

Status:
    CONDITIONAL / NOT RUN

Run only if package identity is needed
while full MSIX creates material constraints.
```

## 89. Gate 7 Decision Rule

Reject candidate if:

- cannot install actual selected runtime reliably;
- violates mutable-data ownership;
- requires developer prerequisites;
- cannot support clean upgrade;
- creates unacceptable security/trust behavior;
- breaks required Windows capability.

For survivors compare:

```text
Reliability
Runtime Compatibility
Update Model
Install UX
Package Size
Signing
Identity Needs
Implementation Complexity
Distribution Flexibility
Maintenance Cost
```

## 90. Gate 7 Required Decision Output

Gate 7 must lock:

```text
Publish Mode
Installer/Package Format
Installer Tool
Package Identity Strategy
Install Scope
Signing Strategy
Update Strategy
Model Distribution Strategy
Windows Minimum Version
Release Artifact Layout
```

## 91. Relationship to FEASIBILITY_RESULTS.md

This file defines candidates and constraints.

Actual result belongs:

```text
04-technology/FEASIBILITY_RESULTS.md
```

After Gate 7:

```text
FEASIBILITY_RESULTS.md
    ↓
evidence accepted
    ↓
TECH_STACK.md
    updated
```

Do not mark installer as selected in this file before evidence.

## 92. Relationship to TESTING.md

`TESTING.md` must define automated/manual validation for:

- build;
- architecture;
- publish;
- install;
- upgrade;
- uninstall;
- dependency presence;
- smoke tests.

This file defines what packaging must achieve.

`TESTING.md` defines how quality enforcement is organized.

## 93. Relationship to Persistence

Packaging owns installation.

Persistence owns application data/schema.

Canonical:

```text
Installer
    X
direct DB schema ownership

Persistence
    → schema/migration ownership
```

Upgrade must preserve the boundary.

## 94. Relationship to Plugin System

Installer may deploy built-in plugins/providers.

Plugin architecture owns runtime plugin semantics.

Installer does not decide:

- plugin activation;
- plugin capability;
- plugin execution authority.

Future external plugin installation requires its own trust/update policy.

## 95. Relationship to Runtime

Packaging determines where executable/runtime assets exist.

Runtime owns execution lifecycle after application starts.

Worker process packaging does not transfer lifecycle authority from Runtime.

## 96. Relationship to Windows Platform

Windows Platform adapter owns Windows-specific API usage.

Packaging may grant capabilities/identity needed by those APIs.

Business modules must not inspect:

```text
MSIX
MSI
PackageFamilyName
installer registry keys
```

to decide business behavior.

## 97. Relationship to OCR

OCR Gate has the largest current packaging dependency.

Required order:

```text
Benchmark OCR
    ↓
Select OCR Runtime
    ↓
Inventory Runtime/Model Assets
    ↓
Run Packaging Gate
```

This ordering is locked.

## 98. Relationship to Translation

Remote provider:

```text
small packaging impact
```

Local model:

```text
large packaging impact
```

If Gate 4 selects optional local Translation after initial Packaging work, Gate 7 must be rerun with that payload before shipping local mode.

## 99. Current Decision State

```text
Build System
    → dotnet / MSBuild
    → SELECTED

Dependency Manager
    → NuGet
    → SELECTED

Target Framework
    → .NET 10 baseline
    → SELECTED

Initial Runtime Identifier
    → win-x64
    → SELECTED

Primary Publish Direction
    → self-contained
    → BASELINE CANDIDATE

Single File
    → NOT SELECTED

Trimming
    → NOT SELECTED

Native AOT
    → DEFERRED

Packaging Format
    → UNDECIDED

Package Identity
    → UNDECIDED

Installer Tool
    → UNDECIDED

Signing Service
    → UNDECIDED

Update Mechanism
    → UNDECIDED

Model Distribution
    → UNDECIDED

Gate 7
    → BLOCKED BY OCR RUNTIME
```

## 100. Next Technology Document

Next:

```text
04-technology/TESTING.md
```

That document should lock:

- .NET test framework;
- unit-test structure;
- architecture tests;
- contract tests;
- integration tests;
- Windows platform tests;
- benchmark harness strategy;
- OCR benchmark enforcement;
- Translation evaluation strategy;
- end-to-end test layers;
- CI test tiers.

It must distinguish deterministic automated tests from hardware/network/provider-dependent feasibility tests.

## 101. Final Principle

CRAI packaging must package the architecture that wins feasibility testing.

It must not force architecture decisions prematurely.

Canonical:

```text
Build
    → deterministic

Dependencies
    → explicit

Assets
    → owned and versioned

User Data
    → outside immutable application payload

Secrets
    → outside package

OCR Runtime
    → selected before final Packaging

Installer
    → evidence-selected

Signing
    → release-controlled

Upgrade
    → preserves authoritative user data

Package Identity
    → requirement-driven

Release
    → reproducible and testable
```

The final installer is not the technology stack.

It is the delivery representation of a technology stack that has already survived CRAI feasibility gates.
