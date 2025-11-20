using UnityEngine;

public enum GameMode
{
    Regular,
    BuffOnly,
    BuffAndDebuff,
    BossOnly,
    RogueLike
}

public class GameModeConfig : MonoBehaviour
{
    public static GameMode CurrentMode { get; private set; } = GameMode.Regular;

    private static GameModeConfig _instance;
    public static bool UpgradesEnabled { get; private set; } = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void SetMode(GameMode mode)
    {
        EnsureInstance();
        CurrentMode = mode;
    }
    public static void SetUpgradesEnabled(bool enabled)
    {
        EnsureInstance();
        UpgradesEnabled = enabled;
    }
    private static void EnsureInstance()
    {
        if (_instance == null)
        {
            var go = new GameObject("GameModeConfig");
            _instance = go.AddComponent<GameModeConfig>();
        }
    }
}