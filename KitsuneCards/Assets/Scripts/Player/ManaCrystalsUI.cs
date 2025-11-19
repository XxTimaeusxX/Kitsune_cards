using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaCrystalsUI : MonoBehaviour
{
    [Header("Prefab / Container")]
    public Transform crystalsParent;   // Parent with HorizontalLayoutGroup
    public GameObject crystalPrefab;   // Prefab containing a single Image (Type = Simple)
    public int maxCrystalCap = 10;     // Max balls to instantiate

    private readonly List<GameObject> crystals = new List<GameObject>();

    void Start()
    {
        // Optional: pre-create the default cap on start
        if (crystalsParent != null && crystalPrefab != null)
            Initialize(maxCrystalCap);
    }

    // Create exactly 'cap' crystal GameObjects under crystalsParent (clears existing)
    public void Initialize(int cap)
    {
        cap = Mathf.Clamp(cap, 0, maxCrystalCap);
        if (crystalsParent == null || crystalPrefab == null) return;

        // Clear existing children
        while (crystalsParent.childCount > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(crystalsParent.GetChild(0).gameObject);
#else
            Destroy(crystalsParent.GetChild(0).gameObject);
#endif
        }
        crystals.Clear();

        // Instantiate simple image prefab instances
        for (int i = 0; i < cap; i++)
        {
            var go = Instantiate(crystalPrefab, crystalsParent);
            go.SetActive(true);
            // Ensure the prefab's image is Simple type (no filled logic)
            var img = go.GetComponentInChildren<Image>();
            if (img != null) img.type = Image.Type.Simple;
            crystals.Add(go);
        }
    }

    // Enable the first 'count' balls and disable the rest.
    // Example: ShowBalls(3) -> first 3 active (represent 3 mana)
    public void ShowBalls(int count)
    {
        if (crystals.Count == 0) return;
        count = Mathf.Clamp(count, 0, crystals.Count);
        for (int i = 0; i < crystals.Count; i++)
            crystals[i].SetActive(i < count);
    }
}