using Rift;

namespace DavidInnaRework.CardPatches;

// Sharpening Strike, 1403
// Deal 3 damage to the first enemy. Increase the damage of this and all Strikes in your hand by 2.
// Deal 3 damage to the first enemy. Increase the damage of this and all Strikes in your hand by 3.
public static class Card1403_SharpeningStrike
{
    internal const int SharpeningStrikeCardId = 1403;
    private const int DamageValue = 3;
    private const int DamageValueUpgraded = 3;
    private const int DamageIncrease = 2;
    private const int DamageIncreaseUpgraded = 3;
    private const string NewDescription = "Deal {0} damage to the first enemy. Increase the damage of this and all Strikes in your hand by {1}.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != SharpeningStrikeCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Damage,
            _Targeting = EffectTargeting.Melee,
            _EffectValue = DamageValue,
            _EffectValueUpgraded = DamageValueUpgraded,
        });

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
