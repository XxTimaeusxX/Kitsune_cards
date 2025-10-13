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
        // DisplayDeck();
      //  DebugLogAllCardsInResources();
        DrawCard(HandstartSize);
        StartPlayerTurn();
        
      //  displayhand();
        
        
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
          AbilityType.Debuff,
          AbilityType.Damage,
          AbilityType.Block,
          AbilityType.Buff
            // Add more as needed
        };

        // Loop over all combinations
            foreach (var type in abilityTypes)
            {
            
             foreach (var (mana, limit) in manaLimits)
                 {
                    AddCardsByManaAndType(loadedCards, mana, type, limit);
                 }

            }
    }
    public void AddCardsByManaAndType(CardData[] allCards, int mana, AbilityType type, int count)
    {
        List<CardData> candidates = new List<CardData>();
        foreach (var card in allCards)
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
                Debug.Log($"[DECK] Added: {card.CardName} | {type} at {mana} AP");
            }
        }
        // Add up to 'count' cards, cycling if needed, but do not exceed DeckLimit
        for (int i = 0; i < count && cards.Count < deckSize; i++)
        {
            if (candidates.Count == 0) break;
            cards.Add(candidates[i % candidates.Count]);
            if (cards.Count >= deckSize) break;
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
        
        // Enemy turn logic here

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
        displayhand();
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
    void displayhand()// display player hand by looping through the list
    {
        foreach (var cardData in playerHand)
        {
            Debug.Log($" Card Name: {cardData.CardName}, Card Index: {cardData.CardName}, Attack Strength: {cardData.elementType}");
        }
    }
    void DisplayDeck()// display deck by looping through the list
    {
        // display card data
        foreach (var cardData in cards)
        {
            Debug.Log($"Card Name: {cardData.CardName}, Card Index: {cardData.CardName}, Attack Strength: {cardData.elementType}");

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
