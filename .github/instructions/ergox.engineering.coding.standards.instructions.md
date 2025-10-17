---
applyTo: "**"
---

# ErgoX Vecrax Coding Standards for AI Agents

## 🎯 EXIT CRITERIA (Acceptance Gates)
- ✅ **ZERO errors** in build output
- ✅ **ZERO warnings** in build output (except documented exceptions)
- ✅ All new code passes security analyzers (SCSxxxx, S-Security)
- ✅ No resource leaks (IDISP001-006 compliance)
- ✅ Async/await patterns correct (VSTHRD103, MA0045 compliance)

## ⚡ CRITICAL RULES (Build Blockers - ERRORS)

### Security & Reliability
- **S-Security**, **S-Bug**, **SCSxxxx**: Any security/bug finding MUST be fixed
- **IDISP001-006**: All `IDisposable` resources MUST be disposed (use `using`/`await using`)
- **MA0045**: NO `.Wait()`, `.Result`, or blocking in async code
- **VSTHRD103**: Use async methods; never sync-over-async
- **CA2000**: Dispose objects before losing scope

### Coding Patterns
- **Nullable**: C# nullable reference types enabled; annotate correctly (`?`, `!`)
- **Async naming**: Public async methods MUST end with `Async`
- **No secrets**: Zero hard-coded credentials/keys/connection strings (SCS checks)

## ⚠️ ENFORCED STANDARDS (Warnings - Fix Before PR)

### Formatting (4-space indent, UTF-8, CRLF)
```csharp
namespace ErgoX.Vecrax.Feature  // using inside namespace
{
    public class Example
    {
        private readonly string _field;  // underscore prefix OK

        public void Method()  // brace same line
        {
            var list = new List<int> { 1, 2 };  // var when obvious
        }
    }
}
```

### Naming
- Types/properties/methods: **PascalCase**
- Parameters/locals: **camelCase**
- Private fields: **_camelCase** (underscore prefix allowed)
- Async methods: **MethodAsync** suffix

### StyleCop Spacing (SA1000-SA1028)
- Space after comma: `Method(a, b, c)`
- Space before opening parenthesis in control flow: `if (condition)`
- Space before tuple return type: `public static (int X, int Y) GetPoint()`
- NO space before method call parenthesis: `Method()`

