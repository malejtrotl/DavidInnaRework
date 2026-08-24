# Card Modification Knowledge (Breach Wanderers / BepInEx IL2CPP)

How to modify card data (effects, values, text) at runtime via Harmony
patches in this project.

## Project structure

One file per card ID under [CardPatches/](/scripts/c#/bw_patching/DavidInnaRework/CardPatches),
e.g. `CardPatches/Card1432_BottledEctoplasm.cs`. Each file exposes a single
`public static void ApplyMutations(CardData cardData)` — **no
`[HarmonyPatch]` attribute on the card file itself** — that sets every field
the card needs (effects, cost, `_Modifiers`, `_BaseDescription`) in one
place. It's called exactly once, at real game-load time, from
[MechanicPatches/CardDataGameLoadInitializer.cs](/scripts/c#/bw_patching/DavidInnaRework/MechanicPatches/CardDataGameLoadInitializer.cs)
(see "Static mutations" below for why).

Reusable, cross-card behavior belongs in `MechanicPatches/`
(`DavidInnaRework.MechanicPatches` namespace) and is registered in
`Plugin.Load()`, which stays a thin bootstrap: configure any mechanic state
first, then register the initializer:

```csharp
DrawOnCardPlayedRegistry.Register(CardType.Tool, 1401, noFatigue: false);
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CardDataGameLoadInitializerPatch));
```

**Adding a new card:**
1. Create `CardPatches/Card####_Name.cs` with an `internal const int` card ID
   and a `public static void ApplyMutations(CardData cardData)`.
2. Add one line in `CardDataGameLoadInitializerPatch.Postfix`:
   `ApplyIfPresent(cardDataDict, Card####_Name.YourCardId, Card####_Name.ApplyMutations);`
3. If it needs mechanic-specific state (e.g. `DrawOnCardPlayedRegistry.Register(...)`),
   call that from `Plugin.Load()` *before* the initializer is registered.

**Cross-card references** (e.g. a `CreateAndDraw` effect's `_Prefab`, which
must point at another card's real `CardData` instance): give `ApplyMutations`
an extra `CardData` parameter and call it directly from the initializer
instead of through `ApplyIfPresent`. Card `1414` ("Adventurer's Log") does
this for card `1427`'s ("Inkwell and Quill") `CardData`:

```csharp
public static void ApplyMutations(CardData cardData, CardData inkwellAndQuillCardData)

// MechanicPatches/CardDataGameLoadInitializer.cs
if (cardDataDict.ContainsKey(Card1414_AdventurersLog.AdventurersLogCardId)
    && cardDataDict.ContainsKey(Card1427_InkwellAndQuill.InkwellAndQuillCardId))
{
    Card1414_AdventurersLog.ApplyMutations(
        cardDataDict[Card1414_AdventurersLog.AdventurersLogCardId],
        cardDataDict[Card1427_InkwellAndQuill.InkwellAndQuillCardId]);
}
```

Call order between the two doesn't matter: `_Prefab` stores a live object
reference, not a copy, so it reflects whatever the other card's own
`ApplyMutations` does to it regardless of which runs first.

## Static mutations: apply once, at real game-load time

Card values/effects/text are mutated **once**, at real game-load time,
instead of via per-call Harmony prefixes on `GetFinalValue`/`GetEffectCount`/
`GetDescription` (which is how early patches in this project worked, and
still how anything depending on *live match state* has to work — see the
bottom of this section).

**`MetaInventory.Instance.AllCardData` does not work as a mutation target.**
It compiles and runs with no errors, but has no effect in-game — dnSpy
confirms **0 call sites** for its getter anywhere in the assembly. It is not
the live database actual `Card` instances read from.

**What works: `Rift.ResourcesManager.Instance.CardData`** (`Dictionary<int, CardData>`),
confirmed via native pointer comparison to hold the exact same live
`CardData` objects the game uses everywhere. Its getter/setter are plain
field accessors (unpatchable), and `IEnumerator Initialize()` can't be
Postfixed directly (a Postfix on a coroutine-returning method fires when the
state machine is *created*, not when it *finishes*). The fix: patch the
compiler-generated state machine's `MoveNext()`, which *is* a genuine
native-invoked method, and check for `__result == false` (returned exactly
once, on the frame the coroutine finishes) — at that point `_Effects`/
`_EffectValue`/`_Modifiers`/etc. are fully populated.

**Text works too, but with a timing caveat.** At that same `MoveNext`-finish
moment, `_Name`/`_BaseDescription` are still **empty strings** for untouched
cards — the game populates its own default text lazily, inside
`GetDescription`'s first call for that card. But setting `_BaseDescription`
to custom text *before* that first call sticks permanently (the game's own
lazy-population logic must itself check "is this still empty?" first) — so
text mutations belong in the same one-time hook as effect mutations.
**Consequence: a partial `.Replace()` on the existing template does NOT
work here** — it's still empty at this point, so `.Replace()` silently does
nothing. Always write the complete new template as a literal string instead.

```csharp
// MechanicPatches/CardDataGameLoadInitializer.cs
[HarmonyPatch(typeof(ResourcesManager._Initialize_d__12), nameof(ResourcesManager._Initialize_d__12.MoveNext))]
public static class CardDataGameLoadInitializerPatch
{
    private static bool _initialized;

    static void Postfix(ResourcesManager._Initialize_d__12 __instance, bool __result)
    {
        if (_initialized) return;
        if (__result) return; // coroutine still running - wait for the finishing frame.

        _initialized = true;

        var cardDataDict = __instance?.__4__this?.CardData; // __4__this = generated back-reference to ResourcesManager
        if (cardDataDict == null) return;

        // Look up each target card by ID and call its ApplyMutations(CardData).
    }
}
```

**What still has to stay a live per-call/per-event Harmony patch:** anything
depending on match state that can't be precomputed — e.g.
`MechanicPatches/ToolsPlayedThisTurnModifierEmulation.cs`'s "value × tools
played this turn" (`GetFinalValue` Postfix), or
`MechanicPatches/DrawOnCardPlayedRegistry.cs`'s draw trigger on
`CombatManager.UseCard`.

## Field/enum reference

See [card fields and effects reference.md](</scripts/c#/bw_patching/DavidInnaRework/knowledge/card fields and effects reference.md>)
for the full field list (`CardData`, `CardEffect`) and enum members
(`EffectMode`, `EffectTargeting`, `AppliedEffectType`, `EffectModifiers`,
`CardModifiers`, etc). To find names for a new card: open dnSpy on
`...\BepInEx\interop\Assembly-CSharp.dll`, search `CardEffect`/`CardData`/the
relevant enum (method **bodies** are IL2CPP stubs — only **signatures** are
trustworthy), and find a card's numeric ID by logging `_CardID`/`_Name` from
`ResourcesManager.Instance.CardData`.

---

## Editing effects

Every card in this project rebuilds its full `_Effects` list from scratch in
`ApplyMutations()`: `cardData._Effects.Clear()`, then one
`cardData._Effects.Add(new CardEffect { ... })` per effect, in execution
order — rather than mutating whatever the base game happened to load. This
keeps every field explicit and works the same whether the card has one
effect or several. **Assign `CardData = cardData` on every new effect** —
effects loaded from the game's assets already have this owner
back-reference, `new CardEffect` does not, and a missing owner caused a
Unity `NullReferenceException` for a runtime-added `TriggerEffect`.

```csharp
// Card 1432 "Bottled Ectoplasm": two effects, Curse applied then triggered
// on the same target (EffectTargeting.Previous re-targets whatever the
// preceding effect targeted).
cardData._Effects.Clear();

cardData._Effects.Add(new CardEffect
{
    CardData = cardData,
    _Mode = EffectMode.ApplyEffect,
    _AppliedEffect = AppliedEffectType.Curse,
    _Targeting = EffectTargeting.Ranged,
    _EffectValue = CurseApplied,
    _EffectValueUpgraded = CurseAppliedUpgraded,
});

cardData._Effects.Add(new CardEffect
{
    CardData = cardData,
    _Mode = EffectMode.TriggerEffect,
    _AppliedEffect = AppliedEffectType.Curse,
    _Targeting = EffectTargeting.Previous,
    _EffectValue = TriggerCount,
    _EffectValueUpgraded = TriggerCountUpgraded,
});
```

No idempotency guard is needed anywhere in this pattern — `ApplyMutations`
is called exactly once per game process (`CardDataGameLoadInitializerPatch`'s
`_initialized` flag).

**Conditional/chained effects:** `_Modifiers` takes one `EffectModifiers`
value per effect (not a flags mask); most are self-contained, but ones like
`OnlyIfTargetHasEffect` pair with `_ConditionEffect`. `Condition` gates an
effect on the *preceding* effect having succeeded (no `_ConditionEffect`
needed) — see `CardPatches/Card1408_FranticScouring.cs` (`Discard` followed
by a `Condition`-gated `CreateTool`). `EffectTargeting.Previous` chains
targeting the same way — see `Card1409_Investigate.cs` (one `Ranged`
CreateTool, then two debuff-gated `CreateTool` effects targeting `Previous`).

**Order matters:** `_Effects` executes in list order, so build effects in the
order they should run. `Card1411_ResourcefulStrike.cs` marks two effects
(`IncreaseDamage` and `IncreaseStrikeDamage`) with the shared
`ScalePerToolPlayed` marker convention before its `Damage` effect, so the
damage boost applies before the hit lands. Note that this marker convention
(`_Modifiers`/`_ConditionEffect`) works with *any* `_Mode` — the emulation
patch only inspects those two fields, not `_Mode` — which is why this card
could change from scaling a temporary Powerful buff to scaling
`IncreaseDamage` without touching the shared mechanic. Also note an effect
can exist with no corresponding `{N}` placeholder in `_BaseDescription` (its
value simply isn't shown), as long as the placeholders you *do* use still
match each referenced effect's position in `_Effects`.

## Editing card text and name

`CardData.GetDescription(...)` reads `_BaseDescription` (a template with
`{0}`/`{1}`/... placeholders) and fills it with live/buffed values itself —
set the whole string once in `ApplyMutations()`, keeping the placeholders so
the game still computes the numbers:

```csharp
cardData._BaseDescription = "Give Curse ({0}) to any enemy and trigger it {1} time.";
```

**Style rule: parenthesize a buff/debuff's applied value, not other
numbers.** When a `{N}` belongs to an `ApplyEffect*`/`TriggerEffect` effect
with a named `_AppliedEffect` (`Burn`, `Curse`, `Weak`, `Doom`, `Powerful`,
etc.), wrap it in parens: `Burn ({1})`, `Doom ({0})`. Everything else
(damage, counts, mana, cost changes, non-`AppliedEffectType` modes like
`ModifyAllStatuses`/`IncreaseDamage`) has no parens. Matches the base game's
own convention — see `Card1422_FireBomb.cs`, `Card1426_UnstableDarkstone.cs`,
`Card1432_BottledEctoplasm.cs`.

`_Name` (display name) follows the identical pattern — set once in
`ApplyMutations()`:
```csharp
cardData._Name = "New Name";
```

A Postfix on `GetDescription`'s `__result` does **not** work as an
alternative: a Prefix setting `__result` is silently overwritten by the
original method running afterward, and by the time a Postfix sees
`__result` the placeholders are already plain text with no reliable way to
map numbers back to effects.

## Effect counts, upgrades, and temporary effects

- `_EffectValue`/`_EffectValueUpgraded` = amount; `_EffectCount`/
  `_EffectCountUpgraded` = repeated hits/triggers. A `2x3` multi-hit uses
  `_EffectValue = 2`, `_EffectCount = 3`.
- Make a card upgradeable: `cardData._Modifiers &= ~CardModifiers.NoUpgrade`
  (bitwise clear, not overwrite), then set `_CostUpgraded`/`_EffectValueUpgraded`.
- `EffectMode.ApplyEffectThisTurn` + `_AppliedEffect = AppliedEffectType.Powerful`
  applies temporary Powerful.
- `EffectTargeting.Melee` = first enemy; `Ranged` = normal enemy targeting;
  `Self` = caster; `Previous` = re-target whatever the prior effect targeted.

---

## Reusable mechanic patches

**Tools played this turn** (`MechanicPatches/ToolsPlayedThisTurnModifierEmulation.cs`):
emulates a scaling modifier without a new enum member. Mark an effect with
`_Modifiers = EffectModifiers.ScalePerStrikePlayed; _ConditionEffect = AppliedEffectType.COUNT;`
in `ApplyMutations()` (static). The live computation — incrementing
`ToolsPlayedThisTurn` on `CombatManager.UseCard`, resetting it on
`StartPlayerTurn`, and multiplying an effect's value by it in a
`GetFinalValue` Postfix — stays a live patch, since it genuinely depends on
match state.

**Draw a card when a card of a given `CardType` is played**
(`MechanicPatches/DrawOnCardPlayedRegistry.cs`): a generic registry
supporting any number of independent triggers (different `CardType`s,
different targets, per-target fatigue behavior):

```csharp
// Plugin.Load(), before the initializer is registered:
DrawOnCardPlayedRegistry.Register(CardType.Tool, 1401, noFatigue: false);
```

Ownership lives entirely on the plugin's own side (a `Dictionary<CardType, List<Target>>`),
never permanently on `CardData`. `CardModifiers` has exactly one unclaimed
bit (`DrawMarker`) and no room for a second — so every registered target
shares it, but only *transiently*: on `CombatManager.UseCard`, the registry
sets `DrawMarker` (+ `NoFatigueSpecialDraw` if configured) only on the cards
registered for the `CardType` that just played, calls
`Entity.DrawCardsWithModifier(DrawMarker)`, then immediately clears both
bits again. This replaced an earlier one-target-per-mechanic design where
the bit was left permanently set — which broke the moment two *different*
target cards, under *different* trigger conditions, were both active at
once (`DrawCardsWithModifier` can't tell which trigger a card "belongs to").
Keeping the bit clear at rest also leaves it free for another plugin to
reuse for something unrelated, outside the brief window a draw is actually
resolving.

---

## General checklist for adding a new card patch

1. Find the card's `_CardID` and effect(s) via dnSpy or logging
   `ResourcesManager.Instance.CardData`.
2. Decide: values, new/reordered effects, text, name, and/or live-state
   behavior (which stays a separate per-call/per-event patch alongside the
   static `ApplyMutations()`).
3. Write `CardPatches/Card####_Name.cs` with `ApplyMutations(CardData cardData)`
   mutating `_Effects`, `_Cost`/`_CostUpgraded`, `_Modifiers`, and/or
   `_BaseDescription`/`_Name` as needed.
4. Register it: `ApplyIfPresent(cardDataDict, Card####_Name.YourCardId, Card####_Name.ApplyMutations);`
   in `CardDataGameLoadInitializer.cs`.
5. If it needs mechanic-specific state (e.g.
   `DrawOnCardPlayedRegistry.Register(cardType, cardId, noFatigue)`), call
   that from `Plugin.Load()` *before* the initializer is registered.
6. Build (`dotnet build`) and deploy with `build_and_deploy.bat`, then test
   in-game.
