using UnityEngine;

public enum PhysicsType
{
    MoveSpeed,
    JumpForce,
    GravityScale,
    Friction,
    Bounciness
}



[CreateAssetMenu(fileName = "NewPhysicsCard", menuName = "Game/Card/Physics")]
public class PhysicsCardData : CardData
{
    public PhysicsType physicsType;

}