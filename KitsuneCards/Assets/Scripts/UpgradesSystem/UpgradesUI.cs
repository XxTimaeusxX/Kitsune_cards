using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesUI : MonoBehaviour
{
    [Header("UI")]
    public Transform panel;                     // empty transform with LayoutGroup
    public GameObject upgradeButtonPrefab;      // prefab with UpgradeButton component

    [Header("Content")]
    public UpgradeData upgradeData;             // assign UpgradeData component in scene

    [Header("Settings")]
    public int choiceCount = 4;

    private readonly List<GameObject> _spawned = new List<GameObject>();
    private System.Random _rng = new System.Random();

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    // Show up to `choiceCount` random upgrades; caller receives the selected UpgradeDef via callback.
    // The callback signature is Action<UpgradeDef>.
    public void ShowRandomChoices(IBlockable blockTarget, IDebuffable debuffTarget, IBuffable buffTarget, IDamageable opponent, Action<UpgradeDef> onChosen)
    {
        if (panel == null || upgradeButtonPrefab == null || upgradeData == null)
        {
            Debug.LogError("UpgradesUI: missing references (panel / prefab / upgradeData).");
            onChosen?.Invoke(default);
            return;
        }

        // Ensure pool exists: call your seeder if list is empty (keeps existing UpgradeList behavior)
        if (upgradeData.Upgrades == null || upgradeData.Upgrades.Count == 0)
        {
            try
            {
                upgradeData.UpgradeList();
            }
            catch (Exception)
            {
                // fallback: do nothing if seeder requires valid targets
            }
        }

        var pool = upgradeData.Upgrades ?? new List<UpgradeDef>();
        if (pool.Count == 0)
        {
            Debug.LogWarning("UpgradesUI: no upgrades available.");
            onChosen?.Invoke(default);
            return;
        }

        ClearSpawned();

        int take = Math.Min(choiceCount, pool.Count);
        var indices = PickRandomIndices(pool.Count, take);

        foreach (int idx in indices)
        {
            var def = pool[idx];
            var go = Instantiate(upgradeButtonPrefab, panel, false);
            var item = go.GetComponent<UpgradeButton>();
            if (item != null)
            {
                item.Setup(def, chosen =>
                {
                    // UI no longer applies the upgrade — forward selection to caller
                    ClearSpawned();
                    onChosen?.Invoke(chosen);
                });
            }
            _spawned.Add(go);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        ClearSpawned();
    }

    private void ClearSpawned()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
        }
        _spawned.Clear();
        gameObject.SetActive(false);
    }

    // Return `k` unique random indices from 0..n-1
    private List<int> PickRandomIndices(int n, int k)
    {
        var indices = new List<int>(n);
        for (int i = 0; i < n; i++) indices.Add(i);

        var result = new List<int>(k);
        for (int i = 0; i < k; i++)
        {
            int j = _rng.Next(i, n);
            int tmp = indices[i];
            indices[i] = indices[j];
            indices[j] = tmp;
            result.Add(indices[i]);
        }
        return result;
    }
}