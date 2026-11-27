using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("방 종류별 카드 풀")]
    public CardPool commonPool;
    public CardPool hardPool;
    public CardPool elitePool;
    public CardPool rewardPool;
    public CardPool bossPool;

    private readonly List<CardInstance> selectedCards = new List<CardInstance>();

    public void ShowCardSelection(MapTree.Node node)
    {
        CardPool pool = GetCardPool(node);
        if (pool == null)
        {
            Debug.LogError("해당 방의 카드 풀이 설정되지 않았습니다.");
            return;
        }

        GenerateCards(pool);
        GameManager.Instance.uiManager.ShowCardSelection(selectedCards);
    }

    private CardPool GetCardPool(MapTree.Node node)
    {
        switch (node.type)
        {
            case MapTree.NodeType.common: return commonPool;
            case MapTree.NodeType.hard: return hardPool;
            case MapTree.NodeType.elite: return elitePool;
            case MapTree.NodeType.reward: return rewardPool;
            case MapTree.NodeType.boss: return bossPool;
            default: return null;
        }
    }

    private void GenerateCards(CardPool pool)
    {
        List<CardPoolEntry> availableCards = new List<CardPoolEntry>(pool.cards);
        selectedCards.Clear();

        for (int i = 0; i < 3; i++)
        {
            CardPoolEntry selectedEntry = SelectCard(availableCards);
            if (selectedEntry == null)
                break;

            // 인스턴스 구성이 지원되는 데이터만 선택 목록에 추가한다.
            CardInstance instance = GenerateCardInstance(selectedEntry.cardData);
            if (instance != null)
                selectedCards.Add(instance);

            availableCards.RemoveAll(
                entry => entry.cardData == selectedEntry.cardData
            );
        }
    }

    // 원래 가중치 추첨 로직을 유지.
    private CardPoolEntry SelectCard(List<CardPoolEntry> cards)
    {
        float totalWeight = 0f;
        foreach (CardPoolEntry entry in cards)
        {
            if (entry.cardData == null || entry.weight <= 0f)
                continue;
            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        foreach (CardPoolEntry entry in cards)
        {
            if (entry.cardData == null || entry.weight <= 0f)
                continue;
            currentWeight += entry.weight;
            if (randomValue < currentWeight)
                return entry;
        }
        return null;
    }

    // CardData는 수정하지 않고 생성되는 CardInstance만 구체 타입으로 변경.
    private CardInstance GenerateCardInstance(CardData data)
    {
        if (data is NumberCardData numberCard)
        {
            return new NumberCardInstance(
                numberCard,
                GenerateOperator(numberCard),
                GenerateNumber(numberCard)
            );
        }
        if (data is PhysicsCardData physicsCard)
            return new PhysicsCardInstance(physicsCard);

        if (data is StatCardData statCard)
            return new StatCardInstance(statCard);

        // 특수 카드 풀을 아직 사용하지 않아도 타입은 대응하도록 준비.
        if (data is SpecialCardData specialCard)
            return new SpecialCardInstance(specialCard);

        // TargetCardData는 아직 없으므로 미지원 카드임을 명시한다.
        Debug.LogError($"지원하지 않는 카드 데이터: {data?.GetType().Name ?? "null"}");
        return null;
    }

    private CardOperator GenerateOperator(NumberCardData card)
    {
        if ((card.cardOperator & CardOperator.Wild) == 0)
            return card.cardOperator;

        List<CardOperator> operators = new List<CardOperator>();
        CardOperator[] candidates =
        {
            CardOperator.Multiply,
            CardOperator.Divide,
            CardOperator.Add,
            CardOperator.Subtract
        };

        foreach (CardOperator op in candidates)
        {
            if ((card.cardOperator & op) != 0)
                operators.Add(op);
        }

        if (operators.Count == 0)
        {
            Debug.LogError("Wild 카드에 허용된 연산자가 없습니다.");
            return CardOperator.None;
        }
        return operators[Random.Range(0, operators.Count)];
    }

    private float GenerateNumber(NumberCardData card)
    {
        float min = Mathf.Min(card.minValue, card.maxValue);
        float max = Mathf.Max(card.minValue, card.maxValue);

        switch (card.cardNumber)
        {
            case CardNumber.Integer:
                {
                    int minInt = Mathf.CeilToInt(min);
                    int maxInt = Mathf.FloorToInt(max);
                    if (minInt > maxInt)
                    {
                        Debug.LogError("정수 범위가 올바르지 않습니다.");
                        return min;
                    }
                    return Random.Range(minInt, maxInt + 1);
                }
            case CardNumber.Decimal:
            case CardNumber.Wild:
                {
                    int minDecimal = Mathf.CeilToInt(min * 100f);
                    int maxDecimal = Mathf.FloorToInt(max * 100f);
                    if (minDecimal > maxDecimal)
                    {
                        Debug.LogError("소수 범위가 올바르지 않습니다.");
                        return min;
                    }
                    return Random.Range(minDecimal, maxDecimal + 1) / 100f;
                }
            default:
                return 0f;
        }
    }

    public void SendCard(int index)
    {
        if (index < 0 || index >= selectedCards.Count)
            return;

        CardInstance instance = selectedCards[index];
        GameManager.Instance.uiManager.AddCard(instance);

        // 실제 카드 효과의 계산 책임은 추후 분리할 예정.
        Debug.Log("선택한 카드: " + instance.cardData.cardName);
        if (instance is NumberCardInstance number)
            Debug.Log($"연산자: {number.cardOperator} / 숫자: {number.numberValue}");

        GameManager.Instance.uiManager.CloseCardSelection();
    }
}
