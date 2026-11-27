using System;

[Serializable]
public sealed class PhysicsCardInstance : CardInstance
{
    public PhysicsType physicsType;

    public override CardType cardType => CardType.Physics;
    public override CardOption ProvidedOptions => CardOption.Physics;

    public PhysicsCardInstance(PhysicsCardData data) : base(data)
    {
        physicsType = data.physicsType;
    }
}
