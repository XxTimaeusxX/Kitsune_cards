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

        // Dropdown for ManaAndEffect
        string[] options = new string[cardData.ManaAndEffect.Count];
        for (int i = 0; i < options.Length; i++)
        {
            var pair = cardData.ManaAndEffect[i];
            options[i] = $"{pair.ManaCost} AP - {pair.EffectDescription}";
        }

        cardData.selectedManaAndEffectIndex = EditorGUILayout.Popup("Mana & Effect", cardData.selectedManaAndEffectIndex, options);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cardData);
        }
    }
}
