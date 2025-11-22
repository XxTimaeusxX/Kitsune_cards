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
    public List<UpgradeDef> RerollUpgrades()
    {
        // Generate a fresh sampled array (GetDefaultUpgrades already samples Random.Range for values)
        var sampledTemplates = GetDefaultUpgrades();
        var sampledList = new List<UpgradeDef>(sampledTemplates.Length);

        foreach (var template in sampledTemplates)
        {
            var sampled = template; // copy

            // Preserve inspector-assigned Icon if present by matching Title
            if (Upgrades != null && Upgrades.Count > 0)
            {
                var existing = Upgrades.Find(u => !string.IsNullOrEmpty(u.Title) && u.Title == template.Title);
                if (existing.Title != null && existing.Icon != null)
                    sampled.Icon = existing.Icon;
            }
            sampledList.Add(sampled);
        }

        return sampledList;
    }
    private UpgradeDef[] GetDefaultUpgrades()
    {
        int dragonArmor = Random.Range(15, 25); // dragon scale gusoku
        int stoneGazeStuns = Random.Range(2, 5); // stone gaze stun turns
        int emberDot = Random.Range(10, 15); // ember of eternity dot damage
        int emberTurns = Random.Range(1, 4); // ember of eternity turns
        int bakuReflectTurns = Random.Range(4, 6); // baku's retribution turns
        int gakiHealAmount = 0; // gaki's gluttony heal amount 
        int gakiManaLoss = 0; // gaki's gluttony mana loss
        int mushiArmor = Random.Range(20, 25); // mushi's pact armor
        int mushiHealthLoss = Random.Range(10, 15); // mushi's pact health loss
        int searingDot = Random.Range(10, 15); // searing retribution dot damage
        int searingTurns = Random.Range(1,3); // searing retribution turns
        return new UpgradeDef[]
        {
            new UpgradeDef
            {
                Title = "Dragon Scale Gusoku",
                Description = $"Increase Block Amount by {dragonArmor}",
                upgradeType = UpgradeDef.UpgradeType.BlockAmount,
                ValueAmount = dragonArmor,
            },
            new UpgradeDef
            {
                Title = "Stone Gaze",
                Description = $"Stun opponent for {stoneGazeStuns} turns",
                upgradeType = UpgradeDef.UpgradeType.StunAmount,
                TurnValue = stoneGazeStuns,
            },
            new UpgradeDef
            {
                Title = "Ember of Eternity",
                Description = $"apply {emberDot} Dot damage for {emberTurns} turns",
                upgradeType = UpgradeDef.UpgradeType.TripleDot,
                TurnValue = emberTurns,
                ValueAmount = emberDot,
            },
            new UpgradeDef
            {
                Title = "Baku's Retribution",
                Description = $"Reflect %50 of incoming Damage for {bakuReflectTurns} turns",
                upgradeType = UpgradeDef.UpgradeType.ReflectAmount,
                TurnValue = bakuReflectTurns,
                MultiplierAmount = .50f,
            },
             new UpgradeDef
            {
                Title = "Gaki's Gluttony",
                Description = $"Heal to full health,lose all your mana.",
                upgradeType = UpgradeDef.UpgradeType.GakisGluttony,
                TurnValue = gakiHealAmount,
                ValueAmount = gakiManaLoss,
            },
              new UpgradeDef// not implemented yet
            {
                Title = "Mushi's Pact",
                Description = $"recieve {mushiArmor} armor,lose {mushiHealthLoss} health.",
                upgradeType = UpgradeDef.UpgradeType.Mushis_Pact,
                TurnValue = mushiArmor,
                ValueAmount = mushiHealthLoss,
            },
                new UpgradeDef// not implemented yet
            {
                Title = "Searing Retribution",
                Description = $"recieve {searingDot} Dot damage for {searingTurns} turn,enemy receives double.",
                upgradeType = UpgradeDef.UpgradeType.Searing_Retribution,
                TurnValue =  searingTurns,
                ValueAmount = searingDot,
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
                    player.TakeDamage(upgrade.ValueAmount);
                    player.ApplyBlock(upgrade.TurnValue);
                }
               // else if (blockTarget != null) { blockTarget.ApplyBlock(upgrade.TurnValue); player.TakeDamage(upgrade.ValueAmount); }
                    // Not implemented yet
                    break;
            case UpgradeDef.UpgradeType.Searing_Retribution:
                if(enemy != null)
                {
                    enemy.ApplyDoT(upgrade.TurnValue * 2, upgrade.ValueAmount *2); // enemy takes x2
                }
                if(player != null)
                {
                    player.ApplyDoT(upgrade.TurnValue, upgrade.ValueAmount); 
                }
                // Not implemented yet
                break;

            default:
                break;
        }
    }
}
