using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardPoolEntry
{
    public CardData cardData;

    [Min(0)]
    public float weight = 1f;
}

[CreateAssetMenu(
    fileName = "NewCardPool",
    menuName = "Game/Card/Pool"
)]
public class CardPool : ScriptableObject
{
    public List<CardPoolEntry> cards;
}