# Card fields and effects reference

Reference dump of the `Rift` namespace types most relevant to card
modification (fields on `CardData`/`CardEffect`, and the enums that drive
effect behavior). Extracted from dnSpy decompiles of the game's IL2CPP
interop assembly:

`C:\Program Files (x86)\Steam\steamapps\common\Breach Wanderers\BepInEx\interop\Assembly-CSharp.dll`

Use this alongside `card modification knowledge.md` (which documents the
patching patterns/conventions) when deciding which fields/enum values to
target for a new card patch.

## `CardData` fields

The card's own metadata. All are simple IL2CPP-backed properties (get/set
directly patchable via field mutation, no Harmony patch needed once you have
an instance).

| Field | Type | Notes |
|---|---|---|
| `_CardID` | `int` | Unique numeric ID. This is what you filter on in patches. |
| `_LinkedCardID` | `int` | Used for e.g. upgraded/alternate versions of a card. |
| `_Name` | `string` | Raw card name (template, not localized/cased). |
| `_BaseDescription` | `string` | Raw tooltip text template with `{0}`, `{1}`, ... placeholders filled in by `GetDescription`. |
| `_Cost` | `int` | Mana cost. |
| `_CostUpgraded` | `int` | Mana cost when upgraded. |
| `_Rarity` | `Rarity` | See enum below. |
| `_CardOrigin` | `CardOrigin` | Which character/origin the card belongs to. See enum below. |
| `_CardType` | `CardType` | Strike/Spell/Skill/etc. See enum below. |
| `_Playable` | `bool` | Whether the card can be played (as opposed to e.g. a passive/summon-only card). |
| `_RepeatForever` | `bool` | Card doesn't get discarded/exhausted after use (repeatable). |
| `_Effects` | `List<CardEffect>` | The card's list of effects — this is what you add/mutate to change card behavior. |
| `_Modifiers` | `CardModifiers` | Card-level modifiers (bitmask/`[Flags]` — see enum below). |
| `_Conditions` | `CardConditions` | Card-level play conditions (bitmask/`[Flags]` — see enum below, paired with `_CardConditionEffect`). |
| `_IncludeInDeckbuilding` | `bool` | Whether the card shows up as a deckbuilding option. |
| `_UnlockTier` | `int` | Progression tier gate for unlocking the card. |
| `_NewCard` | `bool` | Marks card as "new" (UI badge), typically for recently-unlocked cards. |
| `_IsSummonAbility` | `bool` | Whether this card represents a summon's ability rather than a played card. |
| `_ChangeArtOnEffect` | `AppliedEffectType` | If set, card's art changes when the caster has this effect active. |
| `_CardConditionEffect` | `AppliedEffectType` | An `AppliedEffectType` used as part of the card's play condition (paired with `_Conditions`). |
| `_showEffectsInInspector` | `bool` | Unity editor/inspector-only debug flag, no gameplay effect. |

Read-only/computed properties (not fields, don't set directly — useful for
understanding, not for patching):
`IsUnlockableCard`, `CanBeUnlocked`, `IsGeneratedCard`, `IsUnlocked`,
`NoUpgrade`, `IsTool`, `CardName`, `CardNameLowercase`,
`DescriptionLowercase`.

Key methods:
- `GetDescription(Card card = null, bool upgraded = false, bool highlightUpgrade = false, string languageOverride = null)` — reads `_BaseDescription` and fills in its placeholders with live/buffed values. This is also where the game lazily populates its own default `_Name`/`_BaseDescription` text the first time it's called for an untouched card — see the "Static mutations" section of `card modification knowledge.md` for why that matters for the timing of text edits.
- `GetEffectiveTargeting(CardEffect effect, Entity owner)` — resolves an effect's actual targeting at runtime (useful if you need to understand/override targeting logic beyond just setting `_Targeting`).
- `UnlockCard()` / `RefreshUnlocked()` — unlock state management.

## `CardEffect` fields

A single effect entry on a card's `_Effects` list. `CardEffect` is a plain
`Il2CppSystem.Object` (not a `ScriptableObject`), so `new CardEffect { ... }`
object-initializer syntax works fine.

