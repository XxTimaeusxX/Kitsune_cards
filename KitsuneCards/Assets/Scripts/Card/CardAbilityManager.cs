using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardAbilityManager : MonoBehaviour
{
    public void ExecuteCardAbility(CardData card, Player player, Enemy enemy, IDamageable Opponent, IBlockable TargetBlock, IBuffable TargetBuff, IDebuffable TargetDebuff)
    {
        ManaCostandEffect ability = default;

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

        // Determine who is acting: if Opponent is Player, the actor is Enemy; else actor is Player
        bool actorIsEnemy = Opponent is Player;

        // Attacker-wide multipliers
        float actorAllX = 1f;
        float actorWeaken = 1f;

        if (!actorIsEnemy)
        {
            if (player.buffAllEffectsTurns > 0) actorAllX = player.buffAllEffectsMultiplier;
            if (player.damageDebuffTurns > 0) actorWeaken = player.damageDebuffMultiplier;
        }
        else
        {
            // Enemy currently has no "AllX" buff fields; keep 1f
            if (enemy.damageDebuffTurns > 0) actorWeaken = enemy.damageDebuffMultiplier;
        }

        int ScaleDamage(int baseDamage)
        {
            // apply attacker’s AllX buff and attacker’s damage debuff (weaken)
            float scaled = baseDamage * actorAllX * actorWeaken;
            return Mathf.Max(0, Mathf.RoundToInt(scaled));
        }

        int ScaleDot(int baseDot)
        {
            // DoT magnitude is scaled by the attacker’s AllX buff only (commonly how these work)
            float scaled = baseDot * actorAllX;
            return Mathf.Max(0, Mathf.RoundToInt(scaled));
        }

        switch (ability.Type)
        {
            case AbilityType.Damage:
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 5)
                {
                    int baseDamage = 12;
                    Opponent.TakeDamage(ScaleDamage(baseDamage));
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 10)
                {
                    int baseDamage = 32;
                    Opponent.TakeDamage(ScaleDamage(baseDamage));
                    TargetDebuff.ApplyStun(3);
                }
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 2)
                {
                    int baseDamage = 8;
                    Opponent.TakeDamage(ScaleDamage(baseDamage));
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 1)
                {
                    int baseDamage = 2;
                    Opponent.TakeDamage(ScaleDamage(baseDamage));
                }
                else
                {
                    Debug.Log("Damage ability not implemented.");
                }
                break;

            case AbilityType.Block:
                if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 2)
                    TargetBlock.ApplyBlock(4);
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 5)
                    TargetBlock.ApplyBlock(12);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 1)
                    TargetBlock.ApplyBlock(2);
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 10)
                    TargetBlock.ApplyReflect(24, 2, .50f);
                break;

            case AbilityType.Buff:
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 2)
                {
                    TargetBuff.BuffDoT(2, 1);
                    if (enemy.activeDoTTurns > 0)
                    {
                        enemy.activeDoTTurns += 2;
                        enemy.activeDoTDamage += 1;
                        GameTurnMessager.instance.ShowMessage($"Player's DoT buff + 2 turns +1 damage");
                    }
                }
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 10)
                    TargetBuff.BuffAllEffects(2, 2f);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 1)
                {
                    TargetBuff.ExtendDebuff(1);
                    // Retro-extend on enemy
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
                    if (enemy.stunTurnsRemaining > 0)
                    {
                        enemy.stunTurnsRemaining += 1;
                        GameTurnMessager.instance.ShowMessage($"Enemy's stun extended by 1 turn due to buff!");
                    }
                }
                else if (card.elementType == CardData.ElementType.Air && ability.ManaCost == 5)
                    TargetBuff.BuffBlock(2, 2);
                break;

            case AbilityType.Debuff:
                if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 10)
                    TargetDebuff.TripleDoT();
                else if (card.elementType == CardData.ElementType.Fire && ability.ManaCost == 1)
                {
                    int baseDot = 3;
                    int dotDamage = ScaleDot(baseDot);
                    int totalTurns = 2 + player.activeDoTTurns; // if actor is enemy, this stays 2 (player.activeDoTTurns is 0); keep simple for now
                    TargetDebuff.ApplyDoT(totalTurns, dotDamage);
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 2)
                    TargetDebuff.LoseEnergy(3);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 5)
                    TargetDebuff.ApplyDamageDebuff(3, .50f); // apply weaken to the target (enemy when player casts)
                break;
        }
    }
}
