using System;

// 기존 Unity 씬의 CardCombinationManager 규칙 직렬화 호환을 위해 유지한다.
// CardInstance에 옵션을 저장하지 않고, 파생 클래스가 제공하는 옵션을 계산한다.
[Flags]
public enum CardOption
{
    None = 0,
    Operator = 1 << 0,
    Number = 1 << 1,
    Physics = 1 << 2,
    Stat = 1 << 3,
    SpecialEffect = 1 << 4,
    Target = 1 << 5
}

// 기존 0, 1, 2 값은 그대로 유지한다.
public enum CardType
{
    Number = 0,
    Physics = 1,
    Stat = 2,
    Special = 3,
    Target = 4
}

[Serializable]
public abstract class CardInstance
{
    // 생성에 사용된 ScriptableObject. 기존 CardData는 그대로 사용한다.
    public CardData cardData;

    // 1개의 부모와 1개의 자식을 갖는 선형 체인.
    [UnityEngine.SerializeReference] public CardInstance parentCard;
    [UnityEngine.SerializeReference] public CardInstance childCard;

    // 별도 저장 필드가 아닌 파생 타입으로부터 계산되는 속성.
    public abstract CardType cardType { get; }
    public abstract CardOption ProvidedOptions { get; }

    protected CardInstance(CardData data)
    {
        cardData = data;
    }

    // 기존 CardCombinationManager의 Inspector 규칙을 유지하기 위한 호환 API.
    // 플래그가 여러 개라면 모두 충족해야 한다.
    public bool HasOption(CardOption options)
    {
        return options != CardOption.None &&
               (ProvidedOptions & options) == options;
    }
}
