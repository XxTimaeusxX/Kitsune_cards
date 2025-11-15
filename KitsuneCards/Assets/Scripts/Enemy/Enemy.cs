using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable, IBlockable, IDebuffable, IBuffable
{
    [Header("Config")]
    public EnemyData enemyData; // Assign a ScriptableObject to configure this instance
    [Tooltip("Fallback folder if EnemyData.CardsResourceFolder is empty")]
    public string cardsResourceFolder = "enemy1";

    public CardDeckManager deckManager;
    public CardAbilityManager abilityManager;
    private Player player;
    public List<CardData> enemyCards = new List<CardData>();
    // discard pile for enemy (used for reshuffle)
    public List<CardData> enemyDiscard = new List<CardData>();

    [Header("UI")]
    public Image enemyPortrait;
    public TMP_Text enemyNameText;

    [Header("Particle Effects")]
    public ParticleSystem DebuffEffect;
    public ParticleSystem BuffEffect;
    public ParticleSystem ArmorEffect;
    public ParticleSystem DotFireEffect;

    [Header("Health")]
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
    public TMP_Text enemyhealthText;
    public Image enemyHealthBar;

    [Header("Mana")]
    public int Maxmana = 10;
    public int Currentmana = 5;
    public TMP_Text enemymanaText;
    public Image enemymanaBar;

    [Header("Damage")]
    public Animator DamageVFX;
    public int damageAmount = 0;

    [Header("Status HUD")]
    public StatusIconBar EnemystatusHUD; // Drag your statusHUD (with StatusIconBar) here

    [Header("Enemy Debuffs")]
    public int activeDoTTurns = 0;
    public int activeDoTDamage = 0;
    public int damageDebuffTurns = 0;
    public float damageDebuffMultiplier = 1f;
    public int stunTurnsRemaining = 0;
    public bool IsStunned = false;

    [Header("Boss (optional)")]
    public BossData bossData; // optional; set by CardDeckManager per wave
    public bool AutoInitializeOnStart = false;

    // Add near other private fields
    private int _bossTurnIndex = 0;
    private BossAbilityManager _bossMgr;

    [Header("Transitions")]
    [Tooltip("Seconds to fade the portrait out on death.")]
    public float DeathFadeOutDuration = 7;
    [Tooltip("Seconds to wait after fully faded-out before notifying deck manager.")]
    public float DelayBeforeSpawnSeconds = 15f;
    [Tooltip("Seconds to fade the new portrait in.")]
    public float SpawnFadeInDuration = 2;
    [Tooltip("Fade in the portrait whenever InitializeForBattle runs.")]
    public bool FadeOnInitialize = true;
    // runtime state to avoid double death handling
    private bool _isDying = false;
    void Start()
    {
        if (AutoInitializeOnStart)
        {
            InitializeForBattle();
        }
    }

    // Effective config built from EnemyData (+ optional BossData)
    private struct EffectiveConfig
    {
        public string Name;
        public Sprite Portrait;
        public int MaxHealth;
        public int MaxMana;
        public int StartMana;
    }

    private EffectiveConfig BuildEffectiveConfig()
    {
        var cfg = new EffectiveConfig
        {
            Name = enemyData != null ? enemyData.EnemyName : "Enemy",
            Portrait = enemyData != null ? enemyData.EnemySprite : null,
            MaxHealth = enemyData != null ? Mathf.Max(1, enemyData.MaxHealth) : MaxHealth,
            MaxMana = enemyData != null ? Mathf.Max(0, enemyData.MaxMana) : Maxmana,
            StartMana = enemyData != null ? Mathf.Clamp(enemyData.StartMana, 0, (enemyData != null ? enemyData.MaxMana : Maxmana)) : Currentmana
        };

        // Identity: boss overrides name/sprite if provided
        if (bossData != null)
        {
            if (!string.IsNullOrEmpty(bossData.BossName)) cfg.Name = bossData.BossName;
            if (bossData.BossSprite != null) cfg.Portrait = bossData.BossSprite;

            // Stats: optionally override from boss
            if (bossData.OverrideStats)
            {
                cfg.MaxHealth = Mathf.Max(1, bossData.BossMaxHealth);
                cfg.MaxMana = Mathf.Max(0, bossData.BossMaxMana);
                cfg.StartMana = Mathf.Clamp(bossData.BossStartMana, 0, cfg.MaxMana);
            }
        }

        return cfg;
    }
    public void InitializeForBattle()
    {
        // Apply any non-stat config (like deck folder) from EnemyData
        ApplyEnemyData();

        // Reset transient runtime state (debuffs, VFX, etc.)
        ResetRuntimeState();

        // Build effective config from EnemyData + optional BossData
        var cfg = BuildEffectiveConfig();

        // Assign runtime maxima and currents
        MaxHealth = cfg.MaxHealth;
        Maxmana = cfg.MaxMana;
        CurrentHealth = MaxHealth;
        Currentmana = Mathf.Clamp(cfg.StartMana, 0, Maxmana);

        // Identity
        if (enemyPortrait != null) enemyPortrait.sprite = cfg.Portrait;
        if (enemyNameText != null) enemyNameText.text = string.IsNullOrEmpty(cfg.Name) ? "Enemy" : cfg.Name;

        // Rebuild deck
        enemyDiscard.Clear();
        LoadEnemyCards();
        ShuffleDeck();

        // Refresh UI
        UpdateEnemyHealthUI();
        UpdateManaUI();

        _bossTurnIndex = 0;
        if (bossData != null)
        {
            if (_bossMgr == null) _bossMgr = FindObjectOfType<BossAbilityManager>();
            if (_bossMgr == null) // auto-create if missing
                _bossMgr = new GameObject("BossAbilityManager").AddComponent<BossAbilityManager>();

            _bossMgr.OnSpawn(bossData, this, deckManager != null ? deckManager.player : null);
        }
    }

    private void ResetRuntimeState()
    {
        activeDoTTurns = 0;
        activeDoTDamage = 0;
        damageDebuffTurns = 0;
        damageDebuffMultiplier = 1f;
        stunTurnsRemaining = 0;
        IsStunned = false;

        if (DebuffEffect != null) DebuffEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (BuffEffect != null) BuffEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (ArmorEffect != null) ArmorEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (DotFireEffect != null) DotFireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        EnemystatusHUD.Clear();
    }
    public void OnBossTurnStart()
    {
        if (bossData == null) return;
        if (_bossMgr == null) _bossMgr = FindObjectOfType<BossAbilityManager>();
        if (_bossMgr == null) return;

        _bossTurnIndex++;
        _bossMgr.OnTurnStart(bossData, this, deckManager != null ? deckManager.player : null, _bossTurnIndex);
    }
    // Allow spawners to set data at runtime before Start() runs
    public void SetEnemyData(EnemyData data)
    {
        enemyData = data;
        ApplyEnemyData();
    }
    private void ApplyEnemyData()
    {
        if (enemyData == null) return;

        // Do NOT set stats here (hub shouldn’t hard-code).
        // Keep deck folder and baseline visuals only.
        if (!string.IsNullOrEmpty(enemyData.CardsResourceFolder))
            cardsResourceFolder = enemyData.CardsResourceFolder;
    }

    // Build an enemy deck sized like the player (use deckManager.deckSize when available).
    public void LoadEnemyCards()
    {
        enemyCards.Clear();

        string folder = string.IsNullOrEmpty(cardsResourceFolder) ? "enemy1" : cardsResourceFolder;
        CardData[] loaded = Resources.LoadAll<CardData>(folder);

        // Mana limits (from data or fallback)
        List<EnemyData.ManaLimit> manaLimits =
            (enemyData != null && enemyData.ManaLimits != null && enemyData.ManaLimits.Count > 0)
                ? enemyData.ManaLimits
                : new List<EnemyData.ManaLimit>
                {
                    new EnemyData.ManaLimit(1, 16),
                    new EnemyData.ManaLimit(2, 12),
                    new EnemyData.ManaLimit(5, 8),
                    new EnemyData.ManaLimit(10, 4)
                };

        // Choose ability types: prefer enemyData.PreferredAbilities, but if global mode requests buff/debuff only, honor that.
        AbilityType[] abilityTypes =
            (enemyData != null && enemyData.PreferredAbilities != null && enemyData.PreferredAbilities.Length > 0)
                ? enemyData.PreferredAbilities
                : new AbilityType[] { AbilityType.Block, AbilityType.Damage };
        ////------------- Load enemy cards based on game mode-------------////
        switch (GameModeConfig.CurrentMode)
        {
            case GameMode.BossOnly:
                abilityTypes = new AbilityType[] {AbilityType.Damage };
                break;
            case GameMode.BuffAndDebuff:
                abilityTypes = new AbilityType[] { AbilityType.Buff, AbilityType.Debuff };
                break;
            case GameMode.Regular:
                abilityTypes = new AbilityType[] { AbilityType.Damage };
                break;

        }

        // Deck cap: prefer deckManager.deckSize (player deck size) to keep parity; fallback to 40
        int desiredDeckSize = (deckManager != null) ? Mathf.Max(1, deckManager.deckSize) : 40;
        // Also respect enemyData.MaxDeckSize if present (take min)
        int deckCap = (enemyData != null) ? Mathf.Min(Mathf.Max(1, enemyData.MaxDeckSize), desiredDeckSize) : desiredDeckSize;

        // Pre-bucketize: mana -> type -> pool
        var byManaByType = new Dictionary<int, Dictionary<AbilityType, List<CardData>>>();
        for (int i = 0; i < loaded.Length; i++)
        {
            var card = loaded[i];
            ManaCostandEffect? ab = null;
            int idx = card.selectedManaAndEffectIndex;
            switch (card.elementType)
            {
                case CardData.ElementType.Fire:
                    if (idx >= 0 && idx < card.FireAbilities.Count) ab = card.FireAbilities[idx];
                    break;
                case CardData.ElementType.Water:
                    if (idx >= 0 && idx < card.WaterAbilities.Count) ab = card.WaterAbilities[idx];
                    break;
                case CardData.ElementType.Earth:
                    if (idx >= 0 && idx < card.EarthAbilities.Count) ab = card.EarthAbilities[idx];
                    break;
                case CardData.ElementType.Air:
                    if (idx >= 0 && idx < card.AirAbilities.Count) ab = card.AirAbilities[idx];
                    break;
            }

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

     

        // Use the same default limits as existing code (we'll reuse enemyData.ManaLimits if present)
        // We'll iterate the manaLimits list to add cards up to deckCap
        int added = 0;
        foreach (var ml in manaLimits)
        {
            if (added >= deckCap) break;
            int mana = ml.Mana;
            int limit = ml.Limit;
            if (limit <= 0) continue;

            byManaByType.TryGetValue(mana, out var typesDict);
            if (typesDict == null) continue;

            // Distribute target across allowed abilityTypes
            int basePerType = limit / abilityTypes.Length;
            int remainder = limit % abilityTypes.Length;
            var typesOrder = new List<AbilityType>(abilityTypes);

            for (int i = 0; i < typesOrder.Count && added < deckCap; i++)
            {
                AbilityType type = typesOrder[i];
                int targetForType = basePerType + (i < remainder ? 1 : 0);
                if (targetForType == 0) continue;

                typesDict.TryGetValue(type, out var pool);
                pool = pool ?? new List<CardData>();
                Shuffle(pool);

                int takeUnique = Mathf.Min(targetForType, pool.Count);
                for (int u = 0; u < takeUnique && added < deckCap; u++)
                {
                    enemyCards.Add(pool[u]);
                    added++;
                }

                int shortfall = targetForType - takeUnique;
                if (shortfall > 0 && pool.Count > 0)
                {
                    // Round-robin duplicates for shortfall
                    int idx = 0;
                    for (int s = 0; s < shortfall && added < deckCap; s++)
                    {
                        enemyCards.Add(pool[idx]);
                        idx++;
                        if (idx >= pool.Count) idx = 0;
                        added++;
                    }
                }
            }

            // If still short in this bucket, fill from any allowed pools
            int remaining = limit - (basePerType * abilityTypes.Length + remainder);
            if (added >= deckCap) break;
            if (added < deckCap)
            {
                var anyPool = new List<CardData>();
                foreach (var allowedType in typesOrder)
                {
                    if (typesDict.TryGetValue(allowedType, out var p) && p != null && p.Count > 0)
                        anyPool.AddRange(p);
                }

                if (anyPool.Count > 0)
                {
                    Shuffle(anyPool);
                    int need = Mathf.Min(deckCap - added, limit); // conservative fill
                    int rr = 0;
                    for (int n = 0; n < need && added < deckCap; n++)
                    {
                        enemyCards.Add(anyPool[rr]);
                        rr++;
                        if (rr >= anyPool.Count) rr = 0;
                        added++;
                    }
                }
            }
        }

        // If still under deckCap (rare), repeat any available card to reach deckCap
        if (enemyCards.Count < deckCap)
        {
            var any = new List<CardData>();
            foreach (var kv in byManaByType)
            {
                foreach (var kv2 in kv.Value)
                {
                    if (kv2.Value != null && kv2.Value.Count > 0)
                        any.AddRange(kv2.Value);
                }
            }
            if (any.Count > 0)
            {
                Shuffle(any);
                int idx = 0;
                while (enemyCards.Count < deckCap)
                {
                    enemyCards.Add(any[idx]);
                    idx++;
                    if (idx >= any.Count) idx = 0;
                }
            }
        }
    }
    private static void Shuffle<T>(IList<T> list)
    {
        if (list == null || list.Count <= 1) return;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    void ShuffleDeck()
    {
        // shuffle card data
        for (int i = enemyCards.Count - 1; i > 0; i--)
        {
            int shuffle = Random.Range(0, i + 1);
            var temp = enemyCards[i];
            enemyCards[i] = enemyCards[shuffle];
            enemyCards[shuffle] = temp;
        }
    }

    // Ensure deck has cards, reshuffle discard when threshold reached or deck empty
    private void MaybeReshuffleEnemyDeck()
    {
        if (deckManager == null)
        {
            // fallback: if deck empty and discard has cards, always reshuffle
            if (enemyCards.Count == 0 && enemyDiscard.Count > 0)
            {
                enemyCards.AddRange(enemyDiscard);
                enemyDiscard.Clear();
                ShuffleDeck();
                Debug.Log("Enemy reshuffled discard into deck (no deckManager).");
            }
            return;
        }

        int threshold = deckManager.reshuffleThreshold;
        bool autoReshuffle = deckManager.autoReshuffle;

        if ((enemyCards.Count <= threshold || enemyCards.Count == 0) && enemyDiscard.Count > 0 && autoReshuffle)
        {
            enemyCards.AddRange(enemyDiscard);
            enemyDiscard.Clear();
            ShuffleDeck();
            Debug.Log("Enemy reshuffled discard into deck.");
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
            // Reshuffle if needed before selecting playable card
            MaybeReshuffleEnemyDeck();

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
                        deckManager.enemy,             // reference to IBlockable target(enemy)
                        deckManager.enemy, // reference to IBuffable target (none for now)
                        deckManager.player             // reference to IDebuffable target (none for now)
                    );
                }
                // move played card to discard
                enemyCards.Remove(cardToPlay);
                enemyDiscard.Add(cardToPlay);

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
        if (enemyHealthBar != null)
        {
            float normalized = MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
            enemyHealthBar.fillAmount = Mathf.Clamp01(normalized);
        }
    }

    public void UpdateManaUI()
    {
        if (enemymanaText != null) enemymanaText.text = $"{Currentmana}/{Maxmana}";
        if (enemymanaBar != null)
        {

            float normalized = Maxmana > 0 ? (float)Currentmana / Maxmana : 0f;
            enemymanaBar.fillAmount = Mathf.Clamp01(normalized);
        }
    }
    //////////// IDamageable ///////////////
    public void TakeDamage(int amount)
    {
        AudioManager.Instance.PlayAttackSFX();
        DamageVFX.SetTrigger("ClawSlash");
        CurrentHealth -= amount;
        UpdateEnemyHealthUI();
        Debug.Log($"Boss takes {amount} damage. Health: {CurrentHealth}/{MaxHealth}");

        if (CurrentHealth <= 0)
        {
          if (deckManager != null)
            {
               StartCoroutine(HandleDeathTransition());
                return;
            }            
        }

    }

    ///////////// IBlockable///////////////
    public void ApplyBlock(int amount)
    {
        throw new System.NotImplementedException();
    }
    public void ApplyReflect(int blockamount, int turns, float reflectPercentage)
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
        AudioManager.Instance.PlayBuffSFX();
        // DebuffEffect.Play();
        GameTurnMessager.instance.ShowMessage($"all Enemy's debuffs are extended by {turns} turns!");
    }
    public void BuffBlock(int turns, float BlockAmount)
    {
        throw new System.NotImplementedException();
    }
    ///////////// IDEBUFFABLE///////////////

    public void ApplyDoT(int turns, int damageAmount)
    {
        AudioManager.Instance.PlayDoTSFX();
        activeDoTTurns += turns;
        activeDoTDamage = Mathf.Max(activeDoTDamage, damageAmount);
        DebuffEffect.Play();// play debuff particle effect
        EnemystatusHUD.UpdateDot(activeDoTDamage,activeDoTTurns);
        GameTurnMessager.instance.ShowMessage($"Enemy takes {activeDoTDamage} DoT damage for {activeDoTTurns} turns!");
    }
    public void TripleDoT()
    {     
        activeDoTDamage *= 3;
        DebuffEffect.Play();
        AudioManager.Instance.PlayDeBuffSFX();
        GameTurnMessager.instance.ShowMessage($"Enemy's DoT damage is tripled!");
          
    }
   

    public void ApplyDamageDebuff(int turns, float multiplier)
    {
        damageDebuffTurns = turns;
        damageDebuffMultiplier = multiplier;
        AudioManager.Instance.PlayDeBuffSFX();
        DebuffEffect.Play();
        EnemystatusHUD.UpdateWeaken(damageDebuffMultiplier, damageDebuffTurns);
    }

    public void LoseEnergy(int amount)
    {
        AudioManager.Instance.PlayDeBuffSFX();
        Currentmana = Mathf.Max(0, Currentmana - amount);
        DebuffEffect.Play();
        UpdateManaUI();
        GameTurnMessager.instance.ShowMessage($"Enemy loses {amount} mana!");
    }
    public void ApplyStun(int turns)
    {
        stunTurnsRemaining = Mathf.Max(stunTurnsRemaining, turns);
        IsStunned = true;
        EnemystatusHUD.UpdateStun(stunTurnsRemaining);
        GameTurnMessager.instance.ShowMessage($"Enemy is stunned for {stunTurnsRemaining} turns!");
        // Implement stun logic here
    }

    // ---------------------------
    // Fade helpers and routines
    // ---------------------------
    private IEnumerator HandleDeathTransition()
    {
        _isDying = true;

        // stop/clear ongoing effects so they don't float during fade
        if (DebuffEffect != null) DebuffEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (BuffEffect != null) BuffEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (ArmorEffect != null) ArmorEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (DotFireEffect != null) DotFireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // fade-out portrait
        yield return FadeImageAlpha(enemyPortrait, GetPortraitAlphaOrDefault(1f), 0f, Mathf.Max(0.01f, DeathFadeOutDuration));

        // pause before spawning the next enemy
        if (DelayBeforeSpawnSeconds > 0f)
            yield return new WaitForSeconds(DelayBeforeSpawnSeconds);

        // notify the deck manager after transition completes
        if (deckManager != null)
            deckManager.OnEnemyDefeated(this);

        // If this object is re-used and the next sprite is assigned immediately,
        // fade-in the portrait. If this object is destroyed by OnEnemyDefeated, this code won't run (which is fine).
        yield return null; // give a frame for sprite assignment if re-used

        // Ensure we start fade-in from 0 alpha
        EnsurePortraitAlpha(0f);
        yield return FadeImageAlpha(enemyPortrait, 0f, 1f, Mathf.Max(0.01f, SpawnFadeInDuration));

        _isDying = false;
    }

    private IEnumerator FadeInPortrait()
    {
        EnsurePortraitAlpha(0f);
        yield return FadeImageAlpha(enemyPortrait, 0f, 1f, Mathf.Max(0.01f, SpawnFadeInDuration));
    }

    private static IEnumerator FadeImageAlpha(Image image, float from, float to, float duration)
    {
        if (image == null) yield break;

        Color c = image.color;
        c.a = from;
        image.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            image.color = c;
            yield return null;
        }
        c.a = to;
        image.color = c;
    }

    private void EnsurePortraitAlpha(float alpha)
    {
        if (enemyPortrait == null) return;
        var c = enemyPortrait.color;
        c.a = alpha;
        enemyPortrait.color = c;
    }

    private float GetPortraitAlphaOrDefault(float defaultAlpha)
    {
        if (enemyPortrait == null) return defaultAlpha;
        return enemyPortrait.color.a;
    }
}
