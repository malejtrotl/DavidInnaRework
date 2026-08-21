using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1411 "Resourceful Strike":
//   - Changes the damage from 3x2 to 2x3, or 2x5 upgraded.
//   - Changes the other effect from scaling per tool in hand to scaling per
//     tool played this turn.
//   - Converts that effect into temporary Powerful, and moves it ahead of
//     the damage effect so the card reads "gain Powerful, then deal damage."
//
// Applied once via ApplyMutations(), invoked from
// MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time. The
// live "value x tools played this turn" computation still happens per-call,
// in MechanicPatches/ToolsPlayedThisTurnModifierEmulation.cs — this file only
// needs to mark the effect with that shared marker convention once.
public static class Card1411_ResourcefulStrike
{
    internal const int ResourcefulStrikeCardId = 1411;
    private const int DamageValue = 2;
    private const int DamageValueUpgraded = 2;
    private const int DamageCount = 3;
    private const int DamageCountUpgraded = 5;
    private const int PowerfulPerToolPlayed = 1;
    private const int PowerfulPerToolPlayedUpgraded = 1;
    private const int Cost = 1;
    private const AppliedEffectType ToolsPlayedThisTurnMarker = AppliedEffectType.COUNT;
    private const string NewDescription = "Gain Powerful ({0}) this turn per Tool played this turn. Deal {1} damage to the first enemy.";

    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != ResourcefulStrikeCardId) return;

        foreach (var effect in cardData._Effects)
        {
            if (effect._Mode == EffectMode.Damage)
            {
                effect._EffectValue = DamageValue;
                effect._EffectValueUpgraded = DamageValueUpgraded;
                effect._EffectCount = DamageCount;
                effect._EffectCountUpgraded = DamageCountUpgraded;
                continue;
            }

            if (effect._Modifiers != EffectModifiers.ScalePerTool) continue;

            // Marker convention for the shared emulation patch
            // (MechanicPatches/ToolsPlayedThisTurnModifierEmulation.cs):
            //   ScalePerStrikePlayed + ConditionEffect=COUNT => ScalePerToolPlayed.
            effect._Mode = EffectMode.ApplyEffectThisTurn;
            effect._AppliedEffect = AppliedEffectType.Powerful;
            effect._Modifiers = EffectModifiers.ScalePerStrikePlayed;
            effect._ConditionEffect = ToolsPlayedThisTurnMarker;
            effect._EffectValue = PowerfulPerToolPlayed;
            effect._EffectValueUpgraded = PowerfulPerToolPlayedUpgraded;
        }

        // The card's two effects are originally ordered [Damage,
        // ScalePerTool]. After converting the second effect to Powerful
        // above, move it ahead of the damage effect so the card reads "gain
        // Powerful, then deal damage."
        if (cardData._Effects.Count == 2 && cardData._Effects[1]._ConditionEffect == ToolsPlayedThisTurnMarker)
        {
            var firstEffect = cardData._Effects[0];
            var secondEffect = cardData._Effects[1];

            cardData._Effects.RemoveAt(1);
            cardData._Effects.RemoveAt(0);
            cardData._Effects.Add(secondEffect);
            cardData._Effects.Add(firstEffect);
        }

        cardData._Cost = Cost;
        cardData._CostUpgraded = Cost;
        cardData._BaseDescription = NewDescription;
    }
}
