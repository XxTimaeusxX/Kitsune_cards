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
                    TargetBuff.BuffDoT(2, 1);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 10)
                    TargetBuff.BuffAllEffects(2, 2f);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 1)
                    TargetBuff.ExtendDebuff(1);
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
                    // Use a fixed base duration (2). Call site decides targets and any extensions.
                    int totalTurns = 2;
                    TargetDebuff.ApplyDoT(totalTurns, dotDamage);
                    GameTurnMessager.instance.ShowMessage($"Applied DoT to target: {dotDamage} for {totalTurns} turns");
                }
                else if (card.elementType == CardData.ElementType.Earth && ability.ManaCost == 2)
                    TargetDebuff.LoseEnergy(3);
                else if (card.elementType == CardData.ElementType.Water && ability.ManaCost == 5)
                    TargetDebuff.ApplyDamageDebuff(3, .50f);
                break;
        }
    }
}