| Field | Type | Notes |
|---|---|---|
| `_Mode` | `EffectMode` | What the effect *does* (Damage, Shield, ApplyEffect, Draw, CreateAndDraw, etc). See the full 149-member enum below. |
| `_AppliedEffect` | `AppliedEffectType` | Which buff/debuff this effect applies, when `_Mode` is one of the `ApplyEffect*`/`TriggerEffect` modes. See enum below. **Tooltip style:** the `{N}` placeholder for this effect's applied value is written in parentheses in `_BaseDescription`, e.g. `Burn ({1})`, `Doom ({0})`, `Powerful ({0})` — unlike plain damage/count/mana placeholders, which are not parenthesized. |
| `_ConditionEffect` | `AppliedEffectType` | The buff/debuff checked against, when `_Modifiers` includes a conditional modifier like `OnlyIfTargetHasEffect`. |
| `_StatusType` | `StatusType` | Frost/Arcane/Shock, for status-manipulating effects (`ModifyStatus`, etc). |
| `_Modifiers` | `EffectModifiers` | One conditional/scaling enum value layered on top of `_Mode` (e.g. `ScalePerEnemy`, `OnlyIfCritical`, `Condition`). It is assigned directly, not combined as a flags mask. |
| `_Targeting` | `EffectTargeting` | Who/what the effect targets (Self, Melee, Monsters, All, etc). See enum below. |
| `_EffectValue` | `int` | The effect's base numeric value (damage amount, shield amount, buff stacks, etc). |
| `_EffectValueUpgraded` | `int` | Value when the card is upgraded. |
| `_EffectCount` | `int` | How many times the effect triggers/hits (e.g. multi-hit strikes). |
| `_EffectCountUpgraded` | `int` | Hit count when upgraded. |
| `_ShowEffectInInspector` | `bool` | Unity editor/inspector-only debug flag, no gameplay effect. |
| `_VFXPrefab` | `UICardVFX` | Visual effect prefab reference used by asset-loaded effects. Runtime-created effects work without assigning a VFX prefab. |
| `_VFXPrefabID` | `int` | ID tied to the VFX prefab. |
| `_Prefab` | `ScriptableObject` | **Card-reference field for "create card" effects** — `CreateAndDraw`, `CreateAndChoose`, and related modes read it to determine which `CardData` to instantiate. It accepts an arbitrary `CardData`, including one built with `ScriptableObject.CreateInstance<CardData>()`. |
| `CardData` (property) | `CardData` | Back-reference to the owning card. Read via `effect.CardData`, used to filter effects belonging to a specific `_CardID` when needed. **Required for runtime-created effects:** set `CardData = cardData` before adding the effect to `cardData._Effects` inside `ApplyMutations`; list insertion does not initialize it, and a missing owner caused a Unity `NullReferenceException` when a new `TriggerEffect` executed. |
| `AffectedEntity` (property) | `Entity` | The entity the effect last affected (runtime state, not config). |

Key methods:
- `GetFinalValue(Card card, bool ignoreDoubleDamage = false, bool applyRandom = false, bool forceHalf = false, bool divide = true)` — computes the effect's actual value at use-time. Most card patches in this project now set `_EffectValue`/`_EffectValueUpgraded` once via `ApplyMutations` (see `card modification knowledge.md`'s "Static mutations" section) instead of hooking this method; it is still hooked directly (as a Postfix) by mechanics that depend on live match state, e.g. `MechanicPatches/ToolsPlayedThisTurnModifierEmulation.cs`. **Fires once per effect**, so any patch here needs a discriminator (e.g. `_Mode`/`_AppliedEffect` check) if a card has multiple effects.
- `GetEffectCount(Card card)` — resolves actual hit/trigger count.
- `CalculateDamageDealt(...)`, `CalculateStatusApplied(...)` — the actual damage/status math, useful to understand scaling modifiers.
- `CanBeUsedByCaster(...)`, `CanBeUsedOnTarget(...)`, `CanBeUsedOnTargetByCaster(...)` — playability/targeting validation checks.

## `EffectMode` enum (what an effect does)

Grouped roughly by theme for readability (values shown are the enum's
explicit numeric values from decompilation; unlisted ones auto-increment
from the previous value).

**Verified complete:** this listing was diffed against a full decompiled dump
of the enum — all 149 members are present and their values match, so it can be
treated as authoritative rather than a partial sample.

