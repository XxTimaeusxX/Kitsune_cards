using System.Collections.Generic;
using UnityEngine;

public class CardDeckManager : MonoBehaviour
{
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
        DrawCard(6);
        StartPlayerTurn();
      
      //  displayhand();
        
        
    }
    void LoadcardsfromResources()
    {
        // here we are telling the method to clear list, then load all card data from resources/cards folder and add to list
        cards.Clear();
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");
        cards.AddRange(loadedCards);

    }
    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("Player's Turn Started");
        player.PstartTurn();
        // Player turn logic here
    }
    public void StartEnemyturn()
    {
        currentTurn = TurnState.EnemyTurn;
        enemy.StartEnemyTurn();
        Debug.Log("Enemy's Turn Started");
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
            Debug.Log($" Card Name: {cardData.CardName}, Card Index: {cardData.id}, Attack Strength: {cardData.elementType}");
        }
    }
    void DisplayDeck()// display deck by looping through the list
    {
        // display card data
        foreach (var cardData in cards)
        {
            Debug.Log($"Card Name: {cardData.CardName}, Card Index: {cardData.id}, Attack Strength: {cardData.elementType}");

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
