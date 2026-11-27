using System;

// 기획 중인 적용 주체 카드의 런타임 데이터만 우선 정의한다.
// TargetCardData는 아직 프로젝트에 없으므로 CardManager에서 생성하지 않는다.
public enum EffectTarget
{
    Player,
    Enemy
}

[Serializable]
public sealed class TargetCardInstance : CardInstance
{
    public EffectTarget target;

    public override CardType cardType => CardType.Target;
    public override CardOption ProvidedOptions => CardOption.Target;

    public TargetCardInstance(CardData data, EffectTarget target) : base(data)
    {
        this.target = target;
    }
}
