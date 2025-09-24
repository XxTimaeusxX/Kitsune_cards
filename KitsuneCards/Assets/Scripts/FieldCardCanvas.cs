using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldCardCanvas : MonoBehaviour, IDropHandler
{
    public Player player;
    public Transform playerfield;
    public CardDeckManager cardDeckManager;
    private HandUIManager handUIManager;
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop fired on FieldCardCanvas");
        var CardUI = eventData.pointerDrag.GetComponent<CardUI>();
        if (CardUI != null)
        {
            cardDeckManager.playerHand.Remove(CardUI.cardData);
            cardDeckManager.playerfield.Add(CardUI.cardData); // Optionally add to a field list if needed
            CardUI.transform.SetParent(playerfield,false);
            CardUI.transform.localPosition = Vector3.zero; // Center in the field
            CardUI.transform.localScale = Vector3.one; // Reset scale
            var canvasGroup = CardUI.GetComponent<CanvasGroup>();
            if(canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }
            // Hide the draw button after playing a card to the field
            if (cardDeckManager.handUIManager != null)
                cardDeckManager.handUIManager.Hidebutton();

            // after player play card, end turn
            player.OndiscardCard(CardUI.cardData);
            if (handUIManager != null)
            {
                handUIManager.UpdateHandUI(); // Update the hand UI after drawing cards
            }          
        }
    }

   
}
