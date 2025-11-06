using System.Collections.Generic;
using UnityEngine;

public class StatusIconBar : MonoBehaviour
{
    [System.Serializable]
    private struct StatusPrefabEntry { public StatusKind Kind; public StatusIconItem Prefab; }

    [SerializeField] private StatusPrefabEntry[] prefabMap; // assign distinct prefab per kind

    private readonly Dictionary<StatusKind, StatusIconItem> _items = new Dictionary<StatusKind, StatusIconItem>();
    private readonly Dictionary<StatusKind, StatusIconItem> _prefabs = new Dictionary<StatusKind, StatusIconItem>();

    private void Awake()
    {
        _prefabs.Clear();
        for (int i = 0; i < prefabMap.Length; i++)
            _prefabs[prefabMap[i].Kind] = prefabMap[i].Prefab;

        // Start clean so we only have runtime-spawned items
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        _items.Clear();
    }

    // Show/update a status by kind (text/turns only; prefab holds sprite)
    public void Show(StatusKind kind, string valueText, int? turns = null)
    {
        var item = Ensure(kind);
        item.ConfigureText(valueText, turns);
        item.SetActive(true);
    }

    // Hide a status by kind.
    public void Hide(StatusKind kind)
    {
        if (_items.TryGetValue(kind, out var item))
            item.SetActive(false);
    }

    // Clear all (destroy children).
    public void Clear()
    {
        foreach (var it in _items.Values)
            Destroy(it.gameObject);
        _items.Clear();
    }

    // ------ Convenience methods (no cross-hides) ------
    public void UpdateBlock(int armor)
    {
        if (armor > 0) Show(StatusKind.Block, armor.ToString(), null);
        else Hide(StatusKind.Block);
    }

    public void UpdateBlockX(float multiplier, int turns)
    {
        if (turns > 0) Show(StatusKind.BlockX, "x" + multiplier.ToString("0.##"), turns);
        else Hide(StatusKind.BlockX);
    }

    public void UpdateAllX(float multiplier, int turns)
    {
        if (turns > 0) Show(StatusKind.AllX3, "x" + multiplier.ToString("0.##"), turns);
        else Hide(StatusKind.AllX3);
    }

    public void UpdateWeaken(float attackDamage, int turns)
    {
        if (turns > 0) Show(StatusKind.Weaken, "-" + Mathf.RoundToInt(attackDamage * 100f) + "%", turns);
        else Hide(StatusKind.Weaken);
    }

    public void UpdateReflect(float percent, int turns)
    {
        if (turns > 0) Show(StatusKind.Reflect, Mathf.RoundToInt(percent * 100f) + "%", turns);
        else Hide(StatusKind.Reflect);
    }

    public void UpdateDot(int tickDamage, int turns)
    {
        if (turns > 0) Show(StatusKind.Dot, tickDamage.ToString(), turns);
        else Hide(StatusKind.Dot);
    }

    public void UpdateDotAmp(int bonusDamage, int turns)
    {
        if (turns > 0) Show(StatusKind.DotAmp, "+" + bonusDamage.ToString(), turns);
        else Hide(StatusKind.DotAmp);
    }
    public void UpdateStun(int turns)
    {
        if (turns > 0) Show(StatusKind.Stun, "", turns);
        else Hide(StatusKind.Stun);
    }
    // --- Internals ---
    private StatusIconItem Ensure(StatusKind kind)
    {
        if (_items.TryGetValue(kind, out var item)) return item;

        var prefab = _prefabs[kind];                  // must be assigned per kind
        var created = Instantiate(prefab);            // instantiate that prefab
        created.transform.SetParent(transform, false);
        created.name = $"Status_{kind}";
        created.gameObject.SetActive(false);
        _items[kind] = created;
        return created;
    }
}