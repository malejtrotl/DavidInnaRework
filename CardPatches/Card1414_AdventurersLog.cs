using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1414 "Adventurer's Log": the UPGRADED version now costs 2 and draws 2
// cards. The unupgraded version is left exactly as-is, so only the *Upgraded
// fields are written.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
public static class Card1414_AdventurersLog
{
    internal const int AdventurersLogCardId = 1414;
    private const int UpgradedDrawCount = 2;
    private const int UpgradedCost = 2;

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != AdventurersLogCardId) return;

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.Draw) continue;

            effect._EffectValueUpgraded = UpgradedDrawCount;
        }

        cardData._CostUpgraded = UpgradedCost;
    }
}
