using HarmonyLib;
using Rift;
using DavidInnaRework.CardPatches;

namespace DavidInnaRework.MechanicPatches;

// Real, one-time card mutation hook - covers BOTH effect/numeric data AND
// text (description/name).
//
// ResourcesManager.Initialize() is a coroutine (returns IEnumerator), so its
// actual body runs progressively across multiple frames via the compiler-
// generated state machine's MoveNext() method - a Postfix directly on
// Initialize() itself would only fire the instant the state machine OBJECT
// is created, not when the coroutine finishes actually running.
//
// MoveNext() returns false exactly once, on the frame the coroutine
// finishes. This patches THAT method instead, checking for __result ==
// false, to catch the real "coroutine is done" moment.
//
// Confirmed via temporary diagnostic patches (since removed):
//   - ResourcesManager.CardData (Dictionary<int, CardData>) holds the SAME
//     live CardData instances used everywhere else in the game (confirmed by
//     native IL2CPP pointer comparison against the instance
//     CardData.GetDescription is called on) - unlike MetaInventory.
//     AllCardData, which has 0 real usages anywhere in the assembly and is
//     a dead end (see knowledge doc).
//   - At the moment MoveNext() finishes, ResourcesManager.CardData is fully
//     populated (1530 entries) with real effect/numeric data (_Effects,
//     _EffectValue, _Modifiers, etc. all present and correct).
//   - At that same moment, _Name/_BaseDescription are still empty strings
//     for UNTOUCHED cards - the game populates its own default text lazily,
//     inside CardData.GetDescription's own first call for a given card.
//   - HOWEVER: setting _BaseDescription to OUR OWN text here (before
//     GetDescription ever runs for that card) was confirmed in-game to stick
//     permanently across many subsequent GetDescription calls. This means
//     the game's own lazy-population logic must itself check "is
//     _BaseDescription already non-empty?" before populating it (the same
//     kind of guard we'd otherwise write ourselves) - so it simply leaves
//     our value alone. Text mutations belong here too, not on a separate
//     per-call GetDescription patch.
//
// Unlike the plain field-accessor properties (CardData/AllCardData getters
// and setters - both confirmed unpatchable, "field accessor, it can't be
// patched"), MoveNext() is a genuine native-invoked method on the generated
// state machine class, so it's a legitimate Harmony patch target.
[HarmonyPatch(typeof(ResourcesManager._Initialize_d__12), nameof(ResourcesManager._Initialize_d__12.MoveNext))]
public static class CardDataGameLoadInitializerPatch
{
    private static bool _initialized;

    static void Postfix(ResourcesManager._Initialize_d__12 __instance, bool __result)
    {
        if (_initialized) return;
        if (__result) return; // coroutine still running - wait for the finishing frame.

        _initialized = true;

        var cardDataDict = __instance?.__4__this?.CardData;
        if (cardDataDict == null) return;

        ApplyIfPresent(cardDataDict, Card1400_Improvise.ImproviseCardId, Card1400_Improvise.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1401_ImprovisedStrike.ImprovisedStrikeCardId, Card1401_ImprovisedStrike.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1403_SharpeningStrike.SharpeningStrikeCardId, Card1403_SharpeningStrike.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1407_Ingenuity.IngenuityCardId, Card1407_Ingenuity.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1408_FranticScouring.FranticScouringCardId, Card1408_FranticScouring.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1409_Investigate.InvestigateCardId, Card1409_Investigate.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1411_ResourcefulStrike.ResourcefulStrikeCardId, Card1411_ResourcefulStrike.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1414_AdventurersLog.AdventurersLogCardId, Card1414_AdventurersLog.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1418_CleansingBalm.CleansingBalmCardId, Card1418_CleansingBalm.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1422_FireBomb.FireBombCardId, Card1422_FireBomb.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1423_Caltrops.CaltropsCardId, Card1423_Caltrops.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1426_UnstableDarkstone.UnstableDarkstoneCardId, Card1426_UnstableDarkstone.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1427_InkwellAndQuill.InkwellAndQuillCardId, Card1427_InkwellAndQuill.ApplyMutations);
        ApplyIfPresent(cardDataDict, Card1432_BottledEctoplasm.BottledEctoplasmCardId, Card1432_BottledEctoplasm.ApplyMutations);

        if (NoFatigueDrawOnToolPlayedState.TargetCardId >= 0)
        {
            ApplyIfPresent(cardDataDict, NoFatigueDrawOnToolPlayedState.TargetCardId, NoFatigueDrawOnToolPlayedState.ApplyMutations);
        }
    }

    private static void ApplyIfPresent(
        Il2CppSystem.Collections.Generic.Dictionary<int, CardData> cardDataDict,
        int cardId,
        System.Action<CardData> applyMutation)
    {
        if (!cardDataDict.ContainsKey(cardId)) return;

        applyMutation(cardDataDict[cardId]);
    }
}
