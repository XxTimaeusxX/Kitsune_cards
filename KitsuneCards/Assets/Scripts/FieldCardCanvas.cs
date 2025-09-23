using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldCardCanvas : MonoBehaviour, IDropHandler
{
    public Player player;
    public Transform playerfield;
    public CardDeckManager cardDeckManager;
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
            if(cardDeckManager != null)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

   
}
