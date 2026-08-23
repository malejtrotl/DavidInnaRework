using Rift;

namespace DavidInnaRework.CardPatches;

// Investigate, 1409
// Choose any enemy. Create 1 Tool, then 2 Tools if it has 3 or more debuff types, then 2 Tools if it has 5 or more.
// Choose any enemy. Create 1 Tool, then 3 Tools if it has 3 or more debuff types, then 3 Tools if it has 5 or more.
public static class Card1409_Investigate
{
    internal const int InvestigateCardId = 1409;

    private const int BaselineTools = 1;
    private const int BaselineToolsUpgraded = 1;
    private const int ToolsIf3PlusDebuffs = 2;
    private const int ToolsIf3PlusDebuffsUpgraded = 3;
    private const int ToolsIf5PlusDebuffs = 2;
    private const int ToolsIf5PlusDebuffsUpgraded = 3;

    private const string NewDescription =
        "Choose any enemy. Create {0} Tool, then {1} Tools if it has 3 or more debuff types, then {2} Tools if it has 5 or more.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != InvestigateCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = BaselineTools,
            _EffectValueUpgraded = BaselineToolsUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.OnlyIfTargetHas3PlusDebuffs,
            _Targeting = EffectTargeting.Previous,
            _EffectValue = ToolsIf3PlusDebuffs,
            _EffectValueUpgraded = ToolsIf3PlusDebuffsUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.OnlyIfTargetHas5PlusDebuffs,
            _Targeting = EffectTargeting.Previous,
            _EffectValue = ToolsIf5PlusDebuffs,
            _EffectValueUpgraded = ToolsIf5PlusDebuffsUpgraded,
        });

        cardData._BaseDescription = NewDescription;
    }
}
