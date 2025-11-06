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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip TakeDamageSound;
    public AudioClip damageArmorDebuffSound;
    public AudioClip DoTSound;
    public AudioClip buffSound;

    [Header("Health")]
    public float PlayerMaxHealth = 100;
    public float currentHealth = 100;
    public TMP_Text healthText;
    public Slider Healthbar;

    [Header("Mana")]
    public int maxMana = 2;
    public int currentMana = 2;
    public TMP_Text manaText;
    public Slider Manabar;

    [Header("Armor")]
    public int armor = 0;
    public int reflectTurnsRemaining = 0;
    public float reflectPercentage = 1f;
    public Animator ArmorUIvfx;

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
        UpdateManaUI();
        UpdateArmorUI();
    }
    public void PstartTurn()
    {
        if (buffAllEffectsTurns > 0)
        {
            buffAllEffectsTurns--;
            if (buffAllEffectsTurns == 0)
            {
                buffAllEffectsMultiplier = 1f;
                Debug.Log("Player's all effects buff expired.");
            }
        }
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
        statusHUD.UpdateBlockX(buffBlockPercentage, buffBlockTurns);
        statusHUD.UpdateAllX(buffAllEffectsMultiplier, buffAllEffectsTurns);
        statusHUD.UpdateReflect(reflectPercentage, reflectTurnsRemaining);
        Debug.Log("Player now have draw and discard phases.");
        
    }
   /* private void RefreshStatusHUD()
    {
        // Block
        if (armor > 0) statusHUD.Show(StatusKind.Block, armor.ToString(), null);
        else statusHUD.Hide(StatusKind.Block);

        // Block multiplier (e.g., x2 for N turns)
        if (buffBlockTurns > 0) statusHUD.Show(StatusKind.BlockX, "x" + buffBlockPercentage.ToString("0.#"), buffBlockTurns);
        else statusHUD.Hide(StatusKind.BlockX);

        // All effects multiplier (show current multiplier with remaining turns)
        if (buffAllEffectsTurns > 0) statusHUD.Show(StatusKind.AllX3, "x" + buffAllEffectsMultiplier.ToString("0.#"), buffAllEffectsTurns);
        else statusHUD.Hide(StatusKind.AllX3);

        // DoT amplification (bonus damage over time)
        if (activeDoTTurns > 0 && buffDotDamage > 0) statusHUD.Show(StatusKind.DotAmp, "+" + buffDotDamage.ToString(), activeDoTTurns);
        else statusHUD.Hide(StatusKind.DotAmp);

        // Reflect (percentage with remaining turns)
        if (reflectTurnsRemaining > 0) statusHUD.Show(StatusKind.Reflect, Mathf.RoundToInt(reflectPercentage * 100f) + "%", reflectTurnsRemaining);
        else statusHUD.Hide(StatusKind.Reflect);

        // Weaken (damage debuff multiplier)
        if (damageDebuffTurns > 0) statusHUD.Show(StatusKind.Weaken, "x" + damageDebuffMultiplier.ToString("0.##"), damageDebuffTurns);
        else statusHUD.Hide(StatusKind.Weaken);
    }*/
    public void PendTurn()
    {
        StartTurnMana();
        deckManager.OnPlayerEndTurn();
        
    }
 
    public void UpdateHealthUI()
    {
        if (healthText != null) healthText.text = $"{currentHealth}/{PlayerMaxHealth}";
        if (Healthbar != null) Healthbar.maxValue = PlayerMaxHealth; Healthbar.value = currentHealth;
    }
    public void UpdateManaUI()
    {
        if (manaText != null) manaText.text = $"{currentMana}/{maxMana}";
        if(Manabar !=null)Manabar.maxValue = maxMana; Manabar.value = currentMana;

    }
    public void UpdateArmorUI()
    {
       // RefreshStatusHUD();
         statusHUD.UpdateBlock(armor);
    }
    ///////////// mana phases ///////////////
    public void StartTurnMana()
    {
        // Example: gain 1 mana per turn, up to maxMana
        maxMana = Mathf.Min(maxMana +1, 20);
        currentMana = maxMana; // For testing, set to max mana
        UpdateManaUI();
    }

    public void SpendMana(int amount)
    {
        currentMana -= amount;
        UpdateManaUI();
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }

    ///////////// IDamageable///////////////   
    public void TakeDamage(int amount)
    {
        audioSource.PlayOneShot(TakeDamageSound);
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
        //ArmorUIvfx.SetTrigger("ArmorVFX");
        armor += finalBlock;
        UpdateArmorUI();
        ArmorEffect.Play();
        audioSource.PlayOneShot(damageArmorDebuffSound);
        Debug.Log($"Player gains {blockamount} block.");
        // Implement block logic here
    }

    public void ApplyReflect(int blockAmount, int turns, float reflectPercent)
    {
        ApplyBlock(blockAmount);
        reflectTurnsRemaining = turns;
        reflectPercentage = reflectPercent;
        ArmorEffect.Play();
        audioSource.PlayOneShot(buffSound);
        Debug.Log($"Player gains {reflectPercentage * 100}% reflect.");
        statusHUD.UpdateReflect(reflectPercentage,reflectTurnsRemaining);
        // Implement reflect logic here
    }

    ///////////// IBuffable///////////////

    public void BuffDoT(int turns, int BonusDamage )
    {
        activeDoTTurns += turns;
        activeDoTDamage += BonusDamage;
        // buffDotTurns += turns;
        // int totalTurns = 2 + activeDoTTurns;

        BuffEffect.Play();
        audioSource.PlayOneShot(buffSound);
        // Implement DoT buff logic here
    }
    public void BuffAllEffects(int turns, float multiplier)
    {
        buffAllEffectsTurns += turns;
        buffAllEffectsMultiplier = multiplier;
        BuffEffect.Play();
        audioSource.PlayOneShot(buffSound);
        statusHUD.UpdateAllX(buffAllEffectsMultiplier,buffAllEffectsTurns);
        // GameTurnMessager.instance.ShowMessage($"All debuffs extended by {turns} turns.");      
        // Implement buff logic here
    }

    
    public void ExtendDebuff(int turns)
    {
        BuffEffect.Play();
        audioSource.PlayOneShot(buffSound);
        if (activeDoTTurns > 0) activeDoTTurns += turns;
        if (damageDebuffTurns > 0) damageDebuffTurns += turns;
        if (stunTurnsRemaining > 0) stunTurnsRemaining += turns;
    }
    public void BuffBlock(int turns, float blockamount)
    {
        buffBlockTurns = turns;
        buffBlockPercentage = blockamount;
        BuffEffect.Play();
        audioSource.PlayOneShot(buffSound);
        GameTurnMessager.instance.ShowMessage($"Player's block cards value are doubled for 2 turns.");
        Debug.Log($"Player's block cards value are doubled for this turn.");
        statusHUD.UpdateBlockX(buffBlockPercentage, buffBlockTurns);
        // Implement block buff logic here
    }
    ///////////// IDeBuffable///////////////
    public void ApplyDoT(int turns, int damageAmount)
    {
        Debug.Log("dddddddddddoooottt");
        statusHUD.UpdateDot(damageAmount, turns);
        activeDoTTurns += turns;
        activeDoTDamage = Mathf.Max(activeDoTDamage, damageAmount);
    }

    public void TripleDoT()
    {
        if (activeDoTTurns > 0)
        {
            activeDoTDamage *= 3;
            Debug.Log("Player's DoT damage is tripled.");
        }
    }
    public void ApplyDamageDebuff(int turns, float multiplier)
    {
        ArmorEffect.Play();
        damageDebuffTurns = turns;
        damageDebuffMultiplier = multiplier;
         audioSource.PlayOneShot(damageArmorDebuffSound);
        GameTurnMessager.instance.ShowMessage($"next 3 turns Enemy deals 80% less damage.");
    }

    public void LoseEnergy(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        UpdateManaUI();
        GameTurnMessager.instance.ShowMessage($"Player loses {amount} mana.");
        audioSource.PlayOneShot(damageArmorDebuffSound);
        Debug.Log($"Player loses {amount} mana.");
    }
    
    public void ApplyStun(int turns)
    {
        stunTurnsRemaining = Mathf.Max(stunTurnsRemaining, turns);
        IsStunned = true;
        
    }

    
}
