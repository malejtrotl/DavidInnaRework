using Rift;

namespace DavidInnaRework.CardPatches;

// Ingenuity, 1407
// Create 3 Tools, then upgrade all Tool cards in hand.
// Create 3 Tools, then upgrade all Tool cards in hand.
public static class Card1407_Ingenuity
{
    internal const int IngenuityCardId = 1407;
    private const int ToolsCreated = 3;
    private const int ToolsCreatedUpgraded = 3;
    private const int Cost = 4;
    private const int CostUpgraded = 2;
    private const string NewDescription = "Create {0} Tools, then upgrade all Tool cards in hand.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != IngenuityCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToolsCreated,
            _EffectValueUpgraded = ToolsCreatedUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.UpgradeTools,
            _Targeting = EffectTargeting.Self,
        });

        cardData._Cost = Cost;
        cardData._CostUpgraded = CostUpgraded;
        cardData._BaseDescription = NewDescription;
    }
}
