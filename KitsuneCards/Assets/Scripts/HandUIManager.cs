using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandUIManager : MonoBehaviour
{
    public Player player;
    public GameObject Cardprefab;
    public Transform handpanel;
    public CardDeckManager CardDeckManager;
    public GameObject drawButton;

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
        // Clear existing buttonsb
        foreach (Transform child in handpanel)
        {
            Destroy(child.gameObject);
        }
        // Create new card sprites for each card in the player's hand
        foreach(var cardata in CardDeckManager.playerHand)
        {
          //  var card = CardDeckManager.playerHand[i];
            var generatecards = Instantiate(Cardprefab, handpanel);


            // assign the card data name to text button
            var cardUI = generatecards.GetComponent<CardUI>();
            cardUI.Loadtext(cardata);
        }
    }
    
    /// show the button at the start of the players turn
    public void Showbutton()
    {
        if(drawButton != null)
           drawButton.SetActive(true);  
    }
    /// hide the button when player draws card
    public void Hidebutton()
    {
        if (drawButton != null)
            drawButton.SetActive(false);
    }

}