**Core play effects**
- `Damage = 1`, `DamageFixed = 125`
- `Shield = 2`, `ShieldTo = 124`, `ShieldFixed = 88`
- `Heal = 3`
- `Move = 4`
- `ApplyEffect = 5`, `ApplyEffectTo = 131`, `ApplyEffectFixed = 121`, `ApplyEffectThisTurn = 31`, `ApplyEffectThisTurnPlus1 = 87`, `ApplyEffectBaseValue = 68`
- `Draw = 6`, `DrawTo = 113`, `DrawNoFatigue = 103`, `DrawCard = 90`, `DrawID = 54`, `DrawMana = 40`, `DrawSkill`, `DrawSpell = 33`, `DrawStrike = 32`, `DrawBlight = 119`, `DrawWeather = 129`, `DrawSpellOrStrike = 118`, `DrawNonMana = 50`, `DrawNonManaNoFatigue = 128`, `DrawTopDiscardPile = 24`, `DrawTag = 135`, `DrawArcane = 137`, `DrawFrost`, `DrawShock`, `DrawBurn`, `DrawPoison`
- `Discard = 7`, `DiscardAll = 110`, `DiscardTool = 81`, `DiscardRandom = 28`

**Status effects (Frost/Arcane/Shock)**
- `ModifyStatus = 8`, `ModifyStatusFixed = 78`, `ModifyStatusTo = 130`, `ModifyAllStatuses = 9`, `ModifyRandomStatus = 61`
- `ModifyBuffsDuration = 10`, `ModifyDebuffsDuration`
- `ModifyMaxHealth`, `ModifyMaxMana`
- `AbsorbStatus = 18`
- `RemoveStatusAndDealDamage = 20`
- `Cleanse = 69` removes debuffs, `Dispel = 96` dispels effects, `RemoveEffect = 34`, `ClearEffect = 66`

**Card creation ("create card" effects — see `_Prefab` field)**
- `CreateAstral = 93`
- `CreateAndChoose = 94` (auto-incremented), `CreateAndChooseFree = 98`, `CreateAndChooseFreeNoUpgrade = 116`
- `CreateAndDraw = 15` — **confirmed working**: creates and draws a specific `CardData` (via `_Prefab`) directly into hand.
- `CreateAndDrawTemporary = 59`, `CreateAndDrawTemporaryFree = 77`, `CreateAndDrawFree = 148`
- `CreateAndShuffle = 16`, `CreateAndDiscard = 37`, `CreateAndTop = 53`
- `CreateAndDrawUpgraded = 71`, `CreateAndDiscardUpgraded`, `CreateAndShuffleUpgraded`, `CreateAndTopUpgraded = 86`
- `CreateCache = 122`, `CreateTopDeckCopy = 70`, `CreateTool = 46`, `CreateTreasure = 99`
- `CopyCard = 30`, `CopyThis = 52`, `CopyThisDiscard = 95`, `CopyNonMana = 60`, `CopySpell = 144`, `CopySpellAndDouble = 101`

**Mana**
- `AddMana = 17`, `AddManaTo = 123`, `AddManaNextTurn = 23`, `ManaPercentage = 44`
- `LoseManaEndTurn = 22`
- `IncreaseMana = 112`

There is no immediate "lose mana" mode. `LoseManaEndTurn` is deferred to end
of turn, so immediate mana reduction uses `AddMana` with a negative
`_EffectValue`.

**Cost/value scaling & buffs to cards themselves**
- `Increase = 21`, `IncreaseThisTurn = 100`, `IncreaseCannons = 92`, `IncreaseShield = 91`, `IncreaseDamage = 80`, `IncreaseDamageThisTurn = 82`, `IncreaseFixedDamageThisTurn = 143`, `IncreaseTopSpell = 56`, `IncreaseAllSpells = 83`, `IncreaseAllSpellsThisTurn = 109`, `IncreaseCost = 57`, `IncreaseCostThisTurn = 67`, `IncreaseFloodCostThisTurn = 108`, `IncreaseStrikeDamage = 74`, `IncreaseNextStrikeDamage = 146`, `IncreaseResonatingThisTurn = 105`, `IncreasePacts = 136`, `IncreaseHits = 127`
- `ReduceIDCost = 38`, `ReduceIDCostEverywhere = 63`, `ReduceAllCosts = 45`, `ReduceSpellCost = 106`, `ReduceStrikesCost = 145`
- `DoubleSpell = 25`, `DoubleSpellValues = 102`, `DoubleSkill = 26`, `DoubleValues = 65`, `DoubleValuesThisTurn = 84`, `DoubleDamageThisTurn = 133`
- `Upgrade = 47`, `UpgradeID = 43`, `UpgradeThisTurn = 117`, `UpgradeTools = 48`

