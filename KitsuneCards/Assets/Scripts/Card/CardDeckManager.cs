using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CardDeckManager : MonoBehaviour
{
    [Header("APLimit")]
    // Set your per-AP limits here. These are summed to compute the deck size.
    public int oneAPLimit = 16;
    public int twoAPLimit = 12;
    public int fiveAPLimit = 8;
    public int tenAPLimit = 4;

    /////////////////////////////deck features
    [Header("Deck and Hand Settings")]
    public int DrawperHand;
    public int HandstartSize;
    // Kept for Inspector visibility; it is computed from AP limits on Start/Load.
    public int deckSize = 40;
    [SerializeField]
    public HandUIManager handUIManager;
    public List<CardData> cards = new List<CardData>();
    [SerializeField]
    public List<CardData> playerHand = new List<CardData>();
    [SerializeField]
    public List<CardData> discardDeck = new List<CardData>();
    public List<CardData> playerfield = new List<CardData>(); // public getter for discard pile

    [Header("Deck Rules")]
    [Tooltip("If true, tries to add one of each unique card in a mana bucket first, then allows duplicates to reach the bucket limit.")]
    public bool uniqueFirstThenDup = true;

    [Header("Deck Recycling")]
    [Tooltip("If true, when the deck is empty or reaches this threshold, the discard pile is shuffled back into the deck.")]
    public bool autoReshuffle = true;
    [Min(0)]
    public int reshuffleThreshold = 5;

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
        // Ensure deckSize matches the sum of AP limits
        deckSize = oneAPLimit + twoAPLimit + fiveAPLimit + tenAPLimit;

        LoadcardsfromResources();
        Shuffle(cards);
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
        // Build a deck with balanced per-ability quotas per mana bucket.
        // Unique-first; then fill shortfall with well-distributed duplicates (round-robin).
        cards.Clear();
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");

        var manaLimits = new (int mana, int limit)[]
        {
            (1, oneAPLimit),
            (2, twoAPLimit),
            (5, fiveAPLimit),
            (10, tenAPLimit)
        };

        // Ability types we balance across
        AbilityType[] abilityTypes = new AbilityType[] { AbilityType.Damage, AbilityType.Buff, AbilityType.Debuff, AbilityType.Block };

        // Pre-bucketize: mana -> (type -> list of CardData)
        var byManaByType = new Dictionary<int, Dictionary<AbilityType, List<CardData>>>();
        for (int i = 0; i < loadedCards.Length; i++)
        {
            var card = loadedCards[i];
            var ab = GetSelectedAbility(card);
            if (!ab.HasValue) continue;

            int mana = ab.Value.ManaCost;
            AbilityType type = ab.Value.Type;

            if (!byManaByType.TryGetValue(mana, out var byType))
            {
                byType = new Dictionary<AbilityType, List<CardData>>();
                byManaByType[mana] = byType;
            }
            if (!byType.TryGetValue(type, out var list))
            {
                list = new List<CardData>();
                byType[type] = list;
            }
            list.Add(card);
        }

        for (int b = 0; b < manaLimits.Length; b++)
        {
            int mana = manaLimits[b].mana;
            int limit = manaLimits[b].limit;
            if (limit <= 0) continue;

            byManaByType.TryGetValue(mana, out var typesDict);
            if (typesDict == null)
            {
                Debug.LogWarning($"No cards found for mana {mana}. Will attempt to repeat any available cards to reach {limit}, but none exist.");
                continue;
            }

            // Determine per-type quotas for this mana bucket
            int basePerType = limit / abilityTypes.Length;
            int remainder = limit % abilityTypes.Length;

            // Deterministic type order for remainder distribution
            var typesOrder = new List<AbilityType>(abilityTypes);

            int addedInBucket = 0;

            // First: unique-first per type, then fill with per-type duplicates if needed
            for (int i = 0; i < typesOrder.Count; i++)
            {
                AbilityType type = typesOrder[i];
                int targetForType = basePerType + (i < remainder ? 1 : 0);
                if (targetForType == 0) continue;

                typesDict.TryGetValue(type, out var pool);
                pool = pool ?? new List<CardData>();

                // Shuffle the pool to avoid picking the same card first every time
                Shuffle(pool);

                // Unique-first
                int takeUnique = Mathf.Min(targetForType, pool.Count);
                for (int u = 0; u < takeUnique; u++)
                {
                    cards.Add(pool[u]);
                    addedInBucket++;
                }

                int shortfall = targetForType - takeUnique;

                // Fill shortfall with well-distributed duplicates from this type, if any
                if (shortfall > 0 && pool.Count > 0)
                {
                    AddRoundRobin(pool, shortfall, cards);
                    addedInBucket += shortfall;
                }
            }

            // If still short (e.g., some type had zero cards at this mana), fill remaining
            // from any available cards at this mana using round-robin (this may skew type balance if you truly have gaps)
            int remaining = limit - addedInBucket;
            if (remaining > 0)
            {
                var anyPool = new List<CardData>();
                foreach (var kvp in typesDict)
                {
                    if (kvp.Value != null && kvp.Value.Count > 0)
                        anyPool.AddRange(kvp.Value);
                }

                if (anyPool.Count == 0)
                {
                    Debug.LogWarning($"Mana {mana}: no cards available to fill remaining {remaining}. Bucket will be undersized.");
                }
                else
                {
                    Shuffle(anyPool);
                    AddRoundRobin(anyPool, remaining, cards);
                    addedInBucket += remaining;
                }
            }

            if (addedInBucket < limit)
            {
                Debug.LogWarning($"Mana {mana}: Requested {limit}, built {addedInBucket}. Not enough cards; duplicates limited by available pools.");
            }
        }

        // Keep in sync for Inspector visibility
        deckSize = cards.Count;
    }

    private static void AddRoundRobin(List<CardData> pool, int count, List<CardData> destination)
    {
        if (pool == null || pool.Count == 0 || count <= 0) return;
        int idx = 0;
        for (int i = 0; i < count; i++)
        {
            destination.Add(pool[idx]);
            idx++;
            if (idx >= pool.Count) idx = 0;
        }
    }

    // Single generic shuffler used everywhere
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
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
        currentTurn = TurnState.PlayerTurn;
        DrawCard(DrawperHand);
        handUIManager.SetHandCardsInteractable(true);
        Debug.Log("Player's Turn Started");
        player.PstartTurn();
    }
    public IEnumerator StartEnemyturn()
    {
        handUIManager.HideEndTurnButton();
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

    private void RefillDeckFromDiscard()
    {
        if (discardDeck.Count == 0)
            return;

        cards.AddRange(discardDeck);
        discardDeck.Clear();
        Shuffle(cards);
        Debug.Log("Deck refilled from discard and shuffled.");
    }

    public void OnReplayButtonPressed()
    {
        Time.timeScale = 1f; // Resume the game
        AudioListener.pause = false; // Unmute audio
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Auto-reshuffle when empty or when at/under the threshold
            if (autoReshuffle && (cards.Count == 0 || cards.Count <= reshuffleThreshold))
            {
                RefillDeckFromDiscard();
            }

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
                break;
            }
        }
        if (handUIManager != null)
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
