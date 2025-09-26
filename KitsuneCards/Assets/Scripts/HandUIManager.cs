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
    public CardAbilityManager abilityManager;

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

        int cardCount = CardDeckManager.playerHand.Count;
        float spread = 28f; // degrees, how tilted the cards are
        float startAngle = -spread / 2f;
       // float arcradius = 400f; // distance from center, how spread out the cards are

        // Create new card sprites for each card in the player's hand
        for (int i = 0; i< CardDeckManager.playerHand.Count; i++)
        {
            var cardata = CardDeckManager.playerHand[i];
            var generatecards = Instantiate(Cardprefab, handpanel);
            // assign the card data name to text button
            var cardUI = generatecards.GetComponent<CardUI>();
            cardUI.LoadCard(cardata);
           
            //fan effect
            float angle = (cardCount > 1) ? startAngle + (spread / (cardCount - 1)) * i : 0f;
            generatecards.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                -Mathf.Sin(Mathf.Deg2Rad * angle) * 2000f, // higher value more straight, lower value more curve
                -Mathf.Abs(Mathf.Deg2Rad * angle) * 350f // 
            );
            generatecards.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, angle);

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
