using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandUIManager : MonoBehaviour
{
    public Player player;
    public GameObject buttonprefab;
    public Transform handpanel;
    public CardDeckManager CardDeckManager;

    private void OnEnable()
    {
        CardDeckManager.OncardDiscard += HandleCardDiscarded;
        
    }
    private void OnDisable()
    {
        CardDeckManager.OncardDiscard -= HandleCardDiscarded;
    }
    public void HandleCardDiscarded(CardData card)
    {
        Debug.Log($"[EVENT] Card discarded: {card.CardName}");
        UpdateHandUI();
    }
    public void UpdateHandUI()
    {
        // Clear existing buttons
        foreach (Transform child in handpanel)
        {
            Destroy(child.gameObject);
        }
        // Create new buttons for each card in the player's hand
        for(int i = 0; i < CardDeckManager.playerHand.Count; i++)
        {
            var card = CardDeckManager.playerHand[i];
            GameObject button = Instantiate(buttonprefab, handpanel);
            button.GetComponentInChildren<TMPro.TMP_Text>().text = card.CardName; // assign the card data name to text button


            // Capture the card in a local variable for the lambda
            // get the buttons component for each card, and add a "listener" when button is clicked and then call discardcard() from carddeckmanager
            CardData cardToDiscard = card;
            button.GetComponent<Button>().onClick.AddListener(() => player.OndiscardCard(cardToDiscard));
            
                
            
        }
    }

}
