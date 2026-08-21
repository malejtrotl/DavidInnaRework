using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1407 "Ingenuity": new cost, different tool count, and completely new
// description text.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
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

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.CreateTool) continue;

            effect._EffectValue = ToolsCreated;
            effect._EffectValueUpgraded = ToolsCreatedUpgraded;
        }

        cardData._Cost = Cost;
        cardData._CostUpgraded = CostUpgraded;
        cardData._BaseDescription = NewDescription;
    }
}
