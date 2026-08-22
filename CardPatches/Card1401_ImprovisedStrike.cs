using Rift;

namespace DavidInnaRework.CardPatches;

// Improvised Strike, 1401
// Deal 2x2 damage to the first enemy. Draw one Improvised Strike when you play a Tool.
// Deal 2x3 damage to the first enemy. Draw one Improvised Strike when you play a Tool.
//
// The draw trigger is provided by the generic NoFatigueDrawOnToolPlayed
// mechanic, configured for this card in Plugin.Load().
public static class Card1401_ImprovisedStrike
{
    internal const int ImprovisedStrikeCardId = 1401;
    private const int DamageValue = 2;
    private const int DamageValueUpgraded = 2;
    private const int DamageCount = 2;
    private const int DamageCountUpgraded = 3;
    private const int Cost = 1;
    private const string NewDescription =
        "Deal {0} damage to the first enemy. Draw one Improvised Strike when you play a Tool.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != ImprovisedStrikeCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Damage,
            _Targeting = EffectTargeting.Melee,
            _EffectValue = DamageValue,
            _EffectValueUpgraded = DamageValueUpgraded,
            _EffectCount = DamageCount,
            _EffectCountUpgraded = DamageCountUpgraded,
        });

        cardData._Cost = Cost;
        cardData._CostUpgraded = Cost;
        cardData._BaseDescription = NewDescription;
    }
}
