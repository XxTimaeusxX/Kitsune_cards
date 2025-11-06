using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusIconItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;       // status icon image;
    [SerializeField] private TMP_Text turns;   // turn amount
    [SerializeField] private TMP_Text value;   // primary value

    // Configure full (uses prefab's sprite if you set it here)
    public void Configure(Sprite sprite, string valueText, int? turnsCount)
    {
        icon.sprite = sprite;
        value.text = valueText;
        turns.text = turnsCount.HasValue ? turnsCount.Value.ToString() : string.Empty;
    }

    // Configure text only (do not change sprite)
    public void ConfigureText(string valueText, int? turnsCount)
    {
        value.text = valueText;
        turns.text = turnsCount.HasValue ? turnsCount.Value.ToString() : string.Empty;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}