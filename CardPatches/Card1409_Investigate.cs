using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1409 originally reads:
//   "Choose any enemy. Create 1 Tool, then 3 Tools if it has 3 or more debuff
//    types, then 3 Tools if it has 5 or more debuff types."
//
// The baseline (unconditional) CreateTool effect goes from 1 to 2 Tools, for
// both the unupgraded and upgraded versions. The two conditional CreateTool
// effects are left untouched.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time. The
// card has THREE CreateTool effects, so the two conditional ones are
// identified by their _Modifiers gates; anything else is treated as the
// baseline effect. Discriminating by exclusion (rather than testing for
// EffectModifiers.NONE) avoids depending on a modifier value that the
// knowledge reference does not confirm exists.
public static class Card1409_Investigate
{
    internal const int InvestigateCardId = 1409;
    private const int BaselineTools = 2;
    private const string NewDescription =
        "Choose any enemy. Create {0} Tools, then {1} Tools if it has 3 or more debuff types, then {2} Tools if it has 5 or more debuff types.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != InvestigateCardId) return;

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.CreateTool) continue;

            // Leave the two debuff-gated CreateTool effects alone.
            if (effect._Modifiers == EffectModifiers.OnlyIfTargetHas3PlusDebuffs
                || effect._Modifiers == EffectModifiers.OnlyIfTargetHas5PlusDebuffs)
            {
                continue;
            }

            effect._EffectValue = BaselineTools;
            effect._EffectValueUpgraded = BaselineTools;
        }

        cardData._BaseDescription = NewDescription;
    }
}
