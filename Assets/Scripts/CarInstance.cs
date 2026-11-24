using System;

[System.Flags]
public enum CardOption
{
    None = 0,

    Operator = 1 << 0,
    Number = 1 << 1,

    Physics = 1 << 2,
    Stat = 1 << 3,

    SpecialEffect = 1 << 4
}

public enum  CardType
{
    Number,
    Physics,
    Stat,
}

[Serializable]
public class CardInstance
{
    // 원본 카드 데이터
    public CardData cardData;

    // 카드 종류
    public CardType cardType;

    // 실제로 기록된 옵션
    public CardOption recordedOptions;

    // 숫자 카드 옵션
    public CardOperator cardOperator;
    public float numberValue;

    // 물리 카드 옵션
    public string physicsType;
  

    // 능력치 카드 옵션
    public string statType;
 

    // 특수 카드 옵션
    public string specialEffect;

    public CardInstance parentCard;
    public CardInstance childCard;

    public CardInstance(CardData data)
    {
        cardData = data;
        recordedOptions = CardOption.None;
    }

    // 해당 옵션이 기록되어 있는지 검사
    public bool HasOption(CardOption option)
    {
        return (recordedOptions & option) != 0;
    }

    // 옵션 기록
    public void AddOption(CardOption option)
    {
        recordedOptions |= option;
    }
}