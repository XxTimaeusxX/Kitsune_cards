
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct ManaCostandEffect
{
    public int ManaCost;
    public AbilityType Type;
    public string EffectDescription;
}
public enum AbilityType
{
    Debuff,
    Damage,
    Buff,
    Block,
}


[CreateAssetMenu(fileName = "CardData", menuName = "Card")]
public class CardData : ScriptableObject
{
    public enum ElementType
    {
        Fire,
        Water,
        Earth,
        Air,
    }

    public List<ManaCostandEffect> FireAbilities = new List<ManaCostandEffect>
    {
        new ManaCostandEffect { ManaCost = 1,Type = AbilityType.Debuff, EffectDescription = "Apply 2 DoT, for three turns" },
        new ManaCostandEffect { ManaCost = 2,Type = AbilityType.Buff, EffectDescription = "For next 2 turns, DoT applies +1 more" },
        new ManaCostandEffect { ManaCost = 5,Type = AbilityType.Damage, EffectDescription = "Deal 8 damage" },
        new ManaCostandEffect { ManaCost = 10,Type = AbilityType.Debuff, EffectDescription = "Triple current DoT" },
    };

    public List<ManaCostandEffect> WaterAbilities = new List<ManaCostandEffect>
    {
        new ManaCostandEffect { ManaCost = 1,Type = AbilityType.Buff, EffectDescription = "add 1 additional turn to any current debuff to target" },
        new ManaCostandEffect { ManaCost = 2,Type = AbilityType.Block, EffectDescription = "Apply 4 block" },
        new ManaCostandEffect { ManaCost = 5,Type = AbilityType.Debuff, EffectDescription = "next 3 turns target deals 25% less damage" },
        new ManaCostandEffect { ManaCost = 10,Type = AbilityType.Buff, EffectDescription = @"for next 2 turns, Damage dealt, DoT applied 
                                                                   block applied, and buffs applied are 25 percent greater" },
    };

    public List<ManaCostandEffect> EarthAbilities = new List<ManaCostandEffect>
    {
        new ManaCostandEffect { ManaCost = 1,Type = AbilityType.Damage, EffectDescription = "Deal 2 pts Damage" },
        new ManaCostandEffect { ManaCost = 2,Type = AbilityType.Debuff, EffectDescription = "Target Loses 3 energy" },
        new ManaCostandEffect { ManaCost = 5,Type = AbilityType.Block, EffectDescription = "Apply 12 block" },
        new ManaCostandEffect { ManaCost = 10,Type = AbilityType.Damage, EffectDescription = "Deal 32 pts Damage and apply stun for 3 turn" },
    };

    public List<ManaCostandEffect> AirAbilities = new List<ManaCostandEffect>
    {
        new ManaCostandEffect { ManaCost = 1,Type = AbilityType.Block, EffectDescription = "Apply 2 block" },
        new ManaCostandEffect { ManaCost = 2,Type = AbilityType.Damage, EffectDescription = "deal 8 pts damage" },
        new ManaCostandEffect { ManaCost = 5,Type = AbilityType.Buff, EffectDescription = "next 3 turns, target deals 25% less damage" },
        new ManaCostandEffect { ManaCost = 10,Type = AbilityType.Block, EffectDescription = "apply 24 block and reflect 25% of damage taken" },
    };

    public string CardName; 
    public Sprite CharacterImage;
    public ElementType elementType;
    public int selectedManaAndEffectIndex;


}

/*
==========================
Card Rules & Deck Limits
==========================

- Each element (Fire, Water, Earth, Air) has the following card distribution:
  each deck can only have certain amount of ap cards (4 cards of 1 mana cards, 3 cards of 2 mana cards, 2 cards of 5 mana cards, 1 card of 10 mana cards)
  Example,  Fire:
        1x, 10 AP card   // Triple current Fire DoT
        2x, 5 AP cards   // Deal 8 damage
        3x, 2 AP cards   // For next 2 turns, DoT applies +1 more
        4x, 1 AP cards   // Apply 2 DoT

    (Repeat similar structure for Water, Earth, Air with their effects)

- Deck-building limits:
    - Only the specified number of each AP card per element is allowed in a deck.
    - Prevents stacking high AP cards for balance.

- AP (ManaCost) values: 1, 2, 5, 10
- Maximum mana per game: 10

- Card effects are determined by AP and element type.
*/