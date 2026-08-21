using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1408 "Frantic Scouring" gains a NEW when-discarded effect that creates
// 2 Tools (3 upgraded). The card already creates Tools when played; that
// on-play effect is left untouched.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
//
// CardData = cardData is required on the new effect: effects loaded from the
// game's assets already have this owner back-reference, but `new CardEffect`
// does not, and a missing owner causes a NullReferenceException at runtime.
public static class Card1408_FranticScouring
{
    internal const int FranticScouringCardId = 1408;
    private const int ToolsCreated = 2;
    private const int ToolsCreatedUpgraded = 3;
    private const string NewDescription = "Discard {0} to create {1} Tools.\nCreate {2} Tools when discarded.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != FranticScouringCardId) return;

        // The discard effect does not exist on the card yet, and is matched
        // on the WhenDiscarded modifier so it won't be confused with the
        // card's existing on-play CreateTool effect.
        foreach (var existingEffect in cardData._Effects)
        {
            if (existingEffect._Mode == EffectMode.CreateTool
                && existingEffect._Modifiers == EffectModifiers.WhenDiscarded)
            {
                return;
            }
        }

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.WhenDiscarded,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToolsCreated,
            _EffectValueUpgraded = ToolsCreatedUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
