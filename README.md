# David Inna Rework

Rework of Inna. A BepInEx/Harmony mod for Breach Wanderers that patches card
data at runtime — see [CardPatches/](CardPatches) for per-card changes and
[MechanicPatches/](MechanicPatches) for reusable mechanics.

## Building

```
dotnet build
```

## Knowledge files

The [knowledge/](knowledge) folder documents how the patching works. It is
**not guaranteed to be correct**. Treat it as a working reference, not a
source of truth.

- `card modification knowledge.md` — kept by the AI to track how to make
  changes (patterns, gotchas, conventions).
- `card fields and effects reference.md` — for both the user and the AI, to
  know what can be changed and to which values.
