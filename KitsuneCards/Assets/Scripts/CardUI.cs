using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store original parent and position
        originalParent = transform.parent;
        originalPosition = transform.position;
        // Make the card raycast-ignoring so it doesn't block drop detection
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f; // Optional: make card semi-transparent while dragging
        }
        // Optionally bring to front
        transform.SetParent(transform.root, true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore raycast blocking
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
        // snap back to hand panel if player doesnt place card on the player field
        if(transform.parent == originalParent)
        {
            transform.position = originalPosition;
        }
        
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        // Move the card with the pointer
        transform.position = eventData.position;
    }

    public void Loadtext(CardData cardData)
    {
        // Initialize the card UI with card data (e.g., set text, images)
        this.cardData = cardData;
        GetComponentInChildren<TMPro.TMP_Text>().text = cardData.CardName;
    }

}
