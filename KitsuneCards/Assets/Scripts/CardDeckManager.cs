using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TMPro;
public class CardDeckManager : MonoBehaviour
{
    // Set your per-AP limits here (can be scaled up)
    private readonly int oneAPLimit = 12;
    private readonly int twoAPLimit = 9;
    private readonly int fiveAPLimit = 6;
    private readonly int tenAPLimit = 3;

    /////////////////////////////deck features
    public int DeckHandSize;
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
        DisplayDeck();
        DrawCard(8);
        StartPlayerTurn();
      
      //  displayhand();
        
        
    }
    void LoadcardsfromResources()
    {
        // here we are telling the method to clear list, then load all card data from resources/cards folder and add to list
       // cards.Clear();
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
          AbilityType.Block,

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
    private void AddCardsByManaAndType(CardData[] allCards, int mana, AbilityType type, int count)
    {
        // Gather all cards with at least one ability of the given mana and type
        List<CardData> candidates = new List<CardData>();
        foreach (var card in allCards)
        {
            bool hasAbilityOfType = false;
            switch (card.elementType)
            {
                case CardData.ElementType.Fire:
                    hasAbilityOfType = HasAbility(card.FireAbilities, mana, type);
                    break;
                case CardData.ElementType.Water:
                    hasAbilityOfType = HasAbility(card.WaterAbilities, mana, type);
                    break;
                case CardData.ElementType.Earth:
                    hasAbilityOfType = HasAbility(card.EarthAbilities, mana, type);
                    break;
                case CardData.ElementType.Air:
                    hasAbilityOfType = HasAbility(card.AirAbilities, mana, type);
                    break;
            }
            if (hasAbilityOfType)
            {
                // Find the correct index for the Block ability with the right mana
                int abilityIndex = -1;
                switch (card.elementType)
                {
                    case CardData.ElementType.Fire:
                        abilityIndex = card.FireAbilities.FindIndex(ab => ab.ManaCost == mana && ab.Type == type);
                        break;
                    case CardData.ElementType.Water:
                        abilityIndex = card.WaterAbilities.FindIndex(ab => ab.ManaCost == mana && ab.Type == type);
                        break;
                    case CardData.ElementType.Earth:
                        abilityIndex = card.EarthAbilities.FindIndex(ab => ab.ManaCost == mana && ab.Type == type);
                        break;
                    case CardData.ElementType.Air:
                        abilityIndex = card.AirAbilities.FindIndex(ab => ab.ManaCost == mana && ab.Type == type);
                        break;
                }
                card.selectedManaAndEffectIndex = abilityIndex;
                candidates.Add(card);
            }

        }

        // Add up to 'count' cards, cycling if needed
        for (int i = 0; i < count; i++)
        {
            if (candidates.Count == 0) break;
            cards.Add(candidates[i % candidates.Count]);
        }
    }

    private bool HasAbility(List<ManaCostandEffect> abilities, int mana, AbilityType type)
    {
        foreach (var ab in abilities)
            if (ab.ManaCost == mana && ab.Type == type)
                return true;
        return false;
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
    public void StartEnemyturn()
    {
        currentTurn = TurnState.EnemyTurn;
        enemy.StartEnemyTurn();
        handUIManager.SetHandCardsInteractable(false);
        if (handUIManager != null)
            handUIManager.HideEndTurnButton();
            handUIManager.Hidebutton();
        Debug.Log("Enemy's Turn Started");
        GameTurnMessager.instance.ShowMessage("Enemy's Turn");
        // Enemy turn logic here
    }
    public void OnPlayerEndTurn()
    {
        Debug.Log("Player's Turn Ended");
        StartEnemyturn();
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
        DrawCard(DeckHandSize);
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