### Modern C# Patterns
- Prefer `is not null` over `!= null`
- Use switch expressions when clearer
- Use expression-bodied members for simple properties: `public int Value => _value;`
- Use collection expressions: `var list = [1, 2, 3];` (C# 12)

## 📋 DOCUMENTED EXCEPTIONS (Acceptable Warnings)

### Configuration Project: 0 warnings expected ✅

### RAG.Abstractions Project: 0 warnings expected ✅

### Globally Suppressed (Suggestions)
These are **suggestions** (informational) - not blockers:
- **S107**: Too many parameters (records with many immutable properties)
- **S1541**: Method complexity (validators need comprehensive checks)
- **S3776**: Cognitive complexity (refactor opportunistically)
- **S4055**: Literal parameters (performance over localization)
- **MA0165**: Use interpolated string (plain strings OK)
- **CA1859**: Concrete types (interfaces preserve abstraction)

### Disabled Rules
- **SA1600-SA1633**: XML docs not required for private/internal
- **SA1402**: Multiple types per file allowed
- **SA1101**: `this.` prefix not required
- **SA1309**: `_field` underscore prefix allowed
- **SA1649**: File naming (disabled - StyleCop incompatible with C# 11 records)

## 🚀 CODE GENERATION CHECKLIST

### Before Generating Code
1. ✅ Check if async context → use `async`/`await`, `ConfigureAwait(false)` in libraries
2. ✅ Check if disposables → add `using`/`await using`
3. ✅ Check if external input → validate/sanitize (prevent injection)
4. ✅ Check if sensitive data → never log PII/secrets

### After Generating Code
1. ✅ Build project: `dotnet build --no-incremental`
2. ✅ Verify 0 errors, 0 warnings
3. ✅ Check disposal paths for `IDisposable` objects
4. ✅ Verify async patterns (no blocking)

## 📦 PROJECT STRUCTURE RULES

### File Organization
- One primary type per file (nested helpers OK)
- File ends with single newline, no trailing whitespace
- UTF-8 encoding, CRLF line endings

### Member Ordering (SA1201 relaxed to suggestion)
```csharp
// Preferred order (not enforced):
private readonly fields
constructors
public properties
public methods
private methods
nested types
```

## 🔐 SECURITY REQUIREMENTS (Non-Negotiable)

### Input Validation
- Validate all external input (files, HTTP, CLI args)
- Use parameterized queries (ORM or `@param` syntax)
- Never concatenate user input into commands/SQL/shell

### Secrets Management
- Use `IConfiguration`, Azure Key Vault, or environment variables
- NEVER commit secrets in source code
- Redact sensitive data in logs

### Crypto & Randomness
- Use approved APIs: `RandomNumberGenerator`, `Aes`, `RSA`
- NO weak crypto: MD5, SHA1, DES
- NO `Random()` for security contexts

## 🧪 TESTING EXPECTATIONS

### xUnit Patterns
- Deterministic tests (no ambient time/state)
- Async tests: `Task Method_Should_Behavior_When_Condition()`
- Dispose resources in test fixtures (`IDisposable`)

## 🛠️ ANALYZER SUMMARY

| Category | Severity | Examples | Action |
|----------|----------|----------|--------|
| **Security** | ERROR | S-Security, SCSxxxx | Fix immediately |
| **Bugs** | ERROR | S-Bug, IDISP | Fix immediately |
| **Async** | ERROR | VSTHRD103, MA0045 | Fix immediately |
| **Style** | SUGGESTION | SA, RCS-style | Optional fixes |
| **Complexity** | SUGGESTION | S3776, S1541 | Refactor opportunistically |

## 💡 QUICK REFERENCE

### DO ✅
```csharp
// Async all the way
public async Task<Result> ProcessAsync(CancellationToken ct)
{
    await using var resource = CreateResource();
    return await ComputeAsync(ct).ConfigureAwait(false);
}

// Dispose resources
using var stream = File.OpenRead(path);

// Modern patterns
if (value is not null) { }
var result = expression switch { ... };

// Safe tuple spacing
public static (int X, int Y) GetPoint() => (0, 0);
```

### DON'T ❌
```csharp
// Blocking in async
var result = ProcessAsync().Result;  // ERROR: MA0045

// Leaking disposables
var stream = File.OpenRead(path);    // ERROR: IDISP001
return stream;

// Missing space before tuple
public static(int X, int Y) GetPoint()  // WARNING: SA1008

// Hard-coded secrets
var connStr = "Server=...;Password=secret";  // ERROR: SCS
```

## 🎓 REMEMBER
1. **Build must pass with 0 errors, 0 warnings**
2. **Security findings are non-negotiable**
3. **Dispose all disposables**
4. **Async must be end-to-end (no blocking)**
5. **Validate external inputs**
6. **Never commit secrets**

---
**Last Updated**: 2025-10-01
**Analyzer Versions**: SonarAnalyzer.CSharp, StyleCop.Analyzers, Meziantou.Analyzer, IDisposableAnalyzers, Microsoft.VisualStudio.Threading.Analyzers, SecurityCodeScan.VS2019
- [**ERGX00014**] | [**SA1200**] Place **using directives inside the namespace** (preferred). *(We keep SA1200 as suggestion; follow this convention for new code.)*
- [**ERGX00015**] | [**IDE/CA**] Remove unused `using` directives automatically (format-on-save / IDE0005).
- [**ERGX00016**] | [**IDE/CA**] Global usings are allowed for ubiquitous framework namespaces; avoid domain-specific globals.
- [**ERGX00017**] | [**IDE/CA**] Use `using X = Y;` aliases rarely; add a comment explaining why when used.

---

## **File Organization & Layout**
- [**ERGX00018**] | [**SA1402**] **One type per file is not required**; multiple small related types per file are allowed. *(SA1402 disabled)*
- [**ERGX00019**] | [**IDE/CA**] End each file with a single newline; no trailing whitespace.
- [**ERGX00020**] | [**IDE/CA**] Use **UTF‑8 (no BOM)** for source files.
- [**ERGX00021**] | [**IDE/CA**] Within a type, keep consistent order: **fields → ctors → properties → methods → nested types**.

---

## **Formatting & Spacing**
- [**ERGX00022**] | [**IDE/CA**] Indentation is **4 spaces**, never tabs.
- [**ERGX00023**] | [**IDE/CA**] Preferred max line length **120 chars** (allow exceptions for URLs/attributes).
- [**ERGX00024**] | [**IDE/CA**] Always use **braces** for control statements; opening brace on the **same line** (IDE style).
- [**ERGX00025**] | [**IDE/CA**] Use a single blank line between members; avoid multiple consecutive blank lines.
- [**ERGX00026**] | [**IDE/CA**] One space after commas and after `:` in object/collection initializers.

---

## **Naming**
- [**ERGX00027**] | [**IDE/CA**] Types, properties, methods, and public constants use **PascalCase**.
- [**ERGX00028**] | [**IDE/CA**] Parameters and local variables use **camelCase**.
- [**ERGX00029**] | [**SA1309**] Private fields **may** use `_camelCase` (underscore prefix). *(SA1309 disabled)*
- [**ERGX00030**] | [**VSTHRDxxx**] Async methods **must** end with **`Async`** suffix when public API or cross-boundary.

---

## **Language & Idioms**
- [**ERGX00031**] | [**IDE/CA**] Use `var` when the type is obvious; otherwise prefer explicit type.
- [**ERGX00032**] | [**IDE/CA**] Prefer **expression-bodied** members for trivial properties/methods.
- [**ERGX00033**] | [**IDE/CA**] Prefer **switch expressions** over long `if/else` chains when clearer.
- [**ERGX00034**] | [**MA0016**] Prefer modern pattern matching (e.g., `is not null`) over legacy constructs.
- [**ERGX00035**] | [**IDE/CA**] Enable **nullable reference types** and annotate appropriately.

---

## **Async & Threading**
- [**ERGX00036**] | [**MA0045**] Do **not** block on async (`.Result`, `.Wait()`); use fully async flows.
- [**ERGX00037**] | [**VSTHRD103**] Call async methods where available; avoid sync-over-async.
- [**ERGX00038**] | [**VSTHRDxxx**] Avoid `async void` except for event handlers.
- [**ERGX00039**] | [**MA0004**] In libraries, prefer `ConfigureAwait(false)` where context capture is unnecessary.
- [**ERGX00040**] | [**VSTHRD110**] Follow JTF and threading guidance to avoid deadlocks; do not block UI thread.
- [**ERGX00041**] | [**VSTHRD200**] Address analyzer suggestions that improve thread safety and responsiveness.

---

## **Exceptions & Reliability**
- [**ERGX00042**] | [**Sxxxx**] Do not swallow exceptions silently; log or rethrow with context (**Bug/Security → error**).
- [**ERGX00043**] | [**Sxxxx**] Do not use exceptions for control flow (Code Smell → warning).
- [**ERGX00044**] | [**IDE/CA**] Throw specific exception types with meaningful messages.
- [**ERGX00045**] | [**Sxxxx**] Validate inputs to public methods; use guard clauses where appropriate.

---

## **Security**
- [**ERGX00046**] | [**SCSxxxx**] No hard-coded secrets, keys, or credentials.
- [**ERGX00047**] | [**SCS0001**] Prevent command injection; never concatenate untrusted input into shell/CLI.
- [**ERGX00048**] | [**SCS0002**] Prevent XSS; correctly encode/sanitize outputs for the target context.
- [**ERGX00049**] | [**SCSxxxx**] Use parameterized queries / ORMs to prevent SQL injection.
- [**ERGX00050**] | [**SCSxxxx**] Avoid insecure crypto and weak randomness; use approved APIs only.
- [**ERGX00051**] | [**Sxxxx**] Do not log sensitive data (PII/secrets); redact when necessary.

---

## **Resource Management & IDisposable**
- [**ERGX00052**] | [**IDISP001**] Dispose created `IDisposable` instances (`using`/`await using`).
- [**ERGX00053**] | [**IDISP002**] Owning types must dispose disposable members they own.
- [**ERGX00054**] | [**IDISP004**] Do not ignore created disposables; ensure a disposal path exists.
- [**ERGX00055**] | [**IDISP006**] Do not assign disposables to fields with narrower lifetimes; align lifetimes correctly.
- [**ERGX00056**] | [**IDE/CA**] Implement the dispose pattern correctly when owning unmanaged resources (**CA1063/CA2000** complement IDISP).

---

## **Testing (xUnit)**
- [**ERGX00057**] | [**xUnit1xxx**] Follow xUnit analyzer guidance (naming, asserts, facts/theories best practices).
- [**ERGX00058**] | [**xUnit1xxx**] Tests should be deterministic and avoid ambient state/time; prefer async waits over `Thread.Sleep`.

---

## **Style Policy (StyleCop — Partial)**
- [**ERGX00059**] | [**SA1600**] XML docs are **not required** for private/internal members. *(Disabled)*
- [**ERGX00060**] | [**SA1601**] XML docs are **not required** for partial elements. *(Disabled)*
- [**ERGX00061**] | [**SA1633**] File headers are **not required**. *(Disabled)*
- [**ERGX00062**] | [**SA1101**] `this.` qualification is **not required**. *(Disabled)*
- [**ERGX00063**] | [**SA1200**] Using directives **inside namespace** is **preferred** but not enforced as error. *(Suggestion)*
- [**ERGX00064**] | [**SA1201**] Maintain a logical member ordering for readability. *(Enabled at warning if adopted)*
- [**ERGX00065**] | [**SA1309**] Underscore `_field` naming for private fields is allowed. *(Disabled rule)*

---

## **SonarAnalyzer Policy**
- [**ERGX00066**] | [**S3776**] Cognitive complexity findings are **suggestions**; refactor opportunistically.
- [**ERGX00067**] | [**S125**] Do not commit commented‑out code (detected as a smell).
- [**ERGX00068**] | [**S1301**] “Prefer switch over if” is **suggestion**, not mandatory.
- [**ERGX00069**] | [**Sxxxx**] Sonar **Security**/**Bug** rules surface as **errors**.

---

## **Meziantou Policy**
- [**ERGX00070**] | [**MA0045**] No blocking calls in async context.
- [**ERGX00071**] | [**MA0004**] Use `ConfigureAwait(false)` in library code where appropriate.
- [**ERGX00072**] | [**MA0016**] Prefer modern patterns (`is not null`, pattern matching) to improve clarity and correctness.

---

## **Roslynator Policy**
- [**ERGX00073**] | [**RCSxxxx**] Apply refactorings that clearly improve clarity/safety; they SHOULD NOT block merges.
- [**ERGX00074**] | [**RCSxxxx**] Avoid introducing redundant code constructs (redundant casts/usings/empty initializers).

---

## **Built-in .NET / IDE Style**
- [**ERGX00075**] | [**IDE/CA**] Prefer no qualification (`this.`) for members unless needed for disambiguation.
- [**ERGX00076**] | [**IDE/CA**] Always use braces (`csharp_prefer_braces = true`).
- [**ERGX00077**] | [**IDE/CA**] Prefer **switch expressions** where they simplify code.
- [**ERGX00078**] | [**IDE/CA**] Remove unused members/usings; simplify names/expressions automatically when safe (IDE00xx).

---

## **CI & Build Behavior**
- [**ERGX00079**] | [**IDE/CA**] In CI, analyzer **errors** fail the build; warnings do not unless explicitly escalated.
- [**ERGX00080**] | [**SCSxxxx**] Run SecurityCodeScan in **CI/Release** to reduce local build overhead.

---

## **Suppressions & Exceptions**
- [**ERGX00081**] | [**IDE/CA**] Scope suppressions to the smallest area (line/member) and include a justification.
- [**ERGX00082**] | [**IDE/CA**] Use `GlobalSuppressions.cs` sparingly; link to a tracking issue when used.

---

## **Documentation**
- [**ERGX00083**] | [**IDE/CA**] Public APIs should have XML documentation; internal/private docs only when logic is complex.
- [**ERGX00084**] | [**IDE/CA**] Comments explain **why**, not restate **what**.

---

## **Performance & Maintainability**
- [**ERGX00085**] | [**IDE/CA**] Prefer readability over micro-optimizations unless a proven hot path.
- [**ERGX00086**] | [**IDE/CA**] Use named constants/enums instead of magic numbers.
- [**ERGX00087**] | [**IDE/CA**] Consider Span/Memory/pooling only with clear correctness and measurable benefit.

---

## **Repository Hygiene**
- [**ERGX00088**] | [**S125**] No commented‑out code in commits.
- [**ERGX00089**] | [**IDE/CA**] Run `dotnet format` before committing to apply safe style fixes.
- [**ERGX00090**] | [**IDE/CA**] Avoid unrelated whitespace‑only churn in PRs.

---

## **Test/Prod Parity & Configuration**
- [**ERGX00091**] | [**SCSxxxx**] Secrets/connection strings must come from secure configuration providers, not source.
- [**ERGX00092**] | [**SCSxxxx**] Validate and sanitize any external input before use (files, query strings, headers).

---

## **Generator Summary (for Copilot)**
- [**ERGX00093**] | [**Sxxxx**] Do not introduce **errors** for Sonar **Security**/**Bug** rules.
- [**ERGX00094**] | [**IDISPxxxx**] Ensure every created disposable is disposed and lifetimes align.
- [**ERGX00095**] | [**VSTHRDxxx**] Avoid sync-over-async and `async void`; prefer end‑to‑end async.
- [**ERGX00096**] | [**MAxxxx**] In libraries, add `ConfigureAwait(false)` where context capture is unnecessary.
- [**ERGX00097**] | [**SA1200**] Put `using` directives **inside** the namespace and keep imports minimal.
- [**ERGX00098**] | [**IDE/CA**] Follow naming/formatting: 4‑space indent, 120 cols, braces on same line, no trailing spaces.
- [**ERGX00099**] | [**SCSxxxx**] Never embed secrets; never concatenate untrusted input into commands/SQL.

---
