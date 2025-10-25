using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string EnemyName;
    public Sprite EnemySprite;

    [Header("Stats")]
    public int MaxHealth = 100;
    public int MaxMana = 10;
    public int StartMana = 5;

    [Header("Deck/AI")]
    // Folder inside Resources that contains CardData assets for this enemy (e.g., "enemy1", "enemyBandit", etc.)
    public string CardsResourceFolder = "enemy1";
    public int MaxDeckSize = 200;

    // How many cards of a given mana cost to include when building the deck
    public List<ManaLimit> ManaLimits = new List<ManaLimit>
    {
        new ManaLimit(1, 22),
        new ManaLimit(2, 40),
        new ManaLimit(5, 40),
        new ManaLimit(10, 6)
    };

    // Preferred ability order influences how the deck is composed (outer loop),
    // and can be used later to bias AI decisions if desired.
    public AbilityType[] PreferredAbilities = new AbilityType[]
    {
        AbilityType.Block,
        AbilityType.Damage
    };

    [System.Serializable]
    public struct ManaLimit
    {
        public int Mana;
        public int Limit;
        public ManaLimit(int mana, int limit)
        {
            Mana = mana;
            Limit = limit;
        }
    }
}
