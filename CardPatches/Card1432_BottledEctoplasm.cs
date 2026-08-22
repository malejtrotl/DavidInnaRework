using Rift;

namespace DavidInnaRework.CardPatches;

// Bottled Ectoplasm, 1432
// Give Curse (2) to any enemy and trigger it 1 time.
// Give Curse (4) to any enemy and trigger it 1 time.
public static class Card1432_BottledEctoplasm
{
    internal const int BottledEctoplasmCardId = 1432;
    private const int CurseApplied = 2;
    private const int CurseAppliedUpgraded = 4;
    private const int TriggerCount = 1;
    private const int TriggerCountUpgraded = 1;
    private const string NewDescription = "Give Curse ({0}) to any enemy and trigger it {1} time.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != BottledEctoplasmCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.ApplyEffect,
            _AppliedEffect = AppliedEffectType.Curse,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = CurseApplied,
            _EffectValueUpgraded = CurseAppliedUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.TriggerEffect,
            _AppliedEffect = AppliedEffectType.Curse,
            _Targeting = EffectTargeting.Previous,
            _EffectValue = TriggerCount,
            _EffectValueUpgraded = TriggerCountUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
