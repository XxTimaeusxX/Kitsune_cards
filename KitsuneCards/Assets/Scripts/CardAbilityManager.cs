using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardAbilityManager : MonoBehaviour
{
    public void ExecuteCardAbility(CardData card,Player player, Enemy enemy, IDamageable Opponent, IBlockable TargetBlock,IBuffable TargetBuff, IDebuffable TargetDebuff)
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
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 5) 
                {
                    int baseDamage = 12;
                    int damage = baseDamage;
                    if (player.buffAllEffectsTurns > 0)
                    {
                        damage = Mathf.RoundToInt(baseDamage * player.buffAllEffectsMultiplier);
                    }
                    Opponent.TakeDamage(damage);
                     
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 10) 
                {
                    int baseDamage = 32;
                    int damage = baseDamage;
                    if (player.buffAllEffectsTurns > 0)
                    {
                        damage = Mathf.RoundToInt(baseDamage * player.buffAllEffectsMultiplier);
                    }
                    Opponent.TakeDamage(damage); TargetDebuff.ApplyStun(3);
                }
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 2)
                {
                    int baseDamage = 8;
                    int damage = baseDamage;
                    if (player.buffAllEffectsTurns > 0)
                    {
                        damage = Mathf.RoundToInt(baseDamage * player.buffAllEffectsMultiplier);
                    }
                    Opponent.TakeDamage(damage);
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 1)
                {
                    int baseDamage = 2;
                    int damage = baseDamage;
                    if (player.buffAllEffectsTurns > 0)
                    {
                        damage = Mathf.RoundToInt(baseDamage * player.buffAllEffectsMultiplier);
                    }
                    Opponent.TakeDamage(damage);
                }
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
                // Air 10AP: Apply 24 block and reflect all damage for 2 t
                if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 2)
                    TargetBlock.ApplyBlock(4);
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 5)
                    TargetBlock.ApplyBlock(12);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 1)
                    TargetBlock.ApplyBlock(2);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 10)
                {
                  //  TargetBlock.ApplyBlock(24);
                    TargetBlock.ApplyReflect(24,2,.50f);
                }
                break;
            case AbilityType.Buff:
                // Example: Buff logic
                // Fire 2AP: For next 2 turns, DoT applies +1 more
                // Water 10AP: DAmage dealt, DoT applied, block applied are x3 for 2 turns
                // Water 1AP: Add 1 turn to debuff
                // Air 5AP: Block cards are doubled in amount for 2 turns
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 2)
                {
                    TargetBuff.BuffDoT(2, 1);
                    if (enemy.activeDoTTurns > 0)
                    {
                        enemy.activeDoTTurns +=2;
                        enemy.activeDoTDamage += 1;
                        
                        GameTurnMessager.instance.ShowMessage($"Player's DoT buff + 2 turns +1damage");
                        
                    }
                    
                }
                    
                
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 10)
                    TargetBuff.BuffAllEffects(2, 2f);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 1)
                {
                    TargetBuff.ExtendDebuff(1);
                    // Retroactively extend all debuffs on enemy
                    if (enemy.activeDoTTurns > 0)
                    {
                        enemy.activeDoTTurns += 1;
                        GameTurnMessager.instance.ShowMessage($"Enemy's DoT extended +1 turn");
                    }
                    if (enemy.damageDebuffTurns > 0)
                    {
                        enemy.damageDebuffTurns += 1;
                        GameTurnMessager.instance.ShowMessage($"Enemy's damage debuff extended by 1 turn due to buff!");
                    }
                    if(enemy.stunTurnsRemaining > 0)
                    {
                        enemy.stunTurnsRemaining += 1;
                        GameTurnMessager.instance.ShowMessage($"Enemy's stun extended by 1 turn due to buff!");
                    }
                }
                    
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 5)
                    TargetBuff.BuffBlock(2, 2);//might change it to percentage instead.
                break;

            case AbilityType.Debuff:
                // Example: Debuff logic
                // Fire 10AP: Triple current DoT
                // Fire 1AP: Apply 2 DoT
                // Earth 2AP: Target loses 3 energy
                // Water 5AP: Target deals 25% less damage for 3 turns
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 10)
                    TargetDebuff.TripleDoT();
                else if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 1)
                {
                    int totalTurns = 2 + player.activeDoTTurns;
                    int baseDot = 3;
                    int dotDamage = baseDot;
                    if (player.buffAllEffectsTurns > 0)
                    {
                        dotDamage = Mathf.RoundToInt(baseDot * player.buffAllEffectsMultiplier);
                    }
                    TargetDebuff.ApplyDoT(totalTurns, dotDamage);
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 2)
                    TargetDebuff.LoseEnergy(3);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 5)
                    player.ApplyDamageDebuff(3, .20f);
                break;
        }
    }
}
