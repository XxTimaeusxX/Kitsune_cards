using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable, IBlockable, IDebuffable, IBuffable
{
    public CardDeckManager deckManager;
    public HandUIManager HandUIManager;
    private bool HasDrawn= false;
    private bool hasDiscarded = false;

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
    public TMP_Text armorText;
    public GameObject armorIcon;
    public Animator ArmorUIvfx;

    [Header("Buff settings")]
    private int buffDotTurns = 0;
    private int buffDotDamage = 0;
    private int buffAllEffectsTurns = 0;
    private float buffAllEffectsPercentage = 0f;
    private int buffBlockTurns = 0;
    private float buffBlockPercentage = 1f;

    [Header("Debuff settings")]
    private int activeDoTTurns = 0;
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
        if (damageDebuffTurns > 0)
        {
           /* damageDebuffTurns--;
            GameTurnMessager.instance.ShowMessage($"Player's damage debuff: {damageDebuffTurns} turn(s) remaining.");
            if (damageDebuffTurns == 0)
            {
                damageDebuffMultiplier = 1f;
                GameTurnMessager.instance.ShowMessage("Player's damage debuff has worn off.");
            } */
        }
        if (deckManager.handUIManager != null)
        {
            deckManager.handUIManager.Showbutton();
        }
        HasDrawn = false;
        hasDiscarded = false;
        
            
        Debug.Log("Player now have draw and discard phases.");
        
    }
    public void PendTurn()
    {
        deckManager.OnPlayerEndTurn();
        
    }
    public void OndrawCard()
    {
        if (!HasDrawn)
        {
            deckManager.OnbuttonDrawpress();
            HasDrawn = true;
            if(deckManager.handUIManager != null)
               deckManager.handUIManager.Hidebutton(); // Hide the draw button after drawing cards
           
            Debug.Log("Player has drawn cards.");
        }
        else
        {
            Debug.Log("Player has already drawn cards this turn.");
        }
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
        if (armorText != null) { armorText.text = armor.ToString();armorText.gameObject.SetActive(armor > 0); }
        if (armorIcon != null) armorIcon.SetActive(armor > 0);
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
        // Apply damage debuff if active
        int debuffedAmount = amount;
        if (damageDebuffTurns > 0)
        {
            debuffedAmount = Mathf.RoundToInt(debuffedAmount * damageDebuffMultiplier);
            damageDebuffTurns--;
            if (damageDebuffTurns == 0)
            {
                damageDebuffMultiplier = 1f; // Reset when debuff ends
            }
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
      ArmorUIvfx.SetTrigger("ArmorVFX");
        armor += blockamount;
        UpdateArmorUI();
        Debug.Log($"Player gains {blockamount} block.");
        // Implement block logic here
    }
    public void ApplyReflect(float reflectPercentage)
    {
        Debug.Log($"Player gains {reflectPercentage * 100}% reflect.");
        // Implement reflect logic here
    }
    public void BuffBlock(int turns, int blockamount)
    {
        Debug.Log($"Player's block is increased by {blockamount * 100}% for the next {turns} turns.");
        // Implement block buff logic here
    }
    ///////////// IDeBuffable///////////////
    public void ApplyDoT(int turns, int damageAmount)
    {
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

    public void ExtendDebuff(int turns)
    {
        if (activeDoTTurns > 0) activeDoTTurns += turns;
        if (damageDebuffTurns > 0) damageDebuffTurns += turns;
        if (stunTurnsRemaining > 0) stunTurnsRemaining += turns;
    }

    public void ApplyDamageDebuff(int turns, float multiplier)
    {
        damageDebuffTurns = turns;
        damageDebuffMultiplier = multiplier;
    }

    public void LoseEnergy(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        UpdateManaUI();
        Debug.Log($"Player loses {amount} mana.");
    }

    public void ApplyStun(int turns)
    {
        stunTurnsRemaining = Mathf.Max(stunTurnsRemaining, turns);
        IsStunned = true;
        
    }

    ///////////// IBuffable///////////////

    public void BuffDoT( int turns, int damagePerTurn)
    {
        Debug.Log($"Player is buffed with DoT for {turns} turns, taking {damagePerTurn} damage each turn.");
        // Implement DoT buff logic here
    }
    public void BuffAllEffects(int turns, float percentage)
    {
        Debug.Log("Player's damage, DoT, block, and buffs are increased by 25% for the next 2 turns.");
        // Implement buff logic here
    }
   
   
   
}
