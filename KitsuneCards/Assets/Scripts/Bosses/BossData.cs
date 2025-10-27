using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossDefinition", menuName = "Boss")]
public class BossData : ScriptableObject
{
    [Header("Identity")]
    public string BossName;
    public Sprite BossSprite;

    public enum BossAbilityType
    {
        HealEveryNTurns,
        DrainManaEveryNTurns,
        // Add more later...
    }

    [Header("Ability Selection")]
    public BossAbilityType Ability = BossAbilityType.HealEveryNTurns;

    [Header("Heal Every N Turns")]
    [Min(1)] public int healEveryTurns = 5;
    [Min(1)] public int healAmount = 30;

    [Header("Drain Mana Every N Turns")]
    [Min(1)] public int drainEveryTurns = 3;
    [Min(1)] public int drainAmount = 1;
}
