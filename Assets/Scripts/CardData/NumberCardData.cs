using System;
using UnityEngine;

public enum CardNumber
{
    Integer,
    Decimal,
    Wild
}

[Flags]
public enum CardOperator
{
    None = 0,
    Multiply = 1 << 0,
    Divide = 1 << 1,
    Add = 1 << 2,
    Subtract = 1 << 3,
    Wild = 1 << 4
}

[CreateAssetMenu(fileName = "NewNumberCard", menuName = "Game/Card/Number")]
public class NumberCardData : CardData
{
    public CardNumber cardNumber;

    public CardOperator cardOperator;

    [Header("Wild 수치 범위")]
    public float minValue = 1.0f;
    public float maxValue = 2.0f;
}