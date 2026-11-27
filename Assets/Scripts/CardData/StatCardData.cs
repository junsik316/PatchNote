using UnityEngine;

public enum StatType
{
    MaxHealth,
    AttackPower,
    Defense
}

[CreateAssetMenu(fileName = "NewStatCard", menuName = "Game/Card/Stat")]
public class StatCardData : CardData
{
    public StatType statType;

}