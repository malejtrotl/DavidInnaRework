using Rift;

namespace DavidInnaRework.CardPatches;

// Resourceful Strike, 1411
// Increase the damage of this and Strikes in your hand by 1 per Tool played this turn. Deal 2x3 damage to the first enemy.
// Increase the damage of this and Strikes in your hand by 1 per Tool played this turn. Deal 2x5 damage to the first enemy.
public static class Card1411_ResourcefulStrike
{
    internal const int ResourcefulStrikeCardId = 1411;
    private const int Cost = 1;

    private const int DamageIncreasePerToolPlayed = 1;
    private const int DamageIncreasePerToolPlayedUpgraded = 1;
    private const int DamageValue = 2;
    private const int DamageValueUpgraded = 2;
    private const int DamageCount = 3;
    private const int DamageCountUpgraded = 5;

    private const AppliedEffectType ToolsPlayedThisTurnMarker = AppliedEffectType.COUNT;

    private const string NewDescription =
        "Increase the damage of this and Strikes in your hand by {0} per Tool played this turn. Deal {2} damage to the first enemy.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != ResourcefulStrikeCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.IncreaseDamage,
            _Targeting = EffectTargeting.Self,
            _Modifiers = EffectModifiers.ScalePerStrikePlayed,
            _ConditionEffect = ToolsPlayedThisTurnMarker,
            _EffectValue = DamageIncreasePerToolPlayed,
            _EffectValueUpgraded = DamageIncreasePerToolPlayedUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.IncreaseStrikeDamage,
            _Targeting = EffectTargeting.Self,
            _Modifiers = EffectModifiers.ScalePerStrikePlayed,
            _ConditionEffect = ToolsPlayedThisTurnMarker,
            _EffectValue = DamageIncreasePerToolPlayed,
            _EffectValueUpgraded = DamageIncreasePerToolPlayedUpgraded,
        });

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
