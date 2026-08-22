using Rift;

namespace DavidInnaRework.CardPatches;

// Caltrops, 1423
// Deal 1x3 damage to all enemies. Give them Weak (1).
// Deal 1x4 damage to all enemies. Give them Weak (2).
public static class Card1423_Caltrops
{
    internal const int CaltropsCardId = 1423;
    private const int DamageValue = 1;
    private const int DamageValueUpgraded = 1;
    private const int DamageCount = 3;
    private const int DamageCountUpgraded = 4;
    private const int WeakApplied = 1;
    private const int WeakAppliedUpgraded = 2;
    private const string NewDescription = "Deal {0} damage to all enemies. Give them Weak ({1}).";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != CaltropsCardId) return;

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
            _AppliedEffect = AppliedEffectType.Weak,
            _Targeting = EffectTargeting.Monsters,
            _EffectValue = WeakApplied,
            _EffectValueUpgraded = WeakAppliedUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
