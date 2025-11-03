using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CardDeckManager : MonoBehaviour
{
    [Header("APLimit")]
    // Set your per-AP limits here (can be scaled up) = 30 cards based on ap limit rules
    public  int oneAPLimit = 16;
    public  int twoAPLimit = 12;
    public  int fiveAPLimit = 8;
    public  int tenAPLimit = 4;

    /////////////////////////////deck features
    [Header("Deck and Hand Settings")]
    public int DrawperHand;
    public int HandstartSize;
    public int deckSize = 10; // max deck size
    [SerializeField]
    public HandUIManager handUIManager;
    public List<CardData> cards = new List<CardData>();
    [SerializeField]
    public List<CardData> playerHand = new List<CardData>();
    [SerializeField]
    private List<CardData> discardDeck = new List<CardData>();
    public List<CardData> playerfield = new List<CardData>(); // public getter for discard pile

    [Header("Deck Bias")]
    [Tooltip("If true, not all cards are available each scene (random subset).")]
    public bool randomizeAvailability = false;
    [Range(0f, 1f)]
    [Tooltip("Chance a card is available when randomizing.")]
    public float availabilityChance = 0.75f;
    [Tooltip("If true, sorts Damage/Debuff to the top after building the deck.")]
    public bool offenseFirstSort = true;
    [Range(0, 10)]
    [Tooltip("When offense-first sorting, after every N offensive cards, insert one defensive if available. 0 = disabled.")]
    public int interleaveDefensiveEvery = 0;

    //////////////////////////// event system

    public event System.Action<CardData> OncardDiscard;

    //////////////////////////// assets and turns
    public Player player;
    public Enemy enemy;
    public GameObject winPanel; // Assign in Inspector
    public GameObject Victory_UI;
    public GameObject Defeat_UI;
    public TMP_Text resultText;
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }
    public TurnState currentTurn;

    // New: unified wave list (prefer this)
    [System.Serializable]
    public class WaveEntry
    {
        public EnemyData enemyData; // required
        public BossData bossData;   // optional: assign for boss waves
    }

    [Header("Wave Sequence")]
    public List<WaveEntry> waveSequence = new List<WaveEntry>();
    private int currentEnemyIndex = -1;
    // Boss-only sequence (used only when GameMode == BossOnly)
    [Header("Boss Wave Sequence")]
    public List<BossData> bossWaveSequence = new List<BossData>();
    private int currentBossIndex = -1;

    // NEW: pending boss (spawned after the enemy of the same wave)
    private EnemyData _pendingBossBaseEnemyData;
    private BossData _pendingBossData;
    void Start()
    {
        LoadcardsfromResources();
        ShuffleDeck();
        DrawCard(HandstartSize);
        BeginEnemySequence();
        StartPlayerTurn();    
    }


    private void NextEnemy()
    {
        // Boss-only mode: use bossWaveSequence and ignore waveSequence completely
        if (GameModeConfig.CurrentMode == GameMode.BossOnly)
        {
            // No pending base-enemy/boss sequencing in boss-only mode
            _pendingBossBaseEnemyData = null;
            _pendingBossData = null;

            while (true)
            {
                currentBossIndex++;

                if (bossWaveSequence == null || bossWaveSequence.Count == 0 || currentBossIndex >= bossWaveSequence.Count)
                {
                    OnPlayerWin();
                    return;
                }

                var boss = bossWaveSequence[currentBossIndex];
                if (boss == null) continue; // skip empty slots

                enemy.enemyData = null; // rely on boss overrides/defaults
                enemy.bossData = boss;
                enemy.InitializeForBattle();
                return;
            }
        }
        // 1) If a boss is pending from the previous wave entry, spawn it now (do not advance index)
        if (_pendingBossData != null)
        {
            enemy.SetEnemyData(_pendingBossBaseEnemyData); // base for boss (same wave's enemyData)
            enemy.bossData = _pendingBossData;
            enemy.InitializeForBattle();

            // clear pending boss
            _pendingBossBaseEnemyData = null;
            _pendingBossData = null;

            // Player starts; enemy acts after player ends turn
            return;
        }

        // 2) Advance to next wave entry
        currentEnemyIndex++;

        if (waveSequence == null || waveSequence.Count == 0 || currentEnemyIndex >= waveSequence.Count)
        {
            OnPlayerWin();
            return;
        }

        var entry = waveSequence[currentEnemyIndex];

        // invalid entry if neither is set
        if (entry == null || (entry.enemyData == null && entry.bossData == null))
        {
            Debug.LogError("Wave entry is missing both EnemyData and BossData.");
            OnPlayerWin();
            return;
        }

        // Case A: both enemy and boss set -> spawn enemy now, schedule boss next
        if (entry.enemyData != null && entry.bossData != null)
        {
            // spawn enemy now
            enemy.SetEnemyData(entry.enemyData);
            enemy.bossData = null; // ensure this spawn is NOT a boss
            enemy.InitializeForBattle();

            // schedule boss for next NextEnemy call
            _pendingBossBaseEnemyData = entry.enemyData;
            _pendingBossData = entry.bossData;

            return;
        }

        // Case B: enemy only
        if (entry.enemyData != null)
        {
            enemy.SetEnemyData(entry.enemyData);
            enemy.bossData = null;
            enemy.InitializeForBattle();
            return;
        }

        // Case C: boss only (allowed)
        if (entry.bossData != null)
        {
            // Optional: choose a default/base EnemyData if you want, otherwise leave enemy.enemyData as-is
            enemy.enemyData = null; // let boss overrides + defaults apply
            enemy.bossData = entry.bossData;
            enemy.InitializeForBattle();
            return;
        }

        // If you want to immediately give the turn to the enemy:
        //  currentTurn = TurnState.EnemyTurn;
        //  StartCoroutine(StartEnemyturn());
    }
    public void BeginEnemySequence()
    {
        currentEnemyIndex = -1;
        currentBossIndex = -1;
        NextEnemy();
    }
    // Called by Enemy when its health reaches 0
    public void OnEnemyDefeated(Enemy defeated)
    {
        NextEnemy();

    }
    void LoadcardsfromResources()
    {
        // here we are telling the method to clear list, then load all card data from resources/cards folder and add to list
        cards.Clear();
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");

        // Option 1: Randomly limit availability per scene
        if (randomizeAvailability)
        {
            var filtered = new List<CardData>(loadedCards.Length);
            for (int i = 0; i < loadedCards.Length; i++)
            {
                if (Random.value <= availabilityChance)
                    filtered.Add(loadedCards[i]);
            }
            loadedCards = filtered.ToArray();
        }

        // Define mana costs and their limits
        var manaLimits = new (int mana, int limit)[]
        {
            (1, oneAPLimit),
            (2, twoAPLimit),
            (5, fiveAPLimit),
            (10, tenAPLimit)
        };
        // Define all ability types you want to include (Damage/Debuff first biases deck fill)
        AbilityType[] abilityTypes = new AbilityType[]
        {
            AbilityType.Damage,
            AbilityType.Debuff,
            AbilityType.Block,
            AbilityType.Buff,
            // Add more as needed
        };
        // Apply Game Mode filter: BuffOnly -> only Buff cards, disable offense-first sorting
        if (GameModeConfig.CurrentMode == GameMode.BuffAndDebuff)
        {
            abilityTypes = new AbilityType[] { AbilityType.Buff, AbilityType.Debuff };
            offenseFirstSort = false;
            interleaveDefensiveEvery = 0;
        }
        foreach (var (mana, limit) in manaLimits)
        {
            foreach (var type in abilityTypes)
            {
                List<CardData> candidates = new List<CardData>();
                foreach (var card in loadedCards)
                {
                    ManaCostandEffect? selectedAbility = GetSelectedAbility(card);

                    // Only add cards whose selected ability matches the type and mana
                    if (selectedAbility.HasValue && selectedAbility.Value.Type == type && selectedAbility.Value.ManaCost == mana)
                    {
                        candidates.Add(card);
                    }
                }

                // Add up to 'limit' cards, randomly picking from candidates, but do not exceed deckSize
                for (int i = 0; i < limit && cards.Count < deckSize; i++)
                {
                    if (candidates.Count == 0)
                    {
                        // No cards for this type/mana; continue
                        break;
                    }

                    int pick = Random.Range(0, candidates.Count);
                    cards.Add(candidates[pick]);
                }
            }
        }

        // Option 2: Offense-first sort (Damage + Debuff on top), optionally interleave some defensive cards
        if (offenseFirstSort && cards.Count > 1)
        {
            var offense = new List<CardData>(cards.Count);
            var defense = new List<CardData>(cards.Count);

            for (int i = 0; i < cards.Count; i++)
            {
                var ab = GetSelectedAbility(cards[i]);
                if (ab.HasValue && (ab.Value.Type == AbilityType.Damage || ab.Value.Type == AbilityType.Debuff))
                    offense.Add(cards[i]);
                else
                    defense.Add(cards[i]);
            }

            if (interleaveDefensiveEvery > 0)
            {
                var mixed = new List<CardData>(cards.Count);
                int o = 0, d = 0;
                while (o < offense.Count || d < defense.Count)
                {
                    int toTake = interleaveDefensiveEvery;
                    while (toTake-- > 0 && o < offense.Count)
                        mixed.Add(offense[o++]);

                    if (d < defense.Count)
                        mixed.Add(defense[d++]);
                }
                cards.Clear();
                cards.AddRange(mixed);
            }
            else
            {
                offense.AddRange(defense);
                cards.Clear();
                cards.AddRange(offense);
            }
        }
    }

    private ManaCostandEffect? GetSelectedAbility(CardData card)
    {
        ManaCostandEffect? selectedAbility = null;
        int idx = card.selectedManaAndEffectIndex;
        switch (card.elementType)
        {
            case CardData.ElementType.Fire:
                if (idx >= 0 && idx < card.FireAbilities.Count) selectedAbility = card.FireAbilities[idx];
                break;
            case CardData.ElementType.Water:
                if (idx >= 0 && idx < card.WaterAbilities.Count) selectedAbility = card.WaterAbilities[idx];
                break;
            case CardData.ElementType.Earth:
                if (idx >= 0 && idx < card.EarthAbilities.Count) selectedAbility = card.EarthAbilities[idx];
                break;
            case CardData.ElementType.Air:
                if (idx >= 0 && idx < card.AirAbilities.Count) selectedAbility = card.AirAbilities[idx];
                break;
        }
        return selectedAbility;
    }

    public void StartPlayerTurn()
    {
        GameTurnMessager.instance.ShowMessage("Player's Turn");
        handUIManager.SetHandCardsInteractable(true);
       // player.StartTurnMana();
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("Player's Turn Started");
        player.PstartTurn();
        
        // Player turn logic here
    }
    public IEnumerator StartEnemyturn()
    {
        handUIManager.HideEndTurnButton();
        handUIManager.Hidebutton();
        handUIManager.SetHandCardsInteractable(false);
        if (enemy.activeDoTTurns > 0)
        {
            Debug.Log("doteffect");
            GameTurnMessager.instance.ShowMessage($"Enemy takes {enemy.activeDoTDamage} damage, {enemy.activeDoTTurns} DoT turns remaining ");
            enemy.DotFireEffect.Play();
            enemy.audioSource.PlayOneShot(enemy.DoTSound);
            enemy.activeDoTTurns--;
            enemy.TakeDamage(enemy.activeDoTDamage);
            yield return new WaitForSeconds(2f); // Wait for 2 seconds to let player see the message
        }
        // Boss passive tick: after DoT, before stun skip
        if (enemy != null)
            enemy.OnBossTurnStart();
        yield return new WaitForSeconds(1f); // brief pause before stun check
        if (enemy.stunTurnsRemaining > 0)
        {
            
            
                Debug.Log($"Enemy is stunned for {enemy.stunTurnsRemaining} and skips its turn!");
                GameTurnMessager.instance.ShowMessage($"Enemy is stunned for {enemy.stunTurnsRemaining}, turn skipped.");
                yield return new WaitForSeconds(2f);
                enemy.stunTurnsRemaining--;
                OnEnemyEndTurn();
                yield break;
            
            // If stun just ended (now 0), let the enemy act below
        }

        // If not stunned, enemy acts
        currentTurn = TurnState.EnemyTurn;
        enemy.EstartTurn();
        Debug.Log("Enemy's Turn Started");
        GameTurnMessager.instance.ShowMessage("Enemy's Turn");
    }
    public void OnPlayerEndTurn()
    {
        Debug.Log("Player's Turn Ended");
       StartCoroutine(StartEnemyturn());
    }
    public void OnEnemyEndTurn()
    {
        Debug.Log("Enemy's Turn Ended");
        StartPlayerTurn();
    }
    public void OnPlayerWin()
    {
        if (winPanel != null)
        enemy.InitializeForBattle(); // reset enemy for next battle
        StopAllCoroutines();
        enemy.StopAllCoroutines();
        Time.timeScale = 0f; // Pause the game
        AudioListener.pause = true; // Mute all audio
        enemy.audioSource.Stop(); // mute enemy audio
        winPanel.SetActive(true);
        Victory_UI.SetActive(true);
       // resultText.text = "Level Complete";
        // Optionally: Stop further game input, etc.
    }
    public void OnPlayerLose()
    {
        if (winPanel != null)
        enemy.InitializeForBattle(); // reset enemy for next battle
        StopAllCoroutines();
        enemy.StopAllCoroutines();
        Time.timeScale = 0f; // Pause the game
        AudioListener.pause = true; // Mute all audio
        enemy.audioSource.Stop(); // mute enemy audio
        winPanel.SetActive(true);
        Defeat_UI.SetActive(true);
       // resultText.text = "GameOver";
    }
    void ShuffleDeck()
    {
        // shuffle card data
        for (int i = cards.Count - 1; i>0; i-- )
        {
            int shuffle = Random.Range(0, i + 1);
            var temp = cards[i];
            cards[i] = cards[shuffle];
            cards[shuffle] = temp;
        }
    }
   public void OnbuttonDrawpress() // Button Ui will call this function when pressed = drawcard and show card.
    {
        DrawCard(DrawperHand);
        
    }
    public void OnReplayButtonPressed()
    {
        Time.timeScale = 1f; // Resume the game
        AudioListener.pause = false; // Unmute audio
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void DrawCard(int count)
    {
        for(int i = 0; i < count; i++)
        {
            if (cards.Count > 0)// if theres any cards to pick up from deck
            {
                CardData drawnCard = cards[0]; //  draw the top card [0] = top of deck [10] = bottom of deck
                playerHand.Add(drawnCard); // list.add= add to players hand list
                cards.RemoveAt(0); // remove the card from the deck card list
                Debug.Log($"Drew Card: {drawnCard.CardName}");// show prove that it works
            }
            else
            {
                Debug.Log("No more cards to draw!");
            }
        }
        if(handUIManager != null)
        {
            handUIManager.UpdateHandUI(); // Update the hand UI after drawing cards
        }
    }

    public void DiscardCard(CardData card)
    {
        if (playerHand.Contains(card))
        {
            playerHand.Remove(card);
            discardDeck.Add(card);
            Debug.Log($"Discarded Card: {card.CardName}");
            OncardDiscard?.Invoke(card); // Invoke the event if there are subscribers
            if (handUIManager != null)
            {
                handUIManager.UpdateHandUI(); // Update the hand UI after discarding a card
            }
        }
        else
        {
            Debug.Log("Card not found in hand!");
        }
    }
}
