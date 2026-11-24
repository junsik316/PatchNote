
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

    // 현재 선택 화면에 표시할 카드 인스턴스
    private List<CardInstance> selectedCards =
        new List<CardInstance>();

    // 플레이어 인벤토리
   

    // MapManager에서 현재 노드를 전달받음
    public void ShowCardSelection(MapTree.Node node)
    {
        CardPool pool = GetCardPool(node);

        if (pool == null)
        {
            Debug.LogError("해당 방의 카드 풀이 설정되지 않았습니다.");
            return;
        }

        GenerateCards(pool);

        // 생성된 카드 목록을 UIManager에 전달
        GameManager.Instance.uiManager.ShowCardSelection(selectedCards);
    }

    private CardPool GetCardPool(MapTree.Node node)
    {
        switch (node.type)
        {
            case MapTree.NodeType.common:
                return commonPool;
            case MapTree.NodeType.hard:
                return hardPool;
            case MapTree.NodeType.elite:
                return elitePool;
            case MapTree.NodeType.reward:
                return rewardPool;
            case MapTree.NodeType.boss:
                return bossPool;
            default:
                return null;
        }
    }

    // 카드 3장 추첨
    private void GenerateCards(CardPool pool)
    {
        List<CardPoolEntry> availableCards =
            new List<CardPoolEntry>(pool.cards);

        selectedCards.Clear();

        for (int i = 0; i < 3; i++)
        {
            CardPoolEntry selectedEntry =
                SelectCard(availableCards);

            if (selectedEntry == null)
                break;

            CardInstance instance =
                GenerateCardInstance(selectedEntry.cardData);

            selectedCards.Add(instance);

            // 중복 등장 방지
            availableCards.RemoveAll(
                entry => entry.cardData == selectedEntry.cardData
            );
        }
    }

    // 가중치 기반 카드 추첨
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

    // 실제 카드 인스턴스 생성
    private CardInstance GenerateCardInstance(CardData data)
    {
        CardInstance instance = new CardInstance(data);

        if (data is NumberCardData numberCard)
        {
            instance.cardType = CardType.Number;
            instance.cardOperator = GenerateOperator(numberCard);
            instance.AddOption(CardOption.Operator);

            instance.numberValue = GenerateNumber(numberCard);
            instance.AddOption(CardOption.Number);
        }
        else if (data is PhysicsCardData physicsCard)
        {
            instance.cardType = CardType.Physics;
            instance.physicsType = physicsCard.physicsType.ToString();
            instance.AddOption(CardOption.Physics);
        }
        else if (data is StatCardData statCard)
        {
            instance.cardType = CardType.Stat;
            instance.statType = statCard.statType.ToString();
            instance.AddOption(CardOption.Stat);
        }

        return instance;
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

    // UIManager에서 선택 결과를 전달받음
    public void SendCard(int index)
    {
        if (index < 0 || index >= selectedCards.Count)
            return;

        CardInstance instance = selectedCards[index];

        // UIManager에 선택한 카드 전달
        GameManager.Instance.uiManager.AddCard(instance);

        ApplyCardEffect(instance);

        GameManager.Instance.uiManager.CloseCardSelection();
    }
    private void ApplyCardEffect(CardInstance instance)
    {
        Debug.Log("선택한 카드: " + instance.cardData.cardName);

        if (instance.HasOption(CardOption.Operator) &&
            instance.HasOption(CardOption.Number))
        {
            Debug.Log(
                "연산자: " + instance.cardOperator +
                " / 숫자: " + instance.numberValue
            );
        }
    }
}
