
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
    public string CardName;
    public int id;
    public ElementType elementType;
}
