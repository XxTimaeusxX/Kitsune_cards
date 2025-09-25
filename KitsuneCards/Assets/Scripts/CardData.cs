
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct ManaCostandEffect
{
    public int ManaCost;
    public string EffectDescription;
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

    
    
    public List<ManaCostandEffect> ManaAndEffect = new List<ManaCostandEffect>
    {
        new ManaCostandEffect { ManaCost = 1, EffectDescription = "Apply 2 DoT" },
        new ManaCostandEffect { ManaCost = 2, EffectDescription = "For next 2 turns, DoT applies +1 more" },
        new ManaCostandEffect { ManaCost = 5, EffectDescription = "Deal 8 damage" },
        new ManaCostandEffect { ManaCost = 10, EffectDescription = "Triple current DoT" },
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