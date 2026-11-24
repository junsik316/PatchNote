using UnityEngine;

public enum SpecialType
{
    DoubleJump,
    InvincibilityTime
}

[CreateAssetMenu(fileName = "NewSpecialCard", menuName = "Game/Card/Special")]
public class SpecialCardData : CardData
{
    public SpecialType specialType;
}