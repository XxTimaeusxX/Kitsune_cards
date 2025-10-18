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

    [Header("Particle Effects")]
    public ParticleSystem DebuffEffect;
    public ParticleSystem BuffEffect;
    public ParticleSystem ArmorEffect;
    public ParticleSystem DotFireEffect;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageArmorDebuffSound;
    public AudioClip TakeDamageSound;
    public AudioClip DoTSound;
    public AudioClip DeBuffSound;


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



    [Header("Enemy Debuffs")]
    public int activeDoTTurns = 0;
    public int activeDoTDamage = 0;
    public int damageDebuffTurns = 0;
    public float damageDebuffMultiplier = 1f;
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
        (1, 22),
        (2, 40),
        (5, 40),
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
                        deckManager.player, // reference to player
                        deckManager.enemy, // reference to enemy
                        deckManager.player, // reference to IDamageable target(player)
                        null,             // reference to IBlockable target(enemy)
                        null,              // reference to IDebuffable target (none for now)
                        null              // reference to IBuffable target (none for now)
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
        Maxmana = Mathf.Min(Maxmana + 1, 20);
        Currentmana = Maxmana;
        UpdateManaUI();
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
        audioSource.PlayOneShot(TakeDamageSound);
        CurrentHealth -= amount;
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

    ///////////// IBuffable///////////////
    public void BuffDoT(int turns, int BonusDamage)
    {

        activeDoTTurns += turns;
        activeDoTDamage += BonusDamage;

        GameTurnMessager.instance.ShowMessage($" extend DoT + {turns} turns.");
    }

    public void BuffAllEffects(int turns, float multiplier)
    {
        throw new System.NotImplementedException();
    }
    
    public void ExtendDebuff(int turns)
    {
        if (activeDoTTurns > 0) activeDoTTurns += turns;
        if (damageDebuffTurns > 0) damageDebuffTurns += turns;
        if (stunTurnsRemaining > 0) stunTurnsRemaining += turns;
        audioSource.PlayOneShot(DeBuffSound);
       // DebuffEffect.Play();
        GameTurnMessager.instance.ShowMessage($"all Enemy's debuffs are extended by {turns} turns!");
    }
    public void BuffBlock(int turns, float BlockAmount)
    {
        throw new System.NotImplementedException();
    }
    ///////////// IDebuffable///////////////

    public void ApplyDoT(int turns, int damageAmount)
    {
        audioSource.PlayOneShot(DeBuffSound);
        activeDoTTurns += turns;
        activeDoTDamage = Mathf.Max(activeDoTDamage, damageAmount);
        DebuffEffect.Play();// play debuff particle effect
        GameTurnMessager.instance.ShowMessage($"Enemy takes {activeDoTDamage} DoT damage for {activeDoTTurns} turns!");
    }
    public void TripleDoT()
    {
            audioSource.PlayOneShot(DeBuffSound);
            activeDoTDamage *= 3;
            DebuffEffect.Play();
            audioSource.PlayOneShot(DeBuffSound);
            GameTurnMessager.instance.ShowMessage($"Enemy's DoT damage is tripled!");
            Debug.Log("Player's DoT damage is tripled.");
        
    }
   

    public void ApplyDamageDebuff(int turns, float multiplier)
    {
        //only players uses this
        damageDebuffTurns = turns;
        damageDebuffMultiplier = multiplier;
    }

    public void LoseEnergy(int amount)
    {
        audioSource.PlayOneShot(DeBuffSound);
        Currentmana = Mathf.Max(0, Currentmana - amount);
        DebuffEffect.Play();
        UpdateManaUI();
        GameTurnMessager.instance.ShowMessage($"Enemy loses {amount} mana!");
    }
    public void ApplyStun(int turns)
    {
        stunTurnsRemaining = Mathf.Max(stunTurnsRemaining, turns);
        IsStunned = true;
        GameTurnMessager.instance.ShowMessage($"Enemy is stunned for {stunTurnsRemaining} turns!");
        // Implement stun logic here
    }
  

    
}
