using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable, IBlockable, IDebuffable, IBuffable
{
    public CardDeckManager deckManager;
    public HandUIManager HandUIManager;
    public Enemy Enemy;
    private bool HasDrawn= false;
    private bool hasDiscarded = false;

    [Header("Particle Effects")]
    public ParticleSystem BuffEffect;
    public ParticleSystem DebuffEffect;
    public ParticleSystem ArmorEffect;
    public ParticleSystem DotEffect;

    [Header("Health")]
    public float PlayerMaxHealth = 100;
    public float currentHealth = 100;
    public TMP_Text healthText;
    public Image Healthbar;

    [Header("Mana")]
    public float maxMana = 10;
    public float currentMana = 2;
    public TMP_Text manaText;
    public Image Manabar;
    // Cap for max mana (will be used if manaCrystalsUI is not assigned)
    public int maxManaCap = 10;

    // New mana crystals UI (optional). Assign in inspector to use crystal filler instead of or in addition to the bar.
    public ManaCrystalsUI manaCrystalsUI;

    // track last initialized crystal count to avoid reinitializing every update
    private int _lastInitializedCrystals = -1;

    [Header("Armor")]
    public int armor = 0;
    public int reflectTurnsRemaining = 0;
    public float reflectPercentage = 1f;
    public Animator ArmorUIvfx;

    [Header("Damage")]
    public Animator DamageVFX;
    public int damageAmount = 0;


    [Header("Status HUD")]
    public StatusIconBar statusHUD; // Drag your statusHUD (with StatusIconBar) here

    [Header("Buff settings")]
    private int buffBlockTurns = 0;
    public int buffDotTurns { get; set; }
    public int buffDotDamage { get; set; }
    public int buffAllEffectsTurns { get; set; }
    public float buffAllEffectsMultiplier { get; set; } = 1f;
    private float buffBlockPercentage = 1f;

    [Header("Debuff settings")]
    public int activeDoTTurns = 0;
    private int activeDoTDamage = 0;
    public int damageDebuffTurns = 0;
    public float damageDebuffMultiplier = 1f;
    public int stunTurnsRemaining = 0;
    public bool IsStunned = false;

    void Start()
    {
        // call methods
        UpdateHealthUI();

        // initialize crystals UI to current max (optional)
        if (manaCrystalsUI != null)
        {
            int intMax = Mathf.Clamp(Mathf.RoundToInt(maxMana), 0, manaCrystalsUI.maxCrystalCap);
            manaCrystalsUI.Initialize(intMax);
            _lastInitializedCrystals = intMax;
        }

        // Ensure starting values respect the cap (use manaCrystalsUI cap if available)
        int cap = manaCrystalsUI != null ? manaCrystalsUI.maxCrystalCap : maxManaCap;
        maxMana = Mathf.Clamp(maxMana, 1, cap);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        UpdateManaUI();
        UpdateArmorUI();
    }
    public void PstartTurn()
    {
        //---- Player Stun upkeep ---
        if (stunTurnsRemaining <= 0) IsStunned = false;
        statusHUD.UpdateStun(stunTurnsRemaining);
        // --- Player DoT upkeep (mirror enemy logic) ---
        if (activeDoTTurns > 0)
        {
            AudioManager.Instance.PlayDoTSFX();
            DotEffect.Play();
            activeDoTTurns--;
            TakeDamage(activeDoTDamage);
            
            // no yield here (this method is not a coroutine). If you want pauses/animations,
            // process DoT from CardDeckManager.StartPlayerTurn coroutine instead.
        }
        //--- Decrement buffAllEffectsTurns at the start of the turn----
        if (buffAllEffectsTurns > 0)
        {
            buffAllEffectsTurns--;
            if (buffAllEffectsTurns == 0)
            {
                buffAllEffectsMultiplier = 1f;
                Debug.Log("Player's all effects buff expired.");
            }
        }
        //--- Decrement damageDebuffTurns at the start of the turn----
        if (damageDebuffTurns > 0)
        {
            damageDebuffTurns--;
            GameTurnMessager.instance.ShowMessage($"Player's damage debuff: {damageDebuffTurns} turn(s) remaining.");
            
            if (damageDebuffTurns == 0)
            {
                damageDebuffMultiplier = 1f;
                GameTurnMessager.instance.ShowMessage("Player's damage debuff has worn off.");
            } 
        }
        if (deckManager.handUIManager != null)
        {
            deckManager.handUIManager.Showbutton();
        }
        HasDrawn = false;
        hasDiscarded = false;

        // Decrement buffBlockTurns at the start of the turn
        if (buffBlockTurns > 0)
        {
            buffBlockTurns--;
            if (buffBlockTurns == 0)
            {
                buffBlockPercentage = 1f; // Reset multiplier when buff ends
                                          // Optionally show a message: GameTurnMessager.instance.ShowMessage("Block buff expired!");
            }
        }
        // Decrement active reflect turns at the start of the turn
        if (reflectTurnsRemaining > 0)
        {
            reflectTurnsRemaining--;
            if (reflectTurnsRemaining == 0)
            {
                reflectPercentage = 0f;
                Debug.Log("Reflect effect expired.");
            }
        }
        // HUD refresh for turn-based statuses (hides when turns == 0)
        statusHUD.UpdateDot(activeDoTDamage, activeDoTTurns);
        statusHUD.UpdateBlockX(buffBlockPercentage, buffBlockTurns);
        statusHUD.UpdateAllX(buffAllEffectsMultiplier, buffAllEffectsTurns);
        statusHUD.UpdateReflect(reflectPercentage, reflectTurnsRemaining);
        Debug.Log("Player now have draw and discard phases.");
        
    }
 
    public void PendTurn()
    {
        StartTurnMana();
        deckManager.OnPlayerEndTurn();
        
    }
 
    public void UpdateHealthUI()
    {
        if (healthText != null) healthText.text = $"{currentHealth}/{PlayerMaxHealth}";
        if (Healthbar != null) Healthbar.fillAmount = PlayerMaxHealth > 0 ? currentHealth / PlayerMaxHealth : 0f;
    }

    // Updated to use the simple ball crystals UI
    public void UpdateManaUI()
    {
        if (manaText != null) manaText.text = $"{currentMana}/{maxMana}";
        if(Manabar !=null)Manabar.fillAmount = maxMana > 0 ? currentMana / maxMana : 0f;

        Debug.Log($"Player.UpdateManaUI on '{name}': currentMana={currentMana}, maxMana={maxMana}");

        // Update crystals UI if present. Uses integer balls and simple instantiate logic.
        if (manaCrystalsUI != null)
        {
            int intMax = Mathf.Clamp(Mathf.RoundToInt(maxMana), 0, manaCrystalsUI.maxCrystalCap);
            // only re-initialize when the max changed to avoid destroying/creating each update
            if (intMax != _lastInitializedCrystals)
            {
                Debug.Log($"Player.UpdateManaUI: Reinitializing crystals for '{name}' to {intMax} (previous {_lastInitializedCrystals})");
                manaCrystalsUI.Initialize(intMax);
                _lastInitializedCrystals = intMax;
            }

            // Use Floor so when currentMana moves down slightly it visibly decreases
            int intCurrent = Mathf.Clamp(Mathf.FloorToInt(currentMana), 0, intMax);
            Debug.Log($"Player.UpdateManaUI: calling ShowBalls({intCurrent}) on manaCrystalsUI for '{name}'");
            manaCrystalsUI.ShowBalls(intCurrent);
        }
        else
        {
            Debug.LogWarning($"Player.UpdateManaUI: manaCrystalsUI is NULL on Player '{name}'");
        }
    }
    public void UpdateArmorUI()
    {
       // RefreshStatusHUD();
         statusHUD.UpdateBlock(armor);
    }
    ///////////// mana phases ///////////////
    public void StartTurnMana()
    {
        // Increase player's max mana by 1 at start of turn, but clamp to the configured cap (default 10)
        int cap = manaCrystalsUI != null ? manaCrystalsUI.maxCrystalCap : maxManaCap;

        // If maxMana is less than cap, increment it; otherwise leave unchanged.
        if (maxMana < cap)
        {
            maxMana = maxMana + 1;
        }

        // Refill current mana to the player's current max (common design), but ensure it's not above cap.
        maxMana = Mathf.Min(maxMana, cap);
        currentMana = Mathf.Min(currentMana, maxMana);
        currentMana = maxMana; // refill to max at turn start

        // ensure crystals reflect the new max/current
        UpdateManaUI();
    }

    public void SpendMana(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        UpdateManaUI();
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }

    ///////////// IDamageable///////////////   
    public void TakeDamage(int amount)
    {
        AudioManager.Instance.PlayAttackSFX();
        DamageVFX.SetTrigger("ClawSlash");
        // Apply damage debuff if active
        int debuffedAmount = amount;
        if (damageDebuffTurns > 0)
        {
            debuffedAmount = Mathf.RoundToInt(debuffedAmount * damageDebuffMultiplier);
           // damageDebuffTurns--;
            
        }
        // Reflect logic
        if (reflectTurnsRemaining > 0 && Enemy != null)
        {
            Debug.Log("Reflect condition triggered.");
            int reflectedDamage = Mathf.RoundToInt(debuffedAmount * reflectPercentage);
            Enemy.TakeDamage(reflectedDamage);
            Debug.Log($"Reflected {reflectedDamage} damage to enemy!");
        }
        int damageAfterArmor = debuffedAmount;
        if (armor > 0)
        {
            if (armor >= amount)
            {
                armor -= amount;
                damageAfterArmor = 0;
            }
            else
            {
                damageAfterArmor -= armor;
                armor = 0;
            }
            UpdateArmorUI();
            
        }
        
        if (damageAfterArmor > 0)
        {
            currentHealth -= damageAfterArmor;
            UpdateHealthUI();
            
            // Check for defeat
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                UpdateHealthUI();
                if (deckManager != null)
                {
                    deckManager.OnPlayerLose();
                }
            }
        }
      

        Debug.Log($"Player takes {amount} damage. Health: {currentHealth}");
    }
    ///////////// IBlockable///////////////

    public void ApplyBlock(int blockamount)
    {
        int finalBlock = blockamount;
        if (buffAllEffectsTurns > 0)
        {
            finalBlock = Mathf.RoundToInt(blockamount * buffAllEffectsMultiplier);
        }
        if (buffBlockTurns > 0)
        {
            finalBlock = Mathf.RoundToInt(blockamount * buffBlockPercentage);         
        }
        armor += finalBlock;
        UpdateArmorUI();
        ArmorEffect.Play();
        AudioManager.Instance.PlayBlockSFX();
    }

    public void ApplyReflect(int blockAmount, int turns, float reflectPercent)
    {
        ApplyBlock(blockAmount);
        reflectTurnsRemaining = turns;
        reflectPercentage = reflectPercent;
        ArmorEffect.Play();
        AudioManager.Instance.PlayReflectSFX();
        statusHUD.UpdateReflect(reflectPercentage,reflectTurnsRemaining);
    }

    ///////////// IBuffable///////////////

    public void BuffDoT(int turns, int BonusDamage )
    {
        activeDoTTurns += turns;
        activeDoTDamage += BonusDamage;

        DebuffEffect.Play();
        AudioManager.Instance.PlayDeBuffSFX();
    }
    public void BuffAllEffects(int turns, float multiplier)
    {
        buffAllEffectsTurns += turns;
        buffAllEffectsMultiplier = multiplier;
        BuffEffect.Play();
        AudioManager.Instance.PlaySuperBuffSFX();
        statusHUD.UpdateAllX(buffAllEffectsMultiplier,buffAllEffectsTurns);
        // GameTurnMessager.instance.ShowMessage($"All debuffs extended by {turns} turns.");      
        // Implement buff logic here
    }

    
    public void ExtendDebuff(int turns)
    {
        DebuffEffect.Play();
        AudioManager.Instance.PlayDeBuffSFX();
        if (activeDoTTurns > 0) activeDoTTurns += turns;
        if (damageDebuffTurns > 0) damageDebuffTurns += turns;
        if (stunTurnsRemaining > 0) stunTurnsRemaining += turns;
        statusHUD.UpdateDot(activeDoTDamage, activeDoTTurns);
        statusHUD.UpdateWeaken(damageDebuffMultiplier, damageDebuffTurns);
        statusHUD.UpdateStun(stunTurnsRemaining);
        GameTurnMessager.instance.ShowMessage($"All debuffs extended by {turns} turns.");
    }
    public void BuffBlock(int turns, float blockamount)
    {
        buffBlockTurns = turns;
        buffBlockPercentage = blockamount;
        BuffEffect.Play();
        AudioManager.Instance.PlayBuffSFX();
        GameTurnMessager.instance.ShowMessage($"Player's block cards value are doubled for 2 turns.");
        statusHUD.UpdateBlockX(buffBlockPercentage, buffBlockTurns);
    }
    ///////////// IDeBuffable///////////////
    public void ApplyDoT(int turns, int damageAmount)
    {
        AudioManager.Instance.PlayDeBuffSFX();
        DebuffEffect.Play();
        activeDoTTurns += turns;
        activeDoTDamage += damageAmount;
        statusHUD.UpdateDot(activeDoTDamage, activeDoTTurns);
        GameTurnMessager.instance.ShowMessage($"Player takes {damageAmount} DoT for {turns} turns.");
    }

    public void TripleDoT()
    {
        if (activeDoTTurns > 0)
        {
            activeDoTDamage *= 3;
            AudioManager.Instance.PlayDeBuffSFX();
            DebuffEffect.Play();
            statusHUD.UpdateDot(activeDoTDamage, activeDoTTurns);
            GameTurnMessager.instance.ShowMessage($"Player's DoT damage is tripled.");
        }
    }
    public void ApplyDamageDebuff(int turns, float multiplier)
    {
        ArmorEffect.Play();
        damageDebuffTurns = turns;
        damageDebuffMultiplier = multiplier;
        AudioManager.Instance.PlayDeBuffSFX();
        statusHUD.UpdateWeaken(damageDebuffMultiplier, damageDebuffTurns);
        GameTurnMessager.instance.ShowMessage($"Player's damage is reduced by {(1 - multiplier) * 100}% for {turns} turns.");
    }

    public void LoseEnergy(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        UpdateManaUI();
        GameTurnMessager.instance.ShowMessage($"Player loses {amount} mana.");
        AudioManager.Instance.PlayDeBuffSFX();
    }
    
    public void ApplyStun(int turns)
    {
        stunTurnsRemaining = Mathf.Max(stunTurnsRemaining, turns);
        IsStunned = true;
        AudioManager.Instance.PlayDeBuffSFX();
        statusHUD.UpdateStun(stunTurnsRemaining);
        GameTurnMessager.instance.ShowMessage($"Player is stunned for {turns} turns.");
    }

    
}