**Combat/misc**
- `Capture = 114`, `ChangeNextAction = 29`, `ChangePhase = 85`, `ChangeRecoverableHealth = 27`
- `Corrupt = 97`
- `PutOnTop = 62`, `Recycle = 58`, `Shuffle = 132`
- `StasisCard = 104`, `StasisSelf = 147`, `StayHidden = 19`
- `SwitchStance = 51`
- `Summon = 14`, `SummonRandom = 142`, `SummonOverride = 126`, `SummonSick = 89`
- `TransformThis = 35`, `TransformThisAndStay = 111`, `TransformID = 36`, `TransformEntity = 39`, `TransformCard = 55`, `TransformCardRemoveBlights = 76`
- `TriggerEffect = 42`, `TriggerEffectNoReduction = 120`, `TriggerEnemy = 64`, `TriggerIncrease = 49`, `TriggerOpportunity = 115`
- `Kill = 75`
- `Reset = 79`, `ResetCondition = 107`, `ResetAllCosts = 134`
- `COUNT = 149` (sentinel, not a real mode)

## `EffectTargeting` enum (who an effect targets)

```
NONE, Self, Melee, Cleave, Ranged, Monsters, All, Previous, Random,
Allies, FirstAlly, Player, AnyAlly, MonstersDivide, AlliesDivide,
OnlyAllies, COUNT
```

Notes:
- `Melee` targets the first enemy.
- `Monsters` targets all enemies.
- `Self` targets the caster/card owner; `Player` targets the player character.
- `Previous` re-targets whatever the card's preceding effect targeted (e.g.
  the enemy chosen by an earlier `Ranged` effect), rather than resolving a
  new target. Used on Card 1409 ("Investigate") so its two debuff-gated
  `CreateTool` effects apply to the same enemy chosen by the first effect.

## `AppliedEffectType` enum (buffs/debuffs, for `_AppliedEffect`/`_ConditionEffect`/`_ChangeArtOnEffect`/`_CardConditionEffect`)

Large enum (`COUNT = 82`) of every status/buff/debuff in the game:

```
NONE, Adept=20, Ambush=32, ArcaneBarrier=10, ArcaneRune=6, ArcaneLeak=35,
ArcaneRage=72, ArcaneWeakness=45, Berserk=65, Bleed=7, Bloodlust=27,
BrainFreeze=78, Burn=1, Counterattack=60, Critical=23, Crystallize=62,
Curse=54, DeathSpores=31, Decay=76, Divinity=80, Doom=26, Energy=56,
Enfeebled=66, Envenom=42, Evasion=17, Fatigue=55, ElementalMastery=61,
Fortress=16, Frail=51, Frenzy=25, Frostbite=12, FrostBarrier=8,
FrostRune=4, Frozen=2, Growth=43, Hidden=22, Nullify=74, IceEater=38,
Invincible=70, Leech=14, Lethal=33, Lifesteal=64, Madness=79,
ManaBoost=21, Mark=59, Marksman=71, Mighty=18, Opportunist=58,
Overcharged=46, Reflect=11, PackTactics=34, Plated=15, Poison=41,
PoisonedBlood=44, Powerful=30, Predator=40, Purity=53, Rebirth=37,
Shocked=3, ShockRune=5, ShockBarrier=9, Shrivel=52, SpiritLink=73,
StanceSun=67, StanceStars, StanceMoon, Spellweave=77, SpellStance=50,
StrikeStance=49, StickyGoo=75, Sturdy=47, Supercharged=57, Swarm=39,
Taunt=36, Tough=19, UnleashedPotential=81, Unstable=24, Vengeance=48,
Vulnerable=29, Weak=13, Whiteflame=63, Wits=28, COUNT=82
```

Frequently useful ones already used in this project: `Tough = 19` (block/
defensive buff, "reduce next N instances of damage" style), `Burn = 1`,
`Frozen = 2`, `Shocked = 3`, `Poison = 41`, `Weak = 13`, `Vulnerable = 29`.

`AppliedEffectTag` (separate small enum, classifies the above into
buff/debuff): `NONE`, `Buff`, `Debuff`.

