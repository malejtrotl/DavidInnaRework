using Rift;

namespace DavidInnaRework.CardPatches;

// Fire Bomb, 1422
// Deal 3x3 damage to all enemies. Give them Burn (2).
// Deal 3x4 damage to all enemies. Give them Burn (3).
public static class Card1422_FireBomb
{
    internal const int FireBombCardId = 1422;
    private const int DamageValue = 3;
    private const int DamageValueUpgraded = 3;
    private const int DamageCount = 3;
    private const int DamageCountUpgraded = 4;
    private const int BurnApplied = 2;
    private const int BurnAppliedUpgraded = 3;
    private const string NewDescription = "Deal {0} damage to all enemies. Give them Burn ({1}).";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != FireBombCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Damage,
            _Targeting = EffectTargeting.Monsters,
            _EffectValue = DamageValue,
            _EffectValueUpgraded = DamageValueUpgraded,
            _EffectCount = DamageCount,
            _EffectCountUpgraded = DamageCountUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.ApplyEffect,
            _AppliedEffect = AppliedEffectType.Burn,
            _Targeting = EffectTargeting.Monsters,
            _EffectValue = BurnApplied,
            _EffectValueUpgraded = BurnAppliedUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
