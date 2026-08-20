# Card Modification Knowledge (Breach Wanderers / BepInEx IL2CPP)

This document explains how to modify card data (numbers, multiple numbers, and
text) at runtime using Harmony patches in this project, with real examples
taken from the project's card patch files.

## Project structure

Patches are organized **one file per card ID**, all under
[CardPatches/](/scripts/c#/bw_patching/DavidInnaRework/CardPatches):

- `CardPatches/Card0052_Block.cs` — all patches for Card ID 52 ("Block").
- `CardPatches/Card1003_IceBlast.cs` — all patches for Card ID 1003 ("Ice Blast" / "Piercing Ice").

Each file can contain multiple Harmony patch classes (value patches, text
patches, name patches, new-effect patches, etc.) as long as they all target
the same card ID. `Plugin.cs` stays a thin bootstrap file — it just imports
`DavidInnaRework.CardPatches` and registers every patch class from every file
in `Load()`, grouped with a comment per card:

```csharp
// Card 0052 "Block" (see CardPatches/Card0052_Block.cs)
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ShieldCardBuffPatch));
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BlockGrantsToughPatch));
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BlockDescriptionPatch));

// Card 1003 "Ice Blast" (see CardPatches/Card1003_IceBlast.cs)
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IceBlastPatch));
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IceBlastDescriptionPatch));
new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IceBlastNamePatch));
```

**When adding a new card:** create a new file `CardPatches/Card####_Name.cs`
with the card's ID in the filename, put every patch class for that card in
it, and register the new class(es) in `Plugin.Load()` under a new comment
block for that card. This keeps each card's logic self-contained and easy to
find, and avoids one giant file as more cards get patched.

## Background / why we patch this way

- Card data (values, conditions, etc.) is data-driven — stored in the game's
  Unity assets, not hardcoded in code. We don't edit the asset files directly;
  instead we patch it at runtime with Harmony.
- The key hook point is `CardEffect.GetFinalValue(...)` — this is called
  whenever the game needs the actual value of a card effect (for display,
  combat calculation, tooltips, etc.).
- We patch it with a **Prefix** that mutates the underlying fields directly
  (`_EffectValue`, `_EffectValueUpgraded`, `_Modifiers`, `_ConditionEffect`,
  etc.) instead of just changing the return value.
  - **Important:** If you only change the *return value* (e.g. via a Postfix
    with `ref __result`), the underlying field is untouched. Other systems
    (like the UI) compare the "current" value against the "base" value stored
    in the field and will show a green "modified/buffed" highlight, or may use
    the un-patched value somewhere else. Mutating the field directly keeps
    every code path consistent.
- Each `CardEffect` is a **separate object** in the card's `_Effects` list.
  `GetFinalValue` fires once per effect instance, so if a card has multiple
  numeric effects, your patch runs multiple times (once per effect) and needs
  a way to tell the effects apart (see "Editing multiple numbers" below).

## Relevant data model (namespace `Rift`)

- `MetaInventory.Instance.AllCardData` — `List<CardData>`, the master card
  registry (read-only in practice — its setter can't be Harmony-patched
  because IL2CPP field-accessor setters are not patchable).
- `CardData`:
  - `_CardID: int` — the card's numeric ID.
  - `_Name` / `CardName: string`
  - `_Effects: List<CardEffect>` — the card's effects.
  - `_BaseDescription: string` — writable raw template string (still contains
    the game's own number placeholders). `GetDescription()` reads this field
    and populates the placeholders with live/buffed values each time it's
    called. Editing this field via a Prefix on `GetDescription` (before the
    original method runs) is the correct way to change tooltip text — see
    "Editing card text" below.
  - `GetDescription(Card card, bool upgraded, bool highlightUpgrade, string languageOverride): string`
- `CardEffect`:
  - `_EffectValue: int`, `_EffectValueUpgraded: int` — the base/upgraded
    numeric values.
  - `CardData: CardData` — back-reference to the owning card (use this to
    check `_CardID`).
  - `_Modifiers: EffectModifiers` — a single enum value (not a flag list)
    describing a conditional/scaling modifier, e.g. `OnlyIfTargetHasEffect`,
    `OnlyIfTargetHasShield`, `OnlyIfTargetHasNoShield`, `OnlyIfTargetHas10PlusShield`.
  - `_ConditionEffect: AppliedEffectType` — companion value used by modifiers
    like `OnlyIfTargetHasEffect` (e.g. `Frozen`). Not needed for self-contained
    modifiers like `OnlyIfTargetHasNoShield`.
  - `_StatusType: StatusType` — e.g. `Frost`, `Arcane`, `Shock`, used to
    identify status-applying effects (as opposed to damage effects).
  - `GetFinalValue(Card, Entity, bool, bool, bool): float` — the method we hook.

## How to find these names for a new/different card

1. Open dnSpy pointed at the game's interop DLL:
   `C:\Program Files (x86)\Steam\steamapps\common\Breach Wanderers\BepInEx\interop\Assembly-CSharp.dll`
2. Search for the relevant class (`CardEffect`, `CardData`, enums like
   `EffectModifiers`, `AppliedEffectType`, `StatusType`) and note field/method
   names. Remember: method **bodies** in interop DLLs are IL2CPP stub code,
   not real logic — only the **signatures** (names/types) are trustworthy.
3. To find a card's numeric ID, either inspect `CardData` instances at runtime
   (e.g. temporarily log `_CardID` + `_Name` for all cards in
   `MetaInventory.Instance.AllCardData`), or use known IDs from testing.

---

## 1. Editing a single number on a card

Example: Card ID `52`, bump its shield value.

```csharp
// Card with CardID 0052: bumps its shield effect value from 12 to 50.
// We patch CardEffect.GetFinalValue (called whenever the game computes the
// actual value of a card effect, e.g. for display or applying the effect)
// with a Prefix that fixes up the underlying _EffectValue field the first
// time it sees the target card. Because we mutate the actual field (not just
// the return value), every downstream system reads the new value directly,
// so no "modified" indicator (e.g. green highlight) is triggered.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class ShieldCardBuffPatch
{
    private const int TargetCardId = 52;
    private const int NewValue = 50;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != TargetCardId) return;
        __instance._EffectValue = NewValue;
        __instance._EffectValueUpgraded = NewValue * 2;
    }
}
```

Steps:
1. `[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]` on a
   static class.
2. Add a static `Prefix(CardEffect __instance)` method.
3. Get the owning card via `__instance.CardData`, guard on `_CardID`.
4. Set `_EffectValue` (base) and `_EffectValueUpgraded` (upgraded/leveled
   version) directly.
5. Register the patch class in `Plugin.Load()`:
   `new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ShieldCardBuffPatch));`

---

## 2. Editing multiple numbers on a card

When a card has more than one numeric effect (e.g. Card ID `1003` "Ice Blast":
Frost application + bonus damage if the target is Frozen), the Prefix runs
once per `CardEffect` instance — you must distinguish which effect you're
looking at using its other fields (`_Modifiers`/`_ConditionEffect` for
conditional effects, `_StatusType` for status-applying effects, etc.).

```csharp
// Card 1003 "Ice Blast" has two effects: applying Frost, and dealing bonus
// damage if the target is already Frozen. We distinguish them via
// _Modifiers/_ConditionEffect (the "only if target has effect: Frozen" gate)
// vs _StatusType (Frost) on the plain status-applying effect.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class IceBlastPatch
{
    private const int IceBlastCardId = 1003;
    private const int FrostApplied = 5;
    private const int DamageIfFrozen = 10;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != IceBlastCardId) return;

        bool isConditionalDamageEffect =
            __instance._Modifiers == EffectModifiers.OnlyIfTargetHasEffect
            && __instance._ConditionEffect == AppliedEffectType.Frozen;

        if (isConditionalDamageEffect)
        {
            __instance._EffectValue = DamageIfFrozen;
            __instance._EffectValueUpgraded = DamageIfFrozen * 2;
        }
        else if (__instance._StatusType == StatusType.Frost)
        {
            __instance._EffectValue = FrostApplied;
            __instance._EffectValueUpgraded = FrostApplied * 2;
        }
    }
}
```

Notes:
- It can look like "only one value ever changes" when testing casually, but
  this is because each effect is a *separate* `CardEffect` object — the Prefix
  fires independently for each one across separate calls, so both values do
  get set correctly overall.
- You can go further and rewrite the *condition itself*, not just the value.
  Since `_Modifiers` and `_ConditionEffect` are plain writable fields, you can
  reassign them to a different modifier (e.g. change "if target is Frozen" to
  "if target has no Shield"):

```csharp
bool isConditionalDamageEffect =
    (__instance._Modifiers == EffectModifiers.OnlyIfTargetHasEffect
        && __instance._ConditionEffect == AppliedEffectType.Frozen)
    || __instance._Modifiers == EffectModifiers.OnlyIfTargetHasNoShield; // idempotency check

if (isConditionalDamageEffect)
{
    // Rewrite the condition: "if frozen" -> "if no shield"
    __instance._Modifiers = EffectModifiers.OnlyIfTargetHasNoShield;
    __instance._ConditionEffect = AppliedEffectType.NONE; // not needed for this modifier

    __instance._EffectValue = DamageIfNoShield;
    __instance._EffectValueUpgraded = DamageIfNoShield * 2;
}
```

- The idempotency check (`|| __instance._Modifiers == EffectModifiers.OnlyIfTargetHasNoShield`)
  matters because the Prefix runs on every call to `GetFinalValue`, including
  calls *after* you've already rewritten the modifier — without it, the
  `else if (_StatusType == StatusType.Frost)` branch could wrongly match once
  `_Modifiers` no longer equals the original `OnlyIfTargetHasEffect` value.
- `EffectModifiers` is a single enum value per effect (not a flags list), and
  most of them are self-contained (no companion value needed) except ones like
  `OnlyIfTargetHasEffect` which pair with `_ConditionEffect`.

---

## 3. Editing card text (tooltip description)

The final tooltip text comes from `CardData.GetDescription(Card card, bool upgraded, bool highlightUpgrade, string languageOverride)`.
Internally, this method reads the **raw template string** stored in
`CardData._BaseDescription` (which still contains the game's own number
placeholders) and populates it with the live/buffed values itself — the same
values `CardEffect.GetFinalValue` would compute (accounting for upgrades,
active combat buffs/debuffs, etc.).

**Correct approach: patch `_BaseDescription`, not the returned string.**
Use a **Prefix** on `GetDescription` that edits `_BaseDescription` right
before the original method runs. Do a targeted phrase replace (not a full
rewrite) so the placeholder tokens for the numbers are left completely
untouched — the game still fills them in afterwards, exactly the same way it
always did, buffs and all. This means you never need to duplicate the
value/buff logic yourself.

```csharp
// Fixes up the tooltip text for Ice Blast so the condition wording matches
// the new "if it has no Shield" logic instead of the old "if it's Frozen"
// wording — WITHOUT touching how the numeric placeholders get filled in.
//
// _BaseDescription is the raw template string (with number placeholders)
// that GetDescription() reads and populates with the live/buffed values
// every time it's called. We patch this as a Prefix on GetDescription and
// edit _BaseDescription just before the original method runs, doing a
// targeted phrase replace (not a full rewrite) so the placeholder tokens for
// the numbers are left completely untouched — the game still fills them in
// itself afterwards, the exact same way it always did, buffs and all.
//
// We only ever replace the OLD phrase, so re-running this on every call is
// safe/idempotent: once the phrase has already been swapped, .Replace finds
// nothing left to change.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class IceBlastDescriptionPatch
{
    private const int IceBlastCardId = 1003;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != IceBlastCardId) return;

        __instance._BaseDescription = __instance._BaseDescription
            .Replace("if it's Frozen", "if it has no Shield");
    }
}
```

Steps:
1. `[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]` on a
   static class.
2. Add a static `Prefix(CardData __instance)` method (not a Postfix — see
   pitfall below).
3. Guard on `__instance._CardID` for the target card.
4. Use `.Replace(...)` on `__instance._BaseDescription` to swap only the old
   phrase for the new one, leaving number placeholders and everything else in
   the template untouched.
5. If the exact source wording of `_BaseDescription` is unknown, log it once
   (temporarily) to see the literal raw template before writing the replace
   call, then remove the debug log afterward.
6. Register the patch class in `Plugin.Load()` just like the others.
7. Because we only ever replace the *old* phrase, this is naturally
   idempotent — once `_BaseDescription` has already been rewritten, later
   calls find nothing left to replace, so it's safe that this Prefix runs on
   every `GetDescription()` call.

### Pitfall: why a Postfix on `__result` doesn't work here

It's tempting to instead patch the *returned* string with a Postfix
(`ref __result`), but this has two problems:

- **A Prefix that sets `__result` is silently discarded.** The original
  method still runs afterward and computes/returns its own value, overwriting
  whatever the Prefix set — this looks like "the game replaces our text back".
- **A Postfix on `__result` can't correctly re-populate numbers.** By the
  time `__result` exists, the placeholders are already filled in with plain
  text — there's no reliable way to know which numbers came from which effect
  without re-deriving the values yourself (e.g. re-reading `_EffectValue` /
  calling `GetFinalValue` again), which is fragile and easy to get out of sync
  with buffs, upgrades, or other patches. Editing `_BaseDescription` up front
  and letting the *original* templating logic do the substitution avoids all
  of this — you get correct numbers for free, in every situation (menus,
  combat, upgraded cards, active buffs) with zero extra code.

---

## 4. Editing a card's name

`CardData._Name` is a plain writable string field holding the card's display
name. Just like `_BaseDescription`, set it with a **Prefix** on
`CardData.GetDescription` — this hook reliably fires whenever the game needs
to display the card (both in combat and in the collection screen), so a
single patch covers every place the name is shown.

```csharp
// Renames Ice Blast (Card ID 1003) to "Piercing Ice".
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class IceBlastNamePatch
{
    private const int IceBlastCardId = 1003;
    internal const string NewName = "Piercing Ice";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != IceBlastCardId) return;

        __instance._Name = NewName;
    }
}
```

Steps:
1. `[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]` on a
   static class (same hook used for `_BaseDescription` edits).
2. Add a static `Prefix(CardData __instance)` method.
3. Guard on `__instance._CardID` for the target card.
4. Set `__instance._Name` directly to the new name.
5. Register the patch class in `Plugin.Load()` just like the others.

Notes:
- Initially it looked like the collection screen might read the name through
  a different code path (`CardName` property / `GetName()` / `GetEnglishName()`)
  than combat does, since only patching `_Name` seemed to work in combat but
  not in the collection at first glance. In practice, simply setting `_Name`
  via the `GetDescription` Prefix (as above) was enough to fix both — no
  extra patches on those other members were needed. If you ever do hit a
  screen that still shows the old name after this, that's a sign something
  reads the name via a different method/property that isn't triggered by
  `GetDescription`, and you'd need to patch that one too (Postfix its
  `__result` to `IceBlastNamePatch.NewName`, matching the same `_CardID` guard).

---

## 5. Adding a new effect

To add a behavior a card does not already have, create a `CardEffect` and add
it to the card's `_Effects` list from a `CardData.GetDescription` Prefix. The
Prefix must guard on the card ID and check for an equivalent existing effect so
it remains idempotent.

**Important: assign `CardData = __instance` on every newly created effect.**
Effects loaded from the game's assets already have this owner back-reference,
but `new CardEffect` does not. The game can dereference the owner while
resolving or executing an effect; omitting it caused a Unity
`NullReferenceException` for a runtime-added `TriggerEffect`.

```csharp
var triggerCurseEffect = new CardEffect
{
    CardData = __instance,
    _Mode = EffectMode.TriggerEffect,
    _AppliedEffect = AppliedEffectType.Curse,
    _Targeting = EffectTargeting.Ranged,
    _EffectValue = 1,
    _EffectValueUpgraded = 1,
    _EffectCount = 1,
    _EffectCountUpgraded = 1,
};

__instance._Effects.Add(triggerCurseEffect);
```

---

## General checklist for adding a new card patch

1. Find the card's `_CardID` and the effect(s) you want to change (dnSpy /
   temporary logging of `MetaInventory.Instance.AllCardData`).
2. Decide: single value, multiple values (need discriminator fields), text
   change, and/or name change.
3. Create (or reuse) `CardPatches/Card####_Name.cs` for that card ID, and
   write a `[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]`
   Prefix for value/condition changes, and/or a
   `[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]` Prefix
   (editing `_BaseDescription` and/or `_Name`) for text/name changes, and/or
   a new-effect patch (adding a `CardEffect` to `_Effects`, with
   `CardData = __instance`) — see the examples above.
4. Register the new patch class(es) in `Plugin.Load()` via
   `new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(YourPatchClass));`,
   under a comment naming the card and its file (matching the existing
   pattern for other cards).
5. Build (`dotnet build`), copy the resulting DLL to
   `...\BepInEx\plugins\DavidInnaRework\DavidInnaRework.dll`, and test in-game.
   `build_and_deploy.bat` in the project root does both steps in one go (only
   copies if the build succeeds).
