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
        OshioniStun,
        YukiOnnaFrostStun,
        // Add more later...
    }

    [Header("Ability Selection")]
    public BossAbilityType Ability = BossAbilityType.HealEveryNTurns;

    [Header("Stat Overrides (optional)")]
    [Tooltip("If true, these values override EnemyData for this boss.")]
    public bool OverrideStats = true;
    [Min(1)] public int BossMaxHealth = 120;
    [Min(0)] public int BossMaxMana = 5;
    [Min(0)] public int BossStartMana = 5;

    [Header("Heal Every N Turns")]
    [Min(1)] public int healEveryTurns = 2;
    [Min(1)] public int healAmount = 30;

    [Header("Drain Mana Every N Turns")]
    [Min(1)] public int drainEveryTurns = 3;
    [Min(1)] public int drainAmount = 1;

    [Header("Oshini Stun/ apply dot Every N Turns")]
    [Min(1)] public int stun_and_Dot_EveryTurns = 4;
    [Min(1)] public int stunDuration = 1;
    [Min(1)] public int dotDuration = 3;
    [Min(1)] public int dotDamagePerTurn = 5;

    [Header("Yuki-onna frost stun")]
    [Min(1)] public int frostStunEveryTurns = 4;
    [Min(1)] public int frostStunDuration = 2;
}
