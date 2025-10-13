using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldCardCanvas : MonoBehaviour, IDropHandler
{
    public Player player;
    public Transform playerfield;
    public CardDeckManager cardDeckManager;
    public CardAbilityManager abilityManager;
    private HandUIManager handUIManager;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip playCardClip;
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop fired on FieldCardCanvas");

        var CardUI = eventData.pointerDrag.GetComponent<CardUI>();
       
        if (CardUI != null)
        {
            if(TryPlayCard(CardUI.cardData))
            {
                cardDeckManager.playerHand.Remove(CardUI.cardData);
                cardDeckManager.playerfield.Add(CardUI.cardData); // Optionally add to a field list if needed
                CardUI.transform.SetParent(playerfield, false);
                CardUI.transform.localPosition = Vector3.zero; // Center in the field
                CardUI.transform.localScale = Vector3.one; // Reset scale
                var canvasGroup = CardUI.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true;
                }
                // Hide the draw button after playing a card to the field
                if (cardDeckManager.handUIManager != null)
                    cardDeckManager.handUIManager.Hidebutton();
                audioSource.PlayOneShot(playCardClip);
                PlayCard(CardUI);

               
                if (handUIManager != null)
                {
                    handUIManager.UpdateHandUI(); // Update the hand UI after drawing cards

                }
            }
            else
            {
                Debug.Log($"Not enough mana to play {CardUI.cardData.CardName}.");
                GameTurnMessager.instance.ShowMessage("Not enough mana!");
                // Optionally: Snap card back to hand or show UI feedback
            }
        }
    }
    // Call this when a card is dropped onto the field
    public void PlayCard(CardUI cardUI)
    {
        if (abilityManager != null && cardUI != null)
        {
            abilityManager.ExecuteCardAbility(
                cardUI.cardData,
                cardDeckManager.enemy, // reference to IDamageable target(enemy)
                cardDeckManager.enemy, // reference to IDebuffable target(enemy)
                cardDeckManager.enemy, // reference to enemy
                cardDeckManager.player, // reference to player
                cardDeckManager.player // reference to who gets the armor.
            );
           
           // cardDeckManager.playerfield.Remove(cardUI.cardData);
            Destroy(cardUI.gameObject);
            // Optionally: Remove card from hand, update field visuals, etc.
        }
        
    }
    public bool TryPlayCard(CardData card)
    {
        // Get the selected ability for this card
        ManaCostandEffect ability = default;
        switch (card.elementType)
        {
            case CardData.ElementType.Fire:
                ability = card.FireAbilities[card.selectedManaAndEffectIndex]; break;
            case CardData.ElementType.Water:
                ability = card.WaterAbilities[card.selectedManaAndEffectIndex]; break;
            case CardData.ElementType.Earth:
                ability = card.EarthAbilities[card.selectedManaAndEffectIndex]; break;
            case CardData.ElementType.Air:
                ability = card.AirAbilities[card.selectedManaAndEffectIndex]; break;
        }
        int manaCost = ability.ManaCost;

        if (player.HasEnoughMana(manaCost))
        {
            player.SpendMana(manaCost);
            return true;
        }
        else
        {
            return false;
        }
    }
}