## `EffectModifiers` enum (conditional/scaling behavior, for `CardEffect._Modifiers`)

Large enum (`COUNT` sentinel, ~150 values) of conditions and scaling rules
layered on top of an effect's base behavior. The listing below is grouped by
theme and covers the members relevant to this project's patches; it is not
a full transcription of the enum.

**Conditionals ("only if ...")**
`OnlyIfTargetHasEffect`, `OnlyIfTargetHasntEffect`, `OnlyIfKilledTarget`,
`OnlyIfAnEnemyHasEffect`, `OnlyIfOutOfMana`, `OnlyIfNoStatus`,
`OnlyIfEnemyLostLife`, `OnlyIfTargetHasNoShield`, `OnlyIfTargetHasShield`,
`OnlyIfCasterHasEffect`, `OnlyIfAnEnemyHas5PlusEffects`, `OnlyIfCritical`,
`OnlyIfEnemyDied`, `OnlyIfNoFrost`, `OnlyIfDealt4OrMoreDamage`,
`OnlyIfDealt10OrMoreDamage`, `OnlyIfDealt20OrMoreDamage`,
`OnlyIfDealt30OrMoreDamage`, `OnlyIfDealt100OrMoreDamage`,
`OnlyIfTargetHas3PlusEffects`, `OnlyIfTargetHas5PlusEffects`,
`OnlyIfTargetHas10PlusEffects`, `OnlyIfTargetHas20LessHealth`,
`OnlyIfTargetHas24PlusEffects`, `OnlyIfTargetIsSummon`,
`OnlyIfAnEnemyHas3PlusDebuffs`, `OnlyIfAnEnemyHas5PlusDebuffs`,
`OnlyIfAnEnemyHas10PlusEffects`, `OnlyIfTargetHas3PlusDebuffs`,
`OnlyIfTargetHas5PlusDebuffs`, `OnlyIfAlly`, `OnlyIfSprite`,
`OnlyIfDetonatedThisTurn`, `OnlyIfNoFatigueTrigger`,
`OnlyIfCasterHas5PlusEffects`, `OnlyIfPlayed3PlusStrikes`,
`OnlyIfTargetHas10PlusShield`, `OnlyIfStrikeInHand`, `OnlyIfSpellInHand`,
`OnlyIfManaInHand`, `OnlyIfSkillInHand`

**Scaling ("scale per/with ...") — these consume `_EffectValue` as a
per-unit amount and multiply by some count**
`ScalePerEnemyWithEffect`, `ScalePerTotalEffectRank`, `ScalePerEffect`,
`ScalePerEnemy`, `ScalePerTool`, `ScalePerCard`, `ScalePerCasterEffect`,
`ScaleWithMaxMana`, `ScaleWithCasterShield`, `ScaleWithMaxShield`,
`ScalePerDamageDealt`, `ScalePerHealingDone`, `ScaleWithMaxFrost`,
`ScalePerCannon`, `ScalePerArea`, `ScalePerAlly`, `ScaleWithMaxArcane`,
`ScalePerCardDrawnCost`, `ScaleWithMaxHealth`, `ScaleWithMaxShock`,
`ScalePerBuffOnTarget`, `ScalePerDebuffOnTarget`, `ScalePerBuffOnCaster`,
`ScalePerDebuffOnCaster`, `ScalePerAreaHalf`, `ScalePerAreaAndDivide`,
`ScalePerCurrentMana`, `ScalePerTargetKilled`,
`ScaleHitsPerStrikePlayed`, `ScaleWithMaxFrostIfEffect`,
`ScalePerTotalCard`, `ScalePerDetonation`, `ScalePerIdPlayed`,
`ScalePerStrikePlayed`, `ScalePerDeath`, `ScalePerOverhealingDone`,
`ScalePerCardCreatedThisTurn`, `ScalePerCardPerArea`,
`ScalePerDebuffOnTargetPerArea`, `ScalePerTotalEffectRankAllies`,
`ScalePerCardDiscarded`, `ScaleWithManaGain`, `ScalePerDoom`

The project emulates "scale per Tool played this turn" by using
`ScalePerStrikePlayed` with `_ConditionEffect = AppliedEffectType.COUNT` as a
private marker. The `ToolsPlayedThisTurn` mechanic counts Tool cards in
`CombatManager.UseCard`, resets the count at `StartPlayerTurn`, and multiplies
the effect's base/upgraded value by the count in a `GetFinalValue` Postfix.

