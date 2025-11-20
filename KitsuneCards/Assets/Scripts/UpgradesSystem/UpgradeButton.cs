using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("UI refs")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image iconImage;
    public Button button;

    private UpgradeDef _def;
    private Action<UpgradeDef> _onClick;

    public void Setup(in UpgradeDef def, Action<UpgradeDef> onClick)
    {
        _def = def;
        _onClick = onClick;

        if (titleText != null) titleText.text = string.IsNullOrEmpty(def.Title) ? "Upgrade" : def.Title;
        if (descriptionText != null) descriptionText.text = def.Description ?? "";
        if (iconImage != null) iconImage.sprite = def.Icon;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonPressed);
        }
    }

    private void OnButtonPressed()
    {
        _onClick?.Invoke(_def);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveAllListeners();
    }
}