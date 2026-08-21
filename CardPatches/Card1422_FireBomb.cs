using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1422 "Fire Bomb": increases the hit count from 2 to 3 (4 upgraded).
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
public static class Card1422_FireBomb
{
    internal const int FireBombCardId = 1422;
    private const int HitCount = 3;
    private const int UpgradedHitCount = 4;

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != FireBombCardId) return;

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode != EffectMode.Damage) continue;

            effect._EffectCount = HitCount;
            effect._EffectCountUpgraded = UpgradedHitCount;
        }
    }
}
