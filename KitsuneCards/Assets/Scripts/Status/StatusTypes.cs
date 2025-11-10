using UnityEngine; 
public enum StatusKind
{
    Dot,
    Weaken,
    Stun,
    Block,
    DotAmp,
    AllX3,
    BlockX,
    Reflect,
    Ephemeral
}

public struct StatusView
{
    public StatusKind Kind;
    public Sprite Icon;      // Optional; can be null if your bar resolves by Kind
    public string ValueText; // e.g., "2", "50%", "x3"
    public int Turns;        // 0 if none
    public bool Emphasize;   // Optional style hint
}

/*// DoT applied
statusHUD.Show(new StatusView {
    Kind = StatusKind.Dot,
    Icon = dotSprite,        // or null if hub maps by Kind
    ValueText = doTDmg.ToString(),
    Turns = doTTurns
});

// Weaken (x0.8 for 3 turns)
statusHUD.Show(new StatusView {
    Kind = StatusKind.Weaken,
    Icon = weakenSprite,
    ValueText = "x0.8",
    Turns = 3
});

// Stun (turns only)
statusHUD.Show(new StatusView {
    Kind = StatusKind.Stun,
    Icon = stunSprite,
    ValueText = null,
    Turns = 1
});

// Expire DoT
statusHUD.Hide(StatusKind.Dot);

// New battle
statusHUD.Clear();*/