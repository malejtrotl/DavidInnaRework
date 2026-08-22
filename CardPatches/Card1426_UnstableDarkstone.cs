using Rift;

namespace DavidInnaRework.CardPatches;

// Unstable Darkstone, 1426
// Give Doom (6) to any enemy and Dispel it 2 times. Reduce cost by -1 when you play a non-Mana card.
// Give Doom (6) to any enemy and Dispel it 3 times. Reduce cost by -1 when you play a non-Mana card.
public static class Card1426_UnstableDarkstone
{
    internal const int UnstableDarkstoneCardId = 1426;
    private const int DoomApplied = 6;
    private const int DoomAppliedUpgraded = 6;
    private const int DispelCount = 2;
    private const int DispelCountUpgraded = 3;
    private const int CostReduction = -1;
    private const int CostReductionUpgraded = -1;
    private const string NewDescription = "Give Doom ({0}) to any enemy and Dispel it {1} times. Reduce cost by {2} when you play a non-Mana card.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != UnstableDarkstoneCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.ApplyEffect,
            _AppliedEffect = AppliedEffectType.Doom,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = DoomApplied,
            _EffectValueUpgraded = DoomAppliedUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Dispel,
            _Targeting = EffectTargeting.Previous,
            _EffectValue = DispelCount,
            _EffectValueUpgraded = DispelCountUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.IncreaseCost,
            _Targeting = EffectTargeting.Self,
            _Modifiers = EffectModifiers.TriggerOnNonManaPlayed,
            _EffectValue = CostReduction,
            _EffectValueUpgraded = CostReductionUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
