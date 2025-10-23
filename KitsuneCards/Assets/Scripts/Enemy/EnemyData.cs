using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Enemy")]
public class EnemyData : ScriptableObject
{
    public string EnemyName;
    public int MaxHealth;
    public int AttackPower;
    public Sprite EnemySprite;
}
