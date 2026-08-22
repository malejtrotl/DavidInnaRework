using Rift;

namespace DavidInnaRework.CardPatches;

// Adventurer's Log, 1414
// Draw 2. Create 1 Inkwell and Quill.
// Draw 2. Create 1 Inkwell and Quill.
public static class Card1414_AdventurersLog
{
    internal const int AdventurersLogCardId = 1414;
    private const int DrawCount = 2;
    private const int DrawCountUpgraded = 2;
    private const int InkwellAndQuillCreated = 1;
    private const int InkwellAndQuillCreatedUpgraded = 1;
    private const int Cost = 4;
    private const int UpgradedCost = 2;
    private const string NewDescription = "Draw {0}. Create {1} Inkwell and Quill.";

    public static void ApplyMutations(CardData cardData, CardData inkwellAndQuillCardData)
    {
        if (cardData == null || cardData._CardID != AdventurersLogCardId) return;
        if (inkwellAndQuillCardData == null) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Draw,
            _Targeting = EffectTargeting.Self,
            _EffectValue = DrawCount,
            _EffectValueUpgraded = DrawCountUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateAndDraw,
            _Targeting = EffectTargeting.Self,
            _Prefab = inkwellAndQuillCardData,
            _EffectValue = InkwellAndQuillCreated,
            _EffectValueUpgraded = InkwellAndQuillCreatedUpgraded,
        });

        cardData._Cost = Cost;
        cardData._CostUpgraded = UpgradedCost;
        cardData._BaseDescription = NewDescription;
    }
}
