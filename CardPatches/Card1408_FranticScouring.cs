using Rift;

namespace DavidInnaRework.CardPatches;

// Frantic Scouring, 1408
// Discard 1 to create 3 Tools. Create 1 Tool when discarded.
// Discard 1 to create 4 Tools. Create 2 Tool when discarded.
public static class Card1408_FranticScouring
{
    internal const int FranticScouringCardId = 1408;
    private const int CardsDiscarded = 1;
    private const int ToolsCreated = 3;
    private const int ToolsCreatedUpgraded = 4;
    private const int ToolsCreatedOnDiscard = 1;
    private const int ToolsCreatedOnDiscardUpgraded = 2;
    private const string NewDescription = "Discard {0} to create {1} Tools. Create {2} Tool when discarded.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != FranticScouringCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Discard,
            _Targeting = EffectTargeting.Self,
            _EffectValue = CardsDiscarded,
            _EffectValueUpgraded = CardsDiscarded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.Condition,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToolsCreated,
            _EffectValueUpgraded = ToolsCreatedUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.WhenDiscarded,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToolsCreatedOnDiscard,
            _EffectValueUpgraded = ToolsCreatedOnDiscardUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
