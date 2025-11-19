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
    public GameObject endTurnButton;
    public CardAbilityManager abilityManager;

    [Header("Card prefab variants")]
    [SerializeField] private CardUI firePrefab;
    [SerializeField] private CardUI waterPrefab;
    [SerializeField] private CardUI earthPrefab;
    [SerializeField] private CardUI airPrefab;
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
        // Create new card UI for each card in the player's hand
        for (int i = 0; i < CardDeckManager.playerHand.Count; i++)
        {
            var cardData = CardDeckManager.playerHand[i];

            // Choose the correct prefab by element, fall back to Cardprefab
            var chosen = GetPrefabFor(cardData.elementType);
            GameObject instanceGO;
            CardUI cardUI = null;

            if (chosen != null)
            {
                var instance = Instantiate(chosen, handpanel, false);
                instance.LoadCard(cardData);
                instanceGO = instance.gameObject;
                cardUI = instance;
            }
            else
            {
                if (Cardprefab == null)
                {
                    Debug.LogError("HandUIManager: No prefab found (variants missing and Cardprefab is null).");
                    continue;
                }
                instanceGO = Instantiate(Cardprefab, handpanel);
               cardUI = instanceGO.GetComponent<CardUI>();
                if (cardUI != null)
                    cardUI.LoadCard(cardData);
                else
                    Debug.LogWarning("HandUIManager: Instantiated fallback prefab has no CardUI component.");
            }

            // Fan effect
            float angle = (cardCount > 1) ? startAngle + (spread / (cardCount - 1)) * i : 0f;
            var rt = instanceGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(
                -Mathf.Sin(Mathf.Deg2Rad * angle) * 1800f,
                -Mathf.Abs(Mathf.Deg2Rad * angle) * 150f
            );
            rt.rotation = Quaternion.Euler(0, 0, angle);
            cardUI.HandCardRotation(angle);
        }
    }
    
    /// show the button at the start of the players turn
    public void Showbutton()
    {
        if(endTurnButton != null)
           endTurnButton.SetActive(true);
    }
    /// hide the button when player draws card

    public void HideEndTurnButton()
    {
        if (endTurnButton != null)
            endTurnButton.SetActive(false);
    }

    public void OnEndTurnButtonPressed()
    {
        
        player.PendTurn();
      //  HideEndTurnButton();
    }
    public void SetHandCardsInteractable(bool interactable)
    {
        foreach (Transform child in handpanel)
        {
            var cardUI = child.GetComponent<CardUI>();
            if (cardUI != null)
                cardUI.SetInteractable(interactable);
        }
    }
    private CardUI GetPrefabFor(CardData.ElementType element)
    {
        switch (element)
        {
            case CardData.ElementType.Fire: return firePrefab;
            case CardData.ElementType.Water: return waterPrefab;
            case CardData.ElementType.Earth: return earthPrefab;
            case CardData.ElementType.Air: return airPrefab;
            default: return null;
        }
    }
}
