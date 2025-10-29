# Refactor Plan: Namespace & Directory Changes

## Goal
- Change namespace root from `ErgoX.TokenX` to `ErgoX.TokenX` across entire codebase (src + tests + docs if references?)
- Move directories:
  - `src/SentencePiece` -> `src/SentencePiece`
  - `src/Tiktoken` -> `src/Tiktoken`
  - Adjust project files, namespaces, references accordingly (csproj, Directory.Build.props, etc.)
- Ensure tests pass after refactor (dotnet test with coverage? maybe just dotnet test)

## Steps
1. Update project and solution references
   - Check .sln and .csproj files for project paths; adjust to new directories
2. Move directories physically
3. Update namespaces in all .cs files (source + tests) from `ErgoX.TokenX` to `ErgoX.TokenX`
4. Update `using` directives referencing old namespace
5. Update internal references (partial classes, generated code, etc.)
6. Update tests and other code referencing old namespace
7. Update `InternalsVisibleTo`, assembly name? (Check .csproj) — if assembly name needs change? maybe keep assembly name (should we rename). Probably assembly names remain same unless user wants? Possibly rename root namespace property in csproj. Need to inspect CSProj.
8. Update docs if referencing namespace? lower priority but maybe search/replace.
9. Run `dotnet build` and `dotnet test --configuration Release --verbosity minimal` to confirm.

## Considerations
- Use search & replace carefully (maybe apply regex). Tools: read_file, apply_patch etc. Could use run_in_terminal with `git grep`.
- Ensure `global using` or shared code updated.
- Check `Directory.Build.props` for RootNamespace property.
- Update `docs`? mention instructions? but optional.
- Test reliability: run tests without coverage due time.

## Execution Plan
- Step 0: Inspect solution structure
- Step 1: Update csproj root namespace
- Step 2: Move directories and adjust csproj includes
- Step 3: Namespace rename (maybe use solution-level approach) – manual replacement.
- Step 4: Build/test



