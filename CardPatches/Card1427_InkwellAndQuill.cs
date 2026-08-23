using Rift;

namespace DavidInnaRework.CardPatches;

// Inkwell and Quill, 1427
// Create a Temporary copy of 1 non-Temporary card in your hand.
// Create a Temporary copy of 2 non-Temporary card in your hand.
public static class Card1427_InkwellAndQuill
{
    internal const int InkwellAndQuillCardId = 1427;
    private const int UpgradedCost = 4;
    private const int CopiesCreated = 1;
    private const int CopiesCreatedUpgraded = 2;
    private const string NewDescription = "Create a Temporary copy of {0} non-Temporary card in your hand.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != InkwellAndQuillCardId) return;

        // Clear the "cannot be upgraded" flag, leaving other flags intact.
        cardData._Modifiers &= ~CardModifiers.NoUpgrade;
        cardData._CostUpgraded = UpgradedCost;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CopyCard,
            _Targeting = EffectTargeting.Self,
            _EffectValue = CopiesCreated,
            _EffectValueUpgraded = CopiesCreatedUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
