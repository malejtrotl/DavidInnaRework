using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1403 "Sharpening Strike" keeps all of its existing effects, and gains
// an additional IncreaseDamage effect for 2 (3 upgraded).
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
//
// CardData = cardData is required on the new effect: effects loaded from the
// game's assets already have this owner back-reference, but `new CardEffect`
// does not, and a missing owner causes a NullReferenceException at runtime.
public static class Card1403_SharpeningStrike
{
    internal const int SharpeningStrikeCardId = 1403;
    private const int DamageIncrease = 2;
    private const int DamageIncreaseUpgraded = 3;
    private const string NewDescription = "Deal {0} damage to the first enemy. Increase the damage of this and all Strikes in your hand by {1}.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != SharpeningStrikeCardId) return;

        foreach (var existingEffect in cardData._Effects)
        {
            if (existingEffect._Mode == EffectMode.IncreaseDamage)
            {
                return;
            }
        }

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.IncreaseDamage,
            _Targeting = EffectTargeting.Self,
            _EffectValue = DamageIncrease,
            _EffectValueUpgraded = DamageIncreaseUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
