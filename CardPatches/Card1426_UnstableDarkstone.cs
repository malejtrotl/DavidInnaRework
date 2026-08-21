using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1426 "Unstable Darkstone" gains a NEW Dispel effect for 2 (3 upgraded)
// with Ranged targeting. Its existing effects are left untouched.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
//
// CardData = cardData is required on the new effect: effects loaded from the
// game's assets already have this owner back-reference, but `new CardEffect`
// does not, and a missing owner causes a NullReferenceException at runtime.
public static class Card1426_UnstableDarkstone
{
    internal const int UnstableDarkstoneCardId = 1426;
    private const int DispelCount = 2;
    private const int DispelCountUpgraded = 3;
    private const string NewDescription = "Give Doom ({0}) to any enemy and Dispel it {2} times. Reduce cost by {1} when you play a non-Mana card.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != UnstableDarkstoneCardId) return;

        foreach (var existingEffect in cardData._Effects)
        {
            if (existingEffect._Mode == EffectMode.Dispel)
            {
                return;
            }
        }

        var dispelEffect = new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Dispel,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = DispelCount,
            _EffectValueUpgraded = DispelCountUpgraded,
        };

        cardData._Effects.Add(dispelEffect);

        cardData._BaseDescription = NewDescription;
    }
}
