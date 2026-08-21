using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1423 "Caltrops": increases the hit count from 2 to 3 (4 upgraded).
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
public static class Card1423_Caltrops
{
    internal const int CaltropsCardId = 1423;
    private const int HitCount = 3;
    private const int UpgradedHitCount = 4;

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != CaltropsCardId) return;

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.Damage) continue;

            effect._EffectCount = HitCount;
            effect._EffectCountUpgraded = UpgradedHitCount;
        }
    }
}
