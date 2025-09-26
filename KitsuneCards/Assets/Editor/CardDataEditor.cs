using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(CardData)) ]
public class CardDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CardData cardData = (CardData)target;

        cardData.CardName = EditorGUILayout.TextField("Card Name", cardData.CardName);
        cardData.CharacterImage = (Sprite)EditorGUILayout.ObjectField("Character Image", cardData.CharacterImage, typeof(Sprite), false);
        cardData.elementType = (CardData.ElementType)EditorGUILayout.EnumPopup("Element Type", cardData.elementType);
        
        // show abilities for selected element type
        List<ManaCostandEffect> abilities = null;
        switch (cardData.elementType)
        {
            case CardData.ElementType.Fire: abilities = cardData.FireAbilities; break;
            case CardData.ElementType.Water: abilities = cardData.WaterAbilities; break;
            case CardData.ElementType.Earth: abilities = cardData.EarthAbilities; break;
            case CardData.ElementType.Air: abilities = cardData.AirAbilities; break;
        }

        // Dropdown for abilities
        if (abilities != null && abilities.Count > 0)
        {
            string[] options = abilities.ConvertAll(a => $"{a.ManaCost} AP: {a.EffectDescription}").ToArray();
            cardData.selectedManaAndEffectIndex = EditorGUILayout.Popup("Ability", cardData.selectedManaAndEffectIndex, options);
        }
        else
        {
            EditorGUILayout.HelpBox("No abilities defined for this element.", MessageType.Info);
        }
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cardData);
        }
    }
}