**Triggers/timing**
`WhenDiscarded`, `EndOfTurn`, `FollowStrike`, `FollowSkill`, `FollowSpell`,
`FollowMana`, `FollowTool`, `LastCard`, `WhenDrawn`,
`TriggerOnNonManaPlayed`, `TriggerOnStanceSwitched`, `TriggerOnEvade`,
`TriggerOnDeath`, `TriggerOnCardDiscarded`, `TriggerOnEffectTrigger`,
`FollowWeather`, `IfTargetWillAttack`, `FirstCardThisTurn`,
`FollowNonTemporaryStrike`, `Phase1`, `Phase2`

**Misc**
`CritIfEffect`, `EffectDoubleDamage`, `BonusCritical`, `CritIfShield`,
`Condition`, `Random`, `DoubleDamageEffectTarget`, `JustDidCritical`,
`CannotBeEvaded`

`Condition` gates an effect on the *preceding* effect in the same card's
`_Effects` list succeeding — no `_ConditionEffect` companion value is
needed. Confirmed on Card 1408 ("Frantic Scouring"): its on-play behavior is
a `Discard` effect (discard 1 card) immediately followed by a `CreateTool`
effect with `_Modifiers = EffectModifiers.Condition` and
`_ConditionEffect = NONE` — the CreateTool effect only fires if the Discard
effect actually discarded a card.

`_Modifiers` stores one `EffectModifiers` value per effect. It is not a
`[Flags]` bitmask and values are assigned directly.

## `StatusType` enum

```
NONE, Frost, Arcane, Shock, COUNT
```

## `CardType` enum

```
NONE, Spell, Strike, Skill, Mana, Tool, Weather, Shrine, Blight, COUNT
```

## `Rarity` enum

```
Common, Rare, Epic, Godly, Artifact, COUNT
```

## `CardOrigin` enum

Which character/origin a card belongs to (`Generic` = no specific
character):

```
NONE, Generic, Monster, Trinket, Weather, Shrine, Drofis, Mirley, Raodan,
Silan, Inna, Caitan, Paladin, Shahru, Nayema, Maighir, COUNT
```

## `CardModifiers` enum (`[Flags]`, for `CardData._Modifiers`)

A bitmask of card-level state/behavior modifiers. Since it's `[Flags]`,
multiple values can be combined with `|` (e.g.
`CardModifiers.Temporary | CardModifiers.PlayWhenDrawn`).

