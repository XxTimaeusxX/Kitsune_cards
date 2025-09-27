using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable, IDebuffable, IBuffable
{
 public CardDeckManager deckManager;
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
  
    public TMP_Text enemyhealthText;
    public Slider enemyHealthBar;

    void Start()
    {
        UpdateEnemyHealthUI();
    }
    public void StartEnemyTurn()
    {
        deckManager.StartCoroutine(EnemyTurnRoutine());
    }
   private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(3f);
        // Enemy attacks player for 5 damage
        if (deckManager.player != null)
        {
            deckManager.player.TakeDamage(5);
        }
        // Enemy turn logic here
        yield return new WaitForSeconds(1f); // Example wait time
        deckManager.OnEnemyEndTurn(); ;
    }

    public void UpdateEnemyHealthUI()
    {
        if (enemyhealthText != null) enemyhealthText.text = $"{CurrentHealth}/{MaxHealth}";
        if(enemyHealthBar != null) enemyHealthBar.maxValue = MaxHealth; enemyHealthBar.value = CurrentHealth;

    }
    //////////// IDamageable ///////////////
    public void TakeDamage(int amount)
    { 
            CurrentHealth -=  amount;
            UpdateEnemyHealthUI();
        Debug.Log($"Boss takes {amount} damage. Health: {CurrentHealth}/{MaxHealth}");
        if(CurrentHealth <= 0)
        {
          if (deckManager != null)
            {
                deckManager.OnPlayerWin();
            }
            
        }

    }
    ///////////// IDebuffable///////////////

    public void ApplyDoT(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyDamageDebuff(int turns, float multiplier)
    {
        throw new System.NotImplementedException();
    }

    public void LoseEnergy(int amount)
    {
        throw new System.NotImplementedException();
    }
    public void ApplyStun(int turns)
    {
        Debug.Log($"Boss is stunned for {turns} turns.");
        // Implement stun logic here
    }
    public void ExtendDebuff(int turns)
    {
        Debug.Log($"Boss debuff extended by {turns} turns.");
        // Implement debuff extension logic here
    }
    public void TripleDoT()
    {
        Debug.Log("Boss DoT effects are tripled.");
        // Implement DoT tripling logic here
    }

    ///////////// IBuffable///////////////
    public void BuffDoT(int turns, int bonusDoT)
    {
        throw new System.NotImplementedException();
    }

    public void BuffAllEffects(int turns, float multiplier)
    {
        throw new System.NotImplementedException();
    }
}
