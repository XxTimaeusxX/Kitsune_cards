using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldCardCanvas : MonoBehaviour, IDropHandler
{
    public Player player;
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop fired on FieldCardCanvas");
        var CardUI = eventData.pointerDrag.GetComponent<CardUI>();
        if (CardUI != null)
        {
             Debug.Log($"Card dropped");
            player.OndiscardCard(CardUI.cardData);
        }
    }

   
}
