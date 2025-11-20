using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UpgradeDef
{
    //--------- Types of upgrades states available-----------//
    public enum UpgradeType
    {
       BlockAmount,
       StunAmount,
       TripleDot,
       ReflectAmount,
       GakisGluttony,
       Mushis_Pact,
       Searing_Retribution,
    }
    //--------- upgrades available-----------//
    public string Title;
    public string Description;
    public Sprite Icon;
    public UpgradeType upgradeType;
    public int TurnValue;
    public int ValueAmount;
    public float MultiplierAmount;
}

public class UpgradeData : MonoBehaviour
{
    public List<UpgradeDef> Upgrades = new List<UpgradeDef>();
   
    private void Reset()
    {
        EnsureDefaults();
    }
    private void OnValidate()
    {
        EnsureDefaults();
    }

    // Helper: try to find a Player instance from any provided interface
    private Player ResolvePlayer(IBlockable blockTarget, IDebuffable debuffTarget, IBuffable buffTarget, IDamageable opponent)
    {
        if (opponent is Player p1) return p1;
        if (debuffTarget is Player p2) return p2;
        if (buffTarget is Player p3) return p3;
        if (blockTarget is Player p4) return p4;
        return null;
    }

    // Helper: try to find an Enemy instance from any provided interface
    private Enemy ResolveEnemy(IBlockable blockTarget, IDebuffable debuffTarget, IBuffable buffTarget, IDamageable opponent)
    {
        if (opponent is Enemy e1) return e1;
        if (debuffTarget is Enemy e2) return e2;
        if (buffTarget is Enemy e3) return e3;
        if (blockTarget is Enemy e4) return e4;
        return null;
    }

    private void EnsureDefaults()
    {
        var defaults = GetDefaultUpgrades();
        if (Upgrades == null) Upgrades = new List<UpgradeDef>();

        foreach (var def in defaults)
        {
            // match by Title (unique key). Change to a better key if needed.
            bool exists = Upgrades.Exists(u => !string.IsNullOrEmpty(u.Title) && u.Title == def.Title);
            if (!exists)
            {
                Upgrades.Add(def);
            }
        }
    }
    private UpgradeDef[] GetDefaultUpgrades()
    {
        return new UpgradeDef[]
        {
            new UpgradeDef
            {
                Title = "Dragon Scale Gusoku",
                Description = "Increase Block Amount by 20",
                upgradeType = UpgradeDef.UpgradeType.BlockAmount,
                ValueAmount = 20,
            },
            new UpgradeDef
            {
                Title = "Stone Gaze",
                Description = "Stun opponent for 5 turns",
                upgradeType = UpgradeDef.UpgradeType.StunAmount,
                TurnValue = 5,
            },
            new UpgradeDef
            {
                Title = "Ember of Eternity",
                Description = "apply 12 Dot damage for 3 turns",
                upgradeType = UpgradeDef.UpgradeType.TripleDot,
                TurnValue = 3,
                ValueAmount = 12,
            },
            new UpgradeDef
            {
                Title = "Baku's Retribution",
                Description = "Reflect %50 of incoming Damage for 5 turns",
                upgradeType = UpgradeDef.UpgradeType.ReflectAmount,
                TurnValue = 5,
                MultiplierAmount = .50f,
            },
             new UpgradeDef
            {
                Title = "Gaki's Gluttony",
                Description = "Heal to full health,lose all your mana.",
                upgradeType = UpgradeDef.UpgradeType.GakisGluttony,
                TurnValue = 0,
                ValueAmount = 0,
            },
              new UpgradeDef// not implemented yet
            {
                Title = "Mushi's Pact",
                Description = "recieve 25 armor,lose 15 health.",
                upgradeType = UpgradeDef.UpgradeType.Mushis_Pact,
                TurnValue = 25,
                ValueAmount = 15,
            },
                new UpgradeDef// not implemented yet
            {
                Title = "Searing Retribution",
                Description = "recieve 10 Dot damage for 1 turn,enemy receives double.",
                upgradeType = UpgradeDef.UpgradeType.Searing_Retribution,
                TurnValue = 1,
                ValueAmount = 10,
            },
        };
    }
   
