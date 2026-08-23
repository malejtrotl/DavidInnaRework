using HarmonyLib;
using Rift;

namespace DavidInnaRework.MechanicPatches;

// Generic, multi-trigger "draw a specific card when a card of a given
// CardType is played" mechanic. Replaces the earlier
// NoFatigueDrawOnToolPlayed.cs / DrawOnToolPlayed.cs / DrawOnToolPlayedShared.cs
// trio, which could only safely support cards sharing one trigger CardType
// at a time before their shared marker bit caused cross-contamination (see
// "card modification knowledge.md" for the full background).
//
// Ownership (which CardType triggers which target card(s), and whether each
// target's draw should skip fatigue) is tracked entirely on the plugin's own
// side, in the registry below — never permanently on CardData itself — so
// any number of independent triggers (different CardTypes, different
// target cards) can coexist without cross-contamination.
//
// CardModifiers has no spare bit for a private marker beyond the single
// unclaimed one (value 1 — confirmed via dnSpy: the enum's declared members
// jump straight from NONE = 0 to FreeIfEffectOnTarget = 2, and the enum
// itself is a plain 32-bit int with every other bit already claimed by a
// real named modifier). This mechanic reuses that one bit, but — unlike the
// earlier design — never leaves it set on any card outside the exact
// instant Entity.DrawCardsWithModifier needs it: the bit (and
// NoFatigueSpecialDraw, for targets that want it) is set immediately before
// that call and cleared immediately after, on only the specific cards
// registered for the trigger CardType that just fired. This keeps the bit
// clear at rest for the rest of the game session, leaving it free for
// another plugin to use for its own unrelated purposes outside of that
// narrow window.
public static class DrawOnCardPlayedRegistry
{
    // Private marker bit standing in for a "DrawOnCardPlayed" trigger flag.
    // Only ever set transiently, immediately before a DrawCardsWithModifier
    // call, and cleared again immediately after — never left set at rest.
    internal const CardModifiers DrawMarker = (CardModifiers)1;

    private readonly struct Target
    {
        public readonly int CardId;
        public readonly bool NoFatigue;

        public Target(int cardId, bool noFatigue)
        {
            CardId = cardId;
            NoFatigue = noFatigue;
        }
    }

    // Registered targets, grouped by the CardType that triggers them.
    private static readonly System.Collections.Generic.Dictionary<CardType, System.Collections.Generic.List<Target>> _targetsByTriggerType = new();

    // Live CardData references for every registered target, cached once at
    // real game-load time (see CardDataGameLoadInitializer.cs) so the live
    // trigger patch never needs to search for them.
    private static readonly System.Collections.Generic.Dictionary<int, CardData> _targetCardData = new();

    // Registers `targetCardId` to draw itself whenever a card of
    // `triggerCardType` is played. Set `noFatigue` to true if the draw
    // should not count toward fatigue. Call from Plugin.Load(), before the
    // game-load initializer is registered.
    public static void Register(CardType triggerCardType, int targetCardId, bool noFatigue)
    {
        if (!_targetsByTriggerType.TryGetValue(triggerCardType, out var targets))
        {
            targets = new System.Collections.Generic.List<Target>();
            _targetsByTriggerType[triggerCardType] = targets;
        }

        targets.Add(new Target(targetCardId, noFatigue));
    }

    // Caches live CardData references for every registered target card that
    // exists in `cardDataDict`. Called once from
    // CardDataGameLoadInitializerPatch at real game-load time.
    internal static void CacheCardData(Il2CppSystem.Collections.Generic.Dictionary<int, CardData> cardDataDict)
    {
        if (cardDataDict == null) return;

        foreach (var targets in _targetsByTriggerType.Values)
        {
            foreach (var target in targets)
            {
                if (_targetCardData.ContainsKey(target.CardId)) continue;
                if (!cardDataDict.ContainsKey(target.CardId)) continue;

                _targetCardData[target.CardId] = cardDataDict[target.CardId];
            }
        }
    }

    // Called from the live CombatManager.UseCard trigger patch whenever any
    // card is played. Draws every target registered under the played card's
    // CardType, and only those targets — cards registered under a
    // different trigger CardType are left completely untouched for this
    // call.
    internal static void TriggerDraw(CardType playedCardType, Entity castingEntity)
    {
        if (castingEntity == null) return;
        if (!_targetsByTriggerType.TryGetValue(playedCardType, out var targets)) return;
        if (targets.Count == 0) return;

        // Flag only this trigger's own registered cards, immediately
        // before the draw call.
        foreach (var target in targets)
        {
            if (!_targetCardData.TryGetValue(target.CardId, out var cardData)) continue;

            cardData._Modifiers |= DrawMarker;
            if (target.NoFatigue)
            {
                cardData._Modifiers |= CardModifiers.NoFatigueSpecialDraw;
            }
        }

        castingEntity.DrawCardsWithModifier(DrawMarker);

        // Clear the bits again immediately, so nothing is left set on any
        // card outside the moment it was actually needed.
        foreach (var target in targets)
        {
            if (!_targetCardData.TryGetValue(target.CardId, out var cardData)) continue;

            cardData._Modifiers &= ~DrawMarker;
            if (target.NoFatigue)
            {
                cardData._Modifiers &= ~CardModifiers.NoFatigueSpecialDraw;
            }
        }
    }
}

// The actual trigger is inherently event-driven (fires live, each time any
// card is played during a match), so this stays a per-call Harmony patch —
// a single generic hook serving every registered trigger CardType/target
// combination, instead of one Harmony patch per mechanic.
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.UseCard))]
public static class DrawOnCardPlayedPatch
{
    static void Prefix(Card card, Entity castingEntity)
    {
        if (card == null || castingEntity == null) return;

        var cardData = card.Data;
        if (cardData == null) return;

        DrawOnCardPlayedRegistry.TriggerDraw(cardData._CardType, castingEntity);
    }
}
