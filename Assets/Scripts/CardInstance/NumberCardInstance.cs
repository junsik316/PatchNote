using System;

[Serializable]
public sealed class NumberCardInstance : CardInstance
{
    public CardOperator cardOperator;
    public float numberValue;

    public override CardType cardType => CardType.Number;
    public override CardOption ProvidedOptions => CardOption.Operator | CardOption.Number;

    public NumberCardInstance(NumberCardData data, CardOperator op, float value)
        : base(data)
    {
        cardOperator = op;
        numberValue = value;
    }
}
