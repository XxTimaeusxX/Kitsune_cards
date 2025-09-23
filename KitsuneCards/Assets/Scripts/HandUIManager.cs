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
        for(int i = 0; i < CardDeckManager.playerHand.Count; i++)
        {
            var card = CardDeckManager.playerHand[i];
            GameObject generatecards = Instantiate(Cardprefab, handpanel);


            // assign the card data name to text button
            var cardUI = generatecards.GetComponent<CardUI>();
            if(cardUI != null)
            {
                cardUI.Loadtext(card);
            }



        }
    }

}
