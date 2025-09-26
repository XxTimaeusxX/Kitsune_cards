using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbilityManager : MonoBehaviour
{
    public void ExecuteCardAbility(CardData card, Enemy enemy, Player player)
    {
        ManaCostandEffect ability =default;

        switch (card.elementType)
        {
            case CardData.ElementType.Fire:
                ability = card.FireAbilities[card.selectedManaAndEffectIndex]; break;
            case CardData.ElementType.Water:
                ability = card.WaterAbilities[card.selectedManaAndEffectIndex]; break;
            case CardData.ElementType.Earth:
                ability = card.EarthAbilities[card.selectedManaAndEffectIndex]; break;
            case CardData.ElementType.Air:
                ability = card.AirAbilities[card.selectedManaAndEffectIndex]; break;
            default:
                Debug.LogWarning("Unknown element type.");
                return;
        }

        switch (ability.Type)
        {
            // Example: Damage logic
            // Fire 5AP: Deal 8 damage
            // Earth 1AP: Deal 2 damage
            // Earth 10AP: Deal 32 damage and stun
            // Air 2AP: Deal 8 damage
            case AbilityType.Damage:
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 5) { enemy.TakeDamage(8); }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 10) { enemy.TakeDamage(32); enemy.ApplyStun(1); }
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 2) { enemy.TakeDamage(8); }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 1) { enemy.TakeDamage(2); }
                else
                {
                    Debug.Log("Damage ability not implemented.");
                }
                break;
            case AbilityType.Block:
                // Example: Block logic
                // Water 2AP: Apply 4 block
                // Earth 5AP: Apply 12 block
                // Air 1AP: Apply 2 block
                // Air 10AP: Apply 24 block and reflect
                if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 2)
                    player.ApplyBlock(4);
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 5)
                    player.ApplyBlock(12);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 1)
                    player.ApplyBlock(2);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 10)
                {
                    player.ApplyBlock(24);
                    player.ApplyReflect(0.25f);
                }
                break;
            case AbilityType.Buff:
                // Example: Buff logic
                // Fire 2AP: For next 2 turns, DoT applies +1 more
                // Water 10AP: All effects 25% greater for 2 turns
                // Water 1AP: Add 1 turn to debuff
                // Air 5AP: Block 25% more for 3 turns
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 2)
                    player.BuffDoT(2, 1);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 10)
                    player.BuffAllEffects(2, 1.25f);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 1)
                    enemy.ExtendDebuff(1);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 5)
                    player.BuffBlock(3, 0.25f);
                break;

            case AbilityType.Debuff:
                // Example: Debuff logic
                // Fire 10AP: Triple current DoT
                // Fire 1AP: Apply 2 DoT
                // Earth 2AP: Target loses 3 energy
                // Water 5AP: Target deals 25% less damage for 3 turns
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 10)
                    enemy.TripleDoT();
                else if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 1)
                    enemy.ApplyDoT(2);
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 2)
                    enemy.LoseEnergy(3);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 5)
                    enemy.ApplyDamageDebuff(3, 0.75f);
                break;
        }
    }
}
