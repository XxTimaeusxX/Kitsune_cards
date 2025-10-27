using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAbilityManager : MonoBehaviour
{
    // Map enum -> handler method
    private Dictionary<BossData.BossAbilityType, System.Action<BossData, Enemy, Player, int>> _abilityHandlers;

    private void Awake()
    {
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        if (_abilityHandlers != null) return;

        _abilityHandlers = new Dictionary<BossData.BossAbilityType, System.Action<BossData, Enemy, Player, int>>
        {
            { BossData.BossAbilityType.HealEveryNTurns, HealEveryNTurns },
            { BossData.BossAbilityType.DrainManaEveryNTurns, DrainManaEveryNTurns },
        };
    }

    // Called once when a boss is initialized
    public void OnSpawn(BossData boss, Enemy enemy, Player player)
    {
        if (boss == null || enemy == null) return;
        // Reserved for on-spawn effects (VFX, one-time buffs, etc.)
    }

    // Called at the start of each enemy turn (after DoT tick, before action)
    public void OnTurnStart(BossData boss, Enemy enemy, Player player, int turnIndex)
    {
        if (!Validate(boss, enemy)) return;

        // Ensure handlers exist (in case Awake order differs)
        if (_abilityHandlers == null) InitializeHandlers();

        if (_abilityHandlers.TryGetValue(boss.Ability, out var handler))
        {
            handler?.Invoke(boss, enemy, player, turnIndex);
        }
    }

    // --------------------
    // Ability Implementations
    // --------------------

    private void HealEveryNTurns(BossData boss, Enemy enemy, Player player, int turnIndex)
    {
        if (!ShouldTriggerEvery(turnIndex, boss.healEveryTurns)) return;

        int before = enemy.CurrentHealth;
        int amount = Mathf.Max(1, boss.healAmount);
        enemy.CurrentHealth = Mathf.Min(enemy.MaxHealth, enemy.CurrentHealth + amount);
        int healed = enemy.CurrentHealth - before;

        if (healed > 0)
        {
            enemy.UpdateEnemyHealthUI();
            if (enemy.BuffEffect != null) enemy.BuffEffect.Play();
            GameTurnMessager.instance.ShowMessage($"Boss regenerates {healed} HP.");
        }
    }

    private void DrainManaEveryNTurns(BossData boss, Enemy enemy, Player player, int turnIndex)
    {
        if (!ShouldTriggerEvery(turnIndex, boss.drainEveryTurns)) return;
        if (player == null) return;

        int amt = Mathf.Max(1, boss.drainAmount);
        player.LoseEnergy(amt);
        GameTurnMessager.instance.ShowMessage($"Boss drains {amt} of your mana.");
    }

    // --------------------
    // Helpers
    // --------------------

    private static bool Validate(BossData boss, Enemy enemy)
        => boss != null && enemy != null && enemy.CurrentHealth > 0;

    private static bool ShouldTriggerEvery(int turnIndex, int n)
        => turnIndex > 0 && (n = Mathf.Max(1, n)) > 0 && (turnIndex % n) == 0;
}
