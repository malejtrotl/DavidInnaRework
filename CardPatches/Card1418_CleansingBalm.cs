using Rift;

namespace DavidInnaRework.CardPatches;

// Cleansing Balm, 1418
// Reduce all statuses by -6. Cleanse yourself 2 times.
// Reduce all statuses by -8. Cleanse yourself 3 times.
public static class Card1418_CleansingBalm
{
    internal const int CleansingBalmCardId = 1418;
    private const int ReduceAllStatuses = -6;
    private const int ReduceAllStatusesUpgraded = -8;
    private const int CleanseCount = 2;
    private const int UpgradedCleanseCount = 3;
    private const string NewDescription = "Reduce all statuses by {0}. Cleanse yourself {1} times.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != CleansingBalmCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.ModifyAllStatuses,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ReduceAllStatuses,
            _EffectValueUpgraded = ReduceAllStatusesUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.Cleanse,
            _Targeting = EffectTargeting.Self,
            _EffectValue = CleanseCount,
            _EffectValueUpgraded = UpgradedCleanseCount,
        });

        cardData._BaseDescription = NewDescription;
    }
}