| Value | Bit | Meaning |
|---|---|---|
| `NONE` | 0 | No modifiers. |
| private "draw on card played" marker | 1 | Reserved by this plugin as an internal transient marker; use `DrawOnCardPlayedRegistry.DrawMarker` rather than a numeric cast at call sites. Confirmed via dnSpy: the enum's declared members jump straight from `NONE = 0` to `FreeIfEffectOnTarget = 2`, so this value has no native meaning. `MechanicPatches/DrawOnCardPlayedRegistry.cs` only ever sets this bit (and `NoFatigueSpecialDraw`, for targets configured with `noFatigue: true`) momentarily, immediately before an `Entity.DrawCardsWithModifier` call, clearing both again immediately after — it is never left set on any card at rest, so it stays free for another plugin to reuse for its own unrelated purposes outside that narrow window. |
| `FreeIfEffectOnTarget` | 2 | Card costs 0 if the target has a specific effect (paired with a condition effect elsewhere). |
| `Removed` | 4 | Card has been removed (e.g. from deck). |
| `Temporary` | 8 | Card is temporary (e.g. created mid-combat, removed after use/combat ends). |
| `ShuffleBack` | 16 | Card shuffles back into the deck after use instead of going to discard. |
| `SilanCannon` | 32 | Origin-specific mechanic flag (Silan's cannon system). |
| `Recycled` | 64 | Card was recycled: returns to the deck instead of the discard pile, set by the `Recycle` effect mode. |
| `NoUpgrade` | 128 | Card cannot be upgraded. |
| `UnplayableUnlessFree` | 256 | Card can only be played when its cost is reduced to 0. |
| `Unplayable` | 512 | Card cannot be played at all (e.g. a purely passive/status card). |
| `Stifled` | 1024 | Card's effects are suppressed/silenced. |
| `PlayWhenDrawn` | 2048 | Card auto-plays immediately when drawn. |
| `Lifesteal` | 4096 | Card grants lifesteal (heal for damage dealt). |
| `Amplified` | 8192 | Card's effects are amplified/boosted. |
| `DrawOnNoMana` | 16384 | Draw a card when out of mana. |
| `DrawOnEffectApplied` | 32768 | Draw a card when this card applies an effect. |
| `DrawOnCritical` | 65536 | Draw a card on a critical hit. |
| `DrawOnKill` | 131072 | Draw a card when this card kills its target. |
| `DrawOnNoShield` | 262144 | Draw a card when the target has no shield. |
| `ShowEffectTooltip` | 524288 | Forces the effect tooltip to show (UI-only). |
| `Stasis` | 1048576 | Card is in stasis (can't be discarded/interacted with normally). |
| `Charred` | 2097152 | Card is charred: a burn-related state that reduces the card's effect value or marks it for removal. |
| `Retained` | 4194304 | Card is retained in hand at end of turn (doesn't get discarded). |
| `NoFatigueSpecialDraw` | 8388608 | This card's draw doesn't trigger/count toward fatigue. |
| `DrawOnEvade` | 16777216 | Draw a card when an attack is evaded. |
| `KeepInHand` | 33554432 | Card stays in hand during normal hand-management operations. `Retained` preserves a card specifically through end-of-turn discard. |
| `DrawOn20DamageDealt` | 67108864 | Draw a card after dealing 20+ damage. |
| `DrawOnDoom` | 134217728 | Draw a card when Doom triggers. |
| `ReturnDiscard` | 268435456 | Card returns from discard pile (e.g. back to hand/deck). |
| `Legendary` | 536870912 | Marks the card as Legendary and controls the corresponding card state and display. |
| `FreeIfAlly` | 1073741824 | Card costs 0 if caster has an ally present. |
| `CannotBeEvaded` | -2147483648 (bit 31 / `0x80000000`) | Card's effects cannot be evaded. |

## `CardConditions` enum (`[Flags]`, for `CardData._Conditions`)

A bitmask of card-level play conditions/requirements (paired with
`CardData._CardConditionEffect` when the condition references a specific
buff/debuff, e.g. `PlayerHasCondition`/`PlayerHasntCondition`).

| Value | Bit | Meaning |
|---|---|---|
| `NONE` | 0 | No conditions. |
| `Has3PlusMonsters` | 2 | Requires 3+ enemies present. |
| `Has2LessMonsters` | 4 | Requires 2 or fewer enemies present. |
| `Has2PlusMonsters` | 8 | Requires 2+ enemies present. |
| `UnderHalfHealth` | 16 | Requires caster/target under 50% health. |
| `AboveHalfHealth` | 32 | Requires caster/target above 50% health. |
| `PlayerHasCondition` | 64 | Requires the player to have the effect specified by `_CardConditionEffect`. |
| `PlayerHasntCondition` | 128 | Requires the player to NOT have the effect specified by `_CardConditionEffect`. |
| `HasShield` | 256 | Requires target to have shield. |
| `HasntShield` | 512 | Requires target to have no shield. |
| `Has1Monster` | 1024 | Requires exactly 1 enemy present. |
| `Phase1` | 2048 | Requires combat to be in phase 1 (boss-fight phase gating). |
| `Phase2` | 4096 | Requires combat to be in phase 2. |

## Card registry note

`MetaInventory.AllCardData` is **not** the game's live card registry: it has
0 call sites anywhere in the decompiled assembly, and mutating its entries
never reaches the actual gameplay `CardData` instances.

`ResourcesManager.Instance.CardData` (`Dictionary<int, CardData>`) **is** the
real master registry — confirmed via native IL2CPP pointer comparison against
the instance `CardData.GetDescription` is called on. It is fully populated
(1530 entries) the moment the compiler-generated `MoveNext()` of
`ResourcesManager.Initialize()`'s coroutine state machine returns `false` for
the first time. Card patches read/mutate `CardData` instances from that
dictionary, once, via
[MechanicPatches/CardDataGameLoadInitializer.cs](/scripts/c#/bw_patching/DavidInnaRework/MechanicPatches/CardDataGameLoadInitializer.cs)
— see the "Static mutations" section of `card modification knowledge.md` for
the full explanation and the live-state exceptions that still require
per-call Harmony patches.
