using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1427 "Inkwell and Quill" is not upgradable by default. This makes it
// upgradable, with the upgraded version costing 2 and its single effect value
// also being 2.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
//
// Upgradability is gated by the CardModifiers.NoUpgrade flag on
// CardData._Modifiers. CardModifiers is a [Flags] bitmask, so the flag is
// cleared with &= ~ rather than by overwriting _Modifiers wholesale — that
// preserves any other flags the card carries.
public static class Card1427_InkwellAndQuill
{
    internal const int InkwellAndQuillCardId = 1427;
    private const int UpgradedCost = 2;
    private const int UpgradedValue = 2;

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != InkwellAndQuillCardId) return;

        // Clear the "cannot be upgraded" flag, leaving other flags intact.
        cardData._Modifiers &= ~CardModifiers.NoUpgrade;
        cardData._CostUpgraded = UpgradedCost;

        foreach (var effect in cardData._Effects)
        {
            effect._EffectValueUpgraded = UpgradedValue;
        }
    }
}
