using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable, IBlockable, IBuffable
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

    void Start()
    {
        // call methods
        UpdateHealthUI();
        UpdateManaUI();
        UpdateArmorUI();
    }
    public void PstartTurn()
    {
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
    public void BuffBlock(int turns, float percentage)
    {
        Debug.Log($"Player's block is increased by {percentage * 100}% for the next {turns} turns.");
        // Implement block buff logic here
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
    public void ExtendDebuff(int turns)
    {
        Debug.Log($"debuff is extended for the next {turns} turns.");
        // Implement debuff logic here
    }
    ///////////// IDamageable///////////////   
    public void TakeDamage(int amount)
    {
        int damageAfterArmor = amount;
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
}
