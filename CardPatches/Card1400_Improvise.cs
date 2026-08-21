using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1400 "Improvise" reworked to:
//   - create only 1 tool (down from 2),
//   - then lose 1 (2 upgraded) mana,
//   - then, if you have no mana left, create 2 (3 upgraded) more tools.
//
// The original card only has the CreateTool effect, so the mana loss and the
// conditional second CreateTool are added as brand new CardEffects.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time (see
// that file for why every field here — effects, values, and text — can be
// set exactly once instead of via per-call Harmony prefixes).
public static class Card1400_Improvise
{
    internal const int ImproviseCardId = 1400;
    private const int ToolsCreated = 1;
    private const int ManaLost = 1;
    private const int ManaLostUpgraded = 2;
    private const int BonusTools = 2;
    private const int BonusToolsUpgraded = 3;
    private const int Cost = 1;
    private const string NewDescription =
        "Create {0} Tool. Lose {1} mana. If you have no mana, create {2} more Tools.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != ImproviseCardId) return;

        // Only touch the original, unconditional CreateTool effect — the mana
        // loss and the "if out of mana" CreateTool effect added below keep
        // their own values.
        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.CreateTool) continue;
            if (effect._Modifiers == EffectModifiers.OnlyIfOutOfMana) continue;

            effect._EffectValue = ToolsCreated;
            effect._EffectValueUpgraded = ToolsCreated;
        }

        // Two new effects, in play order after the existing CreateTool:
        // negative AddMana, then CreateTool gated behind
        // EffectModifiers.OnlyIfOutOfMana.
        //
        // Both set CardData = cardData: effects loaded from the game's
        // assets already have this owner back-reference, but `new CardEffect`
        // does not, and a missing owner causes a NullReferenceException at
        // runtime.
        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.AddMana,
            _Targeting = EffectTargeting.Self,
            _EffectValue = -ManaLost,
            _EffectValueUpgraded = -ManaLostUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.OnlyIfOutOfMana,
            _Targeting = EffectTargeting.Self,
            _EffectValue = BonusTools,
            _EffectValueUpgraded = BonusToolsUpgraded,
        });

        cardData._Cost = Cost;
        cardData._CostUpgraded = Cost;
        cardData._BaseDescription = NewDescription;
    }
}
