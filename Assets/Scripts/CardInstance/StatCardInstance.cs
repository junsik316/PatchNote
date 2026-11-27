using System;

[Serializable]
public sealed class StatCardInstance : CardInstance
{
    public StatType statType;

    public override CardType cardType => CardType.Stat;
    public override CardOption ProvidedOptions => CardOption.Stat;

    public StatCardInstance(StatCardData data) : base(data)
    {
        statType = data.statType;
    }
}
