using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public CardData cardData;
    public Image imageHolder; // Assign in Inspector
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;

    private Vector3 originalScale;
    private RectTransform rectTransform;

    private Quaternion _baseRotation = Quaternion.identity;
    private Vector3 cardExpandSize = new Vector3(7f, 9f, 1f);
    private Coroutine _scaleCoroutine;
    // Store original sibling index so we can restore ordering
    private int _originalSiblingIndex = -1;

    public TMP_Text cardNameText; // Assign in Inspector
    public TMP_Text abilityText; // Assign in Inspector
    public TMP_Text manacostText; // Assign in Inspector

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip selectCardClip;

    [SerializeField] private GameObject bossOverlay;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        audioSource.PlayOneShot(selectCardClip);

        // Store original parent and position
        originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();
        originalPosition = transform.position;
        transform.SetAsLastSibling();

        // Make the card raycast-ignoring so it doesn't block drop detection
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
           // canvasGroup.alpha = 0.7f; // Optional: make card semi-transparent while dragging
           
        }
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(Cardexpand(originalScale,  cardExpandSize, .08f));
         rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f); // have card face straight when dragging
    }
    public IEnumerator Cardexpand(Vector3 originalSize, Vector3 expandSize, float Duration)
    {
        float Expandtimer= 0f;
        while(Expandtimer < Duration)
        {
            rectTransform.localScale = Vector3.Lerp(originalSize, expandSize, Expandtimer / Duration);
            Expandtimer += Time.deltaTime;
            yield return null;
        }
        // ensure final value
        rectTransform.localScale = expandSize;

        // clear reference so callers know it's done
        _scaleCoroutine = null;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        // stop any expand coroutine so it doesn't continue writing scale
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }

        rectTransform.localScale = originalScale; // Restore original size
        rectTransform.rotation = _baseRotation; // Restore original rotation
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
        // Restore original sibling index (clamped to current child count)
        if (originalParent != null && _originalSiblingIndex >= 0)
        {
            int maxIndex = Mathf.Max(0, originalParent.childCount - 1);
            int restoreIndex = Mathf.Clamp(_originalSiblingIndex, 0, maxIndex);
            transform.SetSiblingIndex(restoreIndex);
            _originalSiblingIndex = -1;
        }

    }

   
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
       
        // Always use ScreenToWorldPoint with Canvas camera and plane distance
        var canvas = GetComponentInParent<Canvas>();
        float planeDistance = canvas.planeDistance;
        Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, planeDistance);
        transform.position = canvas.worldCamera.ScreenToWorldPoint(screenPos);
        
    }
        

    public void LoadCard(CardData cardData)
    {
        // Initialize the card UI with card data (e.g., set text, images)
        this.cardData = cardData;
        cardNameText.text = cardData.CardName;
        imageHolder.sprite = cardData.CharacterImage;

        // Get the selected ability for this card
        ManaCostandEffect selectedAbility = new ManaCostandEffect();
        bool hasAbility = false;
        switch (cardData.elementType)
        {
            case CardData.ElementType.Fire:
                selectedAbility = cardData.FireAbilities[cardData.selectedManaAndEffectIndex]; hasAbility = true;
                break;
            case CardData.ElementType.Water:
                selectedAbility = cardData.WaterAbilities[cardData.selectedManaAndEffectIndex]; hasAbility = true;
                break;
            case CardData.ElementType.Earth:
                selectedAbility = cardData.EarthAbilities[cardData.selectedManaAndEffectIndex]; hasAbility = true;
                break;
            case CardData.ElementType.Air:
                selectedAbility = cardData.AirAbilities[cardData.selectedManaAndEffectIndex]; hasAbility = true;
                break;
        }
        if (hasAbility)
        {
            manacostText.text = selectedAbility.ManaCost.ToString();
            abilityText.text = selectedAbility.EffectDescription;
        }
        else
        {
            manacostText.text = "-";
            abilityText.text = "No ability";
        }
    }

    // In CardUI.cs
    public void SetInteractable(bool interactable)
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = interactable;
            cg.blocksRaycasts = interactable;
            cg.alpha = interactable ? 1f : 0.5f; // Optional: fade out when not interactable
        }
    }

    public void SetBossCardVisual(bool isBoss)
    {
        if (bossOverlay != null)
            bossOverlay.SetActive(isBoss);
    }
    public void HandCardRotation(float Zangle)
    {
        _baseRotation = Quaternion.Euler(0f, 0f, Zangle);
        rectTransform.rotation = _baseRotation;
    }
}
