---
applyTo: "**"
---

# ErgoX Native Interop & Performance Guidance

## Architectural Priorities
- Favor deterministic startup paths: load the native runtime once, validate symbols, and surface initialization failures immediately.
- Keep managed abstractions thin; expose native capabilities without hiding critical tuning knobs needed for large-model deployments.
- Treat cross-boundary allocations as high-cost operations—prefer span-based APIs and pooled buffers to avoid GC pressure during inference.

## Interop Safety
- Mirror native structs exactly; document the upstream commit hash that the layout matches and add unit tests that assert `sizeof` parity when possible.
- Encapsulate every `GCHandle` in a `try/finally` or `using` block to guarantee release even when marshaling fails.
- Always validate external strings and pointers before dereferencing; throw managed exceptions with actionable guidance instead of letting AVs propagate.
- Avoid implicit conversions between signed/unsigned native types; cast deliberately and guard against overflow when projecting into managed integers.

## Performance First
- Pin memory only for the minimum window necessary; long-lived pins should be backed by `ArrayPool<T>` or native allocations to avoid LOH fragmentation.
- Batch native calls whenever possible and surface APIs that accept spans/slices to minimize transition overhead.
- Cache capability probes and offload-planning metadata; avoid repeating syscalls when serving high-throughput inference workloads.
- Prefer `ref struct`/`Span<T>` patterns for hot paths; fall back to arrays only when interoperability demands contiguous buffers.

## Diagnostics & Observability
- Provide structured logging around initialization, device detection, and long-running native calls with duration metrics.
- Surface native error codes and log correlated managed stack context to simplify cross-team debugging.
- Add health probes for resource exhaustion scenarios (GPU memory, locked pages) and document remediation steps alongside the API.

## Testing & Tooling
- Add smoke tests that exercise every exported native symbol we bind to, ensuring the bindings break fast when llama.cpp changes.
- Use stress tests with extreme tensor sizes and multi-GPU topologies to validate marshal logic under backpressure.
- Integrate static analyzers (CA, IDISP, SCS) and run them on every interop touchpoint; treat new warnings as regressions.

## Deployment Guidance
- Publish a compatibility matrix (OS, GPU driver, backend version) and enforce runtime checks before enabling advanced backends.
- Ship fallbacks for optional features (NUMA, RPC, distributed decoding) so downgrade paths are deterministic.
- Version native assets separately from managed packages; communicate required upgrades in release notes and upgrade guides.
