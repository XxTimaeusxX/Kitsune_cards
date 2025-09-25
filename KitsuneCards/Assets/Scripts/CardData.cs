
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Card")]
public class CardData : ScriptableObject
{
    public enum ElementType
    {
        Fire,
        Water,
        Earth,
        ice,
    }
  public List<int> damageValues = new List<int>(new int[] {10,20,30 }) ; // List to hold multiple damage values



    public string CardName;
    public int ManaCost; 
    public int ElementDamage;
    public Sprite CharacterImage;
    public ElementType elementType;
    
}
