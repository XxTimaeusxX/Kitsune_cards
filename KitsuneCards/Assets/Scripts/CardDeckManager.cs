using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
public class CardDeckManager : MonoBehaviour
{
    // Set your per-AP limits here (can be scaled up) = 30 cards based on ap limit rules
    private readonly int oneAPLimit = 16;
    private readonly int twoAPLimit = 12;
    private readonly int fiveAPLimit = 8;
    private readonly int tenAPLimit = 4;

    /////////////////////////////deck features
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

    //////////////////////////// event system

    public event System.Action<CardData> OncardDiscard;

    //////////////////////////// assets and turns
    public Player player;
    public Enemy enemy;
    public GameObject winPanel; // Assign in Inspector
    public TMP_Text resultText;
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }
    public TurnState currentTurn;
    void Start()
    {
        LoadcardsfromResources();
        ShuffleDeck();
        DrawCard(HandstartSize);
        StartPlayerTurn();    
    }
    void LoadcardsfromResources()
    {
        // here we are telling the method to clear list, then load all card data from resources/cards folder and add to list
        cards.Clear();
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");
        // Define mana costs and their limits
        var manaLimits = new (int mana, int limit)[]
        {
        (1, oneAPLimit),
        (2, twoAPLimit),
        (5, fiveAPLimit),
        (10, tenAPLimit)
        };
        // Define all ability types you want to include
        AbilityType[] abilityTypes = new AbilityType[]
        {
          AbilityType.Damage,
          AbilityType.Block,
          AbilityType.Buff,
          AbilityType.Debuff,
  
            // Add more as needed
        };

        // Step 1: Gather all unique cards for each (mana, ability) combination
        var uniqueCardsByCombo = new Dictionary<(int, AbilityType), List<CardData>>();
        foreach (var (mana, limit) in manaLimits)
        {
            foreach (var type in abilityTypes)
            {
                uniqueCardsByCombo[(mana, type)] = new List<CardData>();
            }
        }

        foreach (var card in loadedCards)
        {
            // Check all abilities for each element
            void AddIfMatch(List<ManaCostandEffect> abilities)
            {
                for (int i = 0; i < abilities.Count; i++)
                {
                    var ab = abilities[i];
                    var key = (ab.ManaCost, ab.Type);
                    if (uniqueCardsByCombo.ContainsKey(key) && !uniqueCardsByCombo[key].Contains(card))
                    {
                        uniqueCardsByCombo[key].Add(card);
                    }
                }
            }
            AddIfMatch(card.FireAbilities);
            AddIfMatch(card.WaterAbilities);
            AddIfMatch(card.EarthAbilities);
            AddIfMatch(card.AirAbilities);
        }

        // Step 2: Add one of each unique card to the deck
        List<CardData> initialDeck = new List<CardData>();
        foreach (var kvp in uniqueCardsByCombo)
        {
            foreach (var card in kvp.Value)
            {
                if (initialDeck.Count < deckSize)
                    initialDeck.Add(card);
            }
        }

        // Step 3: Duplicate cards up to the AP limit for each (mana, ability) combo
        cards.AddRange(initialDeck);
        foreach (var ((mana, type), cardList) in uniqueCardsByCombo)
        {
            int alreadyAdded = 0;
            foreach (var card in cardList)
            {
                alreadyAdded += initialDeck.FindAll(c => c == card).Count;
            }
            int toAdd = 0;
            foreach (var (m, l) in manaLimits)
            {
                if (m == mana) toAdd = l - alreadyAdded;
            }
            for (int i = 0; i < toAdd && cards.Count < deckSize; i++)
            {
                var card = cardList[i % cardList.Count];
                cards.Add(card);
            }
        }
    }
    
    public void StartPlayerTurn()
    {
        GameTurnMessager.instance.ShowMessage("Player's Turn");
        handUIManager.SetHandCardsInteractable(true);
        player.StartTurnMana();
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
        if (enemy.stunTurnsRemaining > 1)
        {
            enemy.stunTurnsRemaining--;
            Debug.Log($"Enemy is stunned for {enemy.stunTurnsRemaining}and skips its turn!");
            GameTurnMessager.instance.ShowMessage($"Enemy is stunned for {enemy.stunTurnsRemaining}, turn skipped.");
            yield return new WaitForSeconds(2f); // Wait for 2 seconds to let player see the message
            OnEnemyEndTurn();
          //  return;
        }    
        else if(enemy.stunTurnsRemaining < 1)
        { 
            currentTurn = TurnState.EnemyTurn;
            enemy.EstartTurn();
            Debug.Log("Enemy's Turn Started");
            GameTurnMessager.instance.ShowMessage("Enemy's Turn");
        }
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
            winPanel.SetActive(true);
        resultText.text = "Level Complete";
        // Optionally: Stop further game input, etc.
    }
    public void OnPlayerLose()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        resultText.text = "GameOver";
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
