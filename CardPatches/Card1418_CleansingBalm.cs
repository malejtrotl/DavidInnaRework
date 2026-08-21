using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1418 "Cleansing Balm": bumps the Cleanse count from 1 to 2 (3
// upgraded).
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
public static class Card1418_CleansingBalm
{
    internal const int CleansingBalmCardId = 1418;
    private const int CleanseCount = 2;
    private const int UpgradedCleanseCount = 3;
    private const string NewDescription = "Reduce all statuses by {0}. Cleanse yourself {1} times.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != CleansingBalmCardId) return;

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.Cleanse) continue;

            effect._EffectValue = CleanseCount;
            effect._EffectValueUpgraded = UpgradedCleanseCount;
        }

        cardData._BaseDescription = NewDescription;
    }
}
