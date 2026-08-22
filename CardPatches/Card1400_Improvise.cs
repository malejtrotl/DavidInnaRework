using Rift;

namespace DavidInnaRework.CardPatches;

// Improvise, 1400
// Create 1 Tool. Lose 1 mana. If you have no mana, create 2 more Tools.
// Create 1 Tool. Lose 2 mana. If you have no mana, create 3 more Tools.
public static class Card1400_Improvise
{
    internal const int ImproviseCardId = 1400;
    private const int ToolsCreated = 1;
    private const int ManaLost = 1;
    private const int ManaLostUpgraded = 2;
    private const int BonusTools = 2;
    private const int BonusToolsUpgraded = 3;
    private const int Cost = 1;
    private const string NewDescription =
        "Create {0} Tool. Lose {1} mana. If you have no mana, create {2} more Tools.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != ImproviseCardId) return;

        cardData._Effects.Clear();

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToolsCreated,
            _EffectValueUpgraded = ToolsCreated,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.AddMana,
            _Targeting = EffectTargeting.Self,
            _EffectValue = -ManaLost,
            _EffectValueUpgraded = -ManaLostUpgraded,
        });

        cardData._Effects.Add(new CardEffect
        {
            CardData = cardData,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.OnlyIfOutOfMana,
            _Targeting = EffectTargeting.Self,
            _EffectValue = BonusTools,
            _EffectValueUpgraded = BonusToolsUpgraded,
        });

        cardData._Cost = Cost;
        cardData._CostUpgraded = Cost;
        cardData._BaseDescription = NewDescription;
    }
}
