using System.Collections.Generic;
using UnityEngine;

public class ManaCrystalsUI : MonoBehaviour
{
    [Header("Prefab + parent")]
    public GameObject crystalPrefab;       // assign single crystal prefab in inspector
    public Transform crystalsParent;       // parent transform for instantiated crystals

    [Header("Limits")]
    public int maxCrystalCap = 10;

    private readonly List<GameObject> _crystals = new List<GameObject>();

    // Create exactly 'cap' crystal GameObjects under crystalsParent (clears existing).
    public void Initialize(int cap)
    {
        try
        {
            if (crystalsParent == null || crystalPrefab == null)
            {
                Debug.LogWarning($"ManaCrystalsUI.Initialize: Missing reference(s) on '{name}' (parent:{crystalsParent}, prefab:{crystalPrefab})");
                return;
            }

            cap = Mathf.Clamp(cap, 0, maxCrystalCap);

            // If already correct count, do nothing
            if (_crystals.Count == cap)
            {
                // Ensure active states are reset (caller will call ShowBalls afterwards)
                for (int i = 0; i < _crystals.Count; i++) _crystals[i].SetActive(true);
                return;
            }

            // Clear existing
            for (int i = _crystals.Count - 1; i >= 0; i--)
            {
                var go = _crystals[i];
                if (go != null) DestroyImmediate(go);
            }
            _crystals.Clear();

            // Instantiate new set
            for (int i = 0; i < cap; i++)
            {
                var go = Instantiate(crystalPrefab, crystalsParent);
                go.name = $"ManaCrystal_{i}";
                _crystals.Add(go);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ManaCrystalsUI.Initialize threw: {ex}");
        }
    }

    // Enable the first 'count' balls and disable the rest.
    public void ShowBalls(int count)
    {
        try
        {
            if (_crystals.Count == 0)
            {
                // nothing initialized — avoid heavy work on menu load
                return;
            }

            count = Mathf.Clamp(count, 0, _crystals.Count);
            for (int i = 0; i < _crystals.Count; i++)
            {
                _crystals[i].SetActive(i < count);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ManaCrystalsUI.ShowBalls threw: {ex}");
        }
    }
}