using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CardDeckManager deckManager;
    private bool HasDrawn= false;
    private bool hasDiscarded = false;
    public void PstartTurn()
    {
        HasDrawn = false;
        hasDiscarded = false;
        if (deckManager.handUIManager != null)
            deckManager.handUIManager.Showbutton();
        Debug.Log("Player now have draw and discard phases.");
        
    }
    public void PendTurn()
    {
        deckManager.OnPlayerEndTurn();
    }
    public void OndrawCard()
    {
        if (!HasDrawn)
        {
            deckManager.OnbuttonDrawpress();
            HasDrawn = true;
            if(deckManager.handUIManager != null)
               deckManager.handUIManager.Hidebutton(); // Hide the draw button after drawing cards
           
            Debug.Log("Player has drawn cards.");
        }
        else
        {
            Debug.Log("Player has already drawn cards this turn.");
        }
    }
    public void OndiscardCard(CardData card)
    {
        if (HasDrawn && !hasDiscarded)
        {
            deckManager.DiscardCard(card);
            hasDiscarded = true;
            Debug.Log("Player has discarded a card.");
            PendTurn();
        }
        else
        {
            Debug.Log("Player has already discarded a card this turn.");
        }
    }
}
