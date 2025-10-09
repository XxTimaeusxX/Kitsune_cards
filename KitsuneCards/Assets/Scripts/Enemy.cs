using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour, IDamageable, IBlockable, IDebuffable, IBuffable
{
 public CardDeckManager deckManager;
    public CardAbilityManager abilityManager;
    public List<CardData> enemyCards = new List<CardData>();

    [Header("Health")]
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
    public TMP_Text enemyhealthText;
    public Slider enemyHealthBar;

    [Header("Mana")]
    public int Maxmana = 10;
    public int Currentmana = 5;
    public TMP_Text enemymanaText;
    public Slider enemymanaBar;

    [Header("Enemy Settings")]
    public int stunTurnsRemaining = 0;
    public bool IsStunned = false;
    void Start()
    {
        LoadEnemyCards();
        ShuffleDeck();
        UpdateEnemyHealthUI();
        UpdateManaUI();
    }
    public void LoadEnemyCards()
    {
        enemyCards.Clear();
        CardData[] loaded = Resources.LoadAll<CardData>("enemy1");

        // Define mana costs and their limits (reuse from CardDeckManager or set boss-specific)
        var manaLimits = new (int mana, int limit)[]
        {
        (1, 12),
        (2, 20),
        (5, 20),
        (10, 6)
        };
        AbilityType[] abilityTypes = new AbilityType[]
        {
        AbilityType.Block,
        AbilityType.Damage,
            // Add more as needed
        };

        //  int deckSize = 10; // Or boss-specific size

        foreach (var type in abilityTypes)
        {
            foreach (var (mana, limit) in manaLimits)
            {
                List<CardData> candidates = new List<CardData>();
                foreach (var card in loaded)
                {
                    ManaCostandEffect? selectedAbility = null;
                    switch (card.elementType)
                    {
                        case CardData.ElementType.Fire:
                            if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.FireAbilities.Count)
                                selectedAbility = card.FireAbilities[card.selectedManaAndEffectIndex];
                            break;
                        case CardData.ElementType.Water:
                            if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.WaterAbilities.Count)
                                selectedAbility = card.WaterAbilities[card.selectedManaAndEffectIndex];
                            break;
                        case CardData.ElementType.Earth:
                            if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.EarthAbilities.Count)
                                selectedAbility = card.EarthAbilities[card.selectedManaAndEffectIndex];
                            break;
                        case CardData.ElementType.Air:
                            if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.AirAbilities.Count)
                                selectedAbility = card.AirAbilities[card.selectedManaAndEffectIndex];
                            break;
                    }

                    if (selectedAbility.HasValue && selectedAbility.Value.ManaCost == mana && selectedAbility.Value.Type == type)
                    {
                        candidates.Add(card);
                    }
                }
                // Add up to 'limit' cards, cycling if needed, but do not exceed a deck size (e.g., 10)
                for (int i = 0; i < limit && enemyCards.Count < 200; i++)
                {
                    if (candidates.Count == 0) break;
                    enemyCards.Add(candidates[i % candidates.Count]);
                    if (enemyCards.Count >= 200) break;
                }
            }
        }
    }

    void ShuffleDeck()
    {
        // shuffle card data
        for (int i = enemyCards .Count - 1; i > 0; i--)
        {
            int shuffle = Random.Range(0, i + 1);
            var temp = enemyCards[i];
            enemyCards[i] = enemyCards[shuffle];
            enemyCards[shuffle] = temp;
        }
    }
    public void EstartTurn()
    {
          
            Maxmana = Mathf.Min(Maxmana + 1, 20);
            Currentmana = Maxmana;
            UpdateManaUI();
            deckManager.StartCoroutine(EnemyLogicRoutine());
                    
    }
    private IEnumerator EnemyLogicRoutine()
    {
        yield return new WaitForSeconds(1f); // Preparation phase

        bool playedAtLeastOne = false;

        // Loop: play as many cards as possible with available mana
        while (true)
        {
            CardData cardToPlay = null;
            ManaCostandEffect? selectedAbility = null;

            // Find the first playable card
            for (int i = 0; i < enemyCards.Count; i++)
            {
                var card = enemyCards[i];
                switch (card.elementType)
                {
                    case CardData.ElementType.Fire:
                        if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.FireAbilities.Count)
                            selectedAbility = card.FireAbilities[card.selectedManaAndEffectIndex];
                        break;
                    case CardData.ElementType.Water:
                        if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.WaterAbilities.Count)
                            selectedAbility = card.WaterAbilities[card.selectedManaAndEffectIndex];
                        break;
                    case CardData.ElementType.Earth:
                        if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.EarthAbilities.Count)
                            selectedAbility = card.EarthAbilities[card.selectedManaAndEffectIndex];
                        break;
                    case CardData.ElementType.Air:
                        if (card.selectedManaAndEffectIndex >= 0 && card.selectedManaAndEffectIndex < card.AirAbilities.Count)
                            selectedAbility = card.AirAbilities[card.selectedManaAndEffectIndex];
                        break;
                }

                if (selectedAbility.HasValue && selectedAbility.Value.ManaCost <= Currentmana)
                {
                    cardToPlay = card;
                    break;
                }
            }

            if (cardToPlay != null && selectedAbility.HasValue)
            {
                playedAtLeastOne = true;
                // Play the card
                Currentmana -= selectedAbility.Value.ManaCost;
                UpdateManaUI();

                GameTurnMessager.instance.ShowMessage($"Enemy plays {cardToPlay.CardName} ({selectedAbility.Value.Type})!");
                yield return new WaitForSeconds(1f);

                if (abilityManager != null && cardToPlay != null)// call the method and assign the target enemy cards affect(player)
                {
                    Debug.Log("executing ability");
                    abilityManager.ExecuteCardAbility(
                        cardToPlay,  
                        deckManager.player, // reference to IDamageable target(player)
                        null,              // reference to IDebuffable target (none for now)
                        deckManager.enemy, // reference to enemy
                        deckManager.player,// reference to player
                        deckManager.enemy // who gets the armor applied.
                    );
                }
                // TODO: Add logic for other ability types

                enemyCards.Remove(cardToPlay);

                yield return new WaitForSeconds(0.5f); // Small delay between plays
            }
            else
            {
                break; // No more playable cards or out of mana
            }
        }

        if (!playedAtLeastOne)
        {
            GameTurnMessager.instance.ShowMessage("Enemy has no playable cards!");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f); // Wait before ending turn

        deckManager.OnEnemyEndTurn();
    }


    public void UpdateEnemyHealthUI()
    {
        if (enemyhealthText != null) enemyhealthText.text = $"{CurrentHealth}/{MaxHealth}";
        if(enemyHealthBar != null) enemyHealthBar.maxValue = MaxHealth; enemyHealthBar.value = CurrentHealth;

    }
    public void UpdateManaUI()
    {
        if (enemymanaText != null) enemymanaText.text = $"{Currentmana}/{Maxmana}";
        if (enemymanaBar != null) enemymanaBar.maxValue = Maxmana; enemymanaBar.value = Currentmana;

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

    ///////////// IBlockable///////////////
    public void ApplyBlock(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyReflect(float percentage)
    {
        throw new System.NotImplementedException();
    }
    public void BuffBlock(int turns, int BlockAmount)
    {
        throw new System.NotImplementedException();
    }


    ///////////// IDebuffable///////////////

    public void ApplyDoT(int turns, int damageAmount)
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
        stunTurnsRemaining = Mathf.Max(stunTurnsRemaining, turns);
        IsStunned = true;
        GameTurnMessager.instance.ShowMessage($"Enemy is stunned for {stunTurnsRemaining} turns!");
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
