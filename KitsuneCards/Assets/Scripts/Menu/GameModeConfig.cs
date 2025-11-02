using UnityEngine;

public enum GameMode
{
    Regular,
    BuffOnly,
    BuffAndDebuff,
    BossOnly
}

public class GameModeConfig : MonoBehaviour
{
    public static GameMode CurrentMode { get; private set; } = GameMode.Regular;

    private static GameModeConfig _instance;

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

    private static void EnsureInstance()
    {
        if (_instance == null)
        {
            var go = new GameObject("GameModeConfig");
            _instance = go.AddComponent<GameModeConfig>();
        }
    }
}