    public void UpgradeList()
    {
        EnsureDefaults();
    }

    // Centralized application — resolve concrete targets internally and apply upgrades accordingly.
    public void ApplyUpgradeToTarget(UpgradeDef upgrade, IBlockable blockTarget, IDebuffable DebuffTarget, IBuffable BuffTarget, IDamageable opponent)
    {
        var player = ResolvePlayer(blockTarget, DebuffTarget, BuffTarget, opponent);
        var enemy = ResolveEnemy(blockTarget, DebuffTarget, BuffTarget, opponent);

        switch (upgrade.upgradeType)
        {
            case UpgradeDef.UpgradeType.BlockAmount:
                // Block upgrades should target the player by default
                if (player != null) player.ApplyBlock(upgrade.ValueAmount);
                else if (blockTarget != null) blockTarget.ApplyBlock(upgrade.ValueAmount);
                break;

            case UpgradeDef.UpgradeType.StunAmount:
                // Stun enemy by default, fall back to provided debuff target
                if (enemy != null) enemy.ApplyStun(upgrade.TurnValue);
                else if (DebuffTarget != null) DebuffTarget.ApplyStun(upgrade.TurnValue);
                break;

            case UpgradeDef.UpgradeType.TripleDot:
                // Apply DoT to enemy by default, fall back to provided debuff target
                if (enemy != null) enemy.ApplyDoT(upgrade.TurnValue, upgrade.ValueAmount);
                else if (DebuffTarget != null) DebuffTarget.ApplyDoT(upgrade.TurnValue, upgrade.ValueAmount);
                break;

            case UpgradeDef.UpgradeType.ReflectAmount:
                // Prefer player, else blockTarget
                if (player != null) player.ApplyReflect(0, upgrade.TurnValue, upgrade.MultiplierAmount);
                else if (blockTarget != null) blockTarget.ApplyReflect(0, upgrade.TurnValue, upgrade.MultiplierAmount);
                break;

            case UpgradeDef.UpgradeType.GakisGluttony:
                // Always target the player. Resolve and operate safely.
                if (player != null)
                {
                    // If ValueAmount > 0 heal by that amount, otherwise heal to full
                    if (upgrade.ValueAmount > 0)
                        player.currentHealth = Mathf.Min(player.PlayerMaxHealth, player.currentHealth + upgrade.ValueAmount);
                    else
                        player.currentHealth = player.PlayerMaxHealth;

                    player.UpdateHealthUI();
                    // Remove all mana from player
                    // Prefer using Player.LoseEnergy if available; convert currentMana to int
                    int toLose = Mathf.CeilToInt(player.currentMana);
                    if (toLose > 0)
                        player.LoseEnergy(toLose);
                    else
                    {
                        // fallback in case LoseEnergy has different behavior
                        player.currentMana = 0f;
                        player.UpdateManaUI();
                    }
                }
                break;
            case UpgradeDef.UpgradeType.Mushis_Pact:
                if (player != null)
                {
                    player.ApplyBlock(upgrade.ValueAmount);
                    player.TakeDamage(upgrade.ValueAmount);
                }
               // else if (blockTarget != null) { blockTarget.ApplyBlock(upgrade.TurnValue); player.TakeDamage(upgrade.ValueAmount); }
                    // Not implemented yet
                    break;
            case UpgradeDef.UpgradeType.Searing_Retribution:
                if(enemy != null)
                {
                    enemy.ApplyDoT(2, 20); // enemy takes 20 DoT for 1 turn
                }
                if(player != null)
                {
                    player.ApplyDoT(1, 10); // player takes 10 DoT for 1 turn
                }
                // Not implemented yet
                break;

            default:
                break;
        }
    }
}
