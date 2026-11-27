using System;

[Serializable]
public sealed class SpecialCardInstance : CardInstance
{
    public SpecialType specialType;

    public override CardType cardType => CardType.Special;
    public override CardOption ProvidedOptions => CardOption.SpecialEffect;

    public SpecialCardInstance(SpecialCardData data) : base(data)
    {
        specialType = data.specialType;
    }
}
