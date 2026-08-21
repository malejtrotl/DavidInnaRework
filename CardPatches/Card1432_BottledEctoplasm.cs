using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1432 "Bottled Ectoplasm" gains a NEW TriggerEffect (Curse) so the
// curse it gives is immediately triggered.
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
//
// CardData = cardData is required on the new effect: effects loaded from the
// game's assets already have this owner back-reference, but `new CardEffect`
// does not, and a missing owner causes a NullReferenceException at runtime.
public static class Card1432_BottledEctoplasm
{
    internal const int BottledEctoplasmCardId = 1432;
    private const string NewDescription = "Give Curse ({0}) to any enemy, then trigger it {1} time.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != BottledEctoplasmCardId) return;

        foreach (var existingEffect in cardData._Effects)
        {
            if (existingEffect._Mode == EffectMode.TriggerEffect
                && existingEffect._AppliedEffect == AppliedEffectType.Curse)
            {
                return;
            }
        }

        var triggerCurseEffect = new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.TriggerEffect,
            _AppliedEffect = AppliedEffectType.Curse,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = 1,
            _EffectValueUpgraded = 1,
        };

        cardData._Effects.Add(triggerCurseEffect);

        cardData._BaseDescription = NewDescription;
    }
}
