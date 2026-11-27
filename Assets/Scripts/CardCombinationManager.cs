using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardCombinationRule
{
    public CardOption parentOption;
    public CardOption childOption;

    public CardCombinationRule(
        CardOption parent,
        CardOption child)
    {
        parentOption = parent;
        childOption = child;
    }
}

public class CardCombinationManager : MonoBehaviour
{
    [Header("허용되는 카드 조합")]
    public List<CardCombinationRule> validRules =
        new List<CardCombinationRule>()
        {
            new CardCombinationRule(
                CardOption.Stat,
                CardOption.Operator | CardOption.Number
            ),

            new CardCombinationRule(
                CardOption.Physics,
                CardOption.Operator | CardOption.Number
            ),

            new CardCombinationRule(
                CardOption.Operator | CardOption.Number,
                CardOption.Operator | CardOption.Number
            )
        };

    // 카드 연결
    public bool TryConnect(
        CardInstance parent,
        CardInstance child)
    {
        if (parent == null || child == null)
            return false;

        // 자기 자신과 연결할 수 없음
        if (parent == child)
            return false;

        // 부모에게 이미 자식이 있음
        if (parent.childCard != null)
            return false;

        // 자식에게 이미 부모가 있음
        if (child.parentCard != null)
            return false;

        // 순환 연결 방지
        if (CreatesCycle(parent, child))
            return false;

        // 조합 옵션 검사
        if (!IsValidCombination(parent, child))
            return false;

        parent.childCard = child;
        child.parentCard = parent;

        Debug.Log("카드 연결 성공");

        return true;
    }

    // 유효한 조합인지 검사
    private bool IsValidCombination(
        CardInstance parent,
        CardInstance child)
    {
        foreach (CardCombinationRule rule in validRules)
        {
            if (parent.HasOption(rule.parentOption) &&
                child.HasOption(rule.childOption))
            {
                return true;
            }
        }

        return false;
    }

    // 순환 구조 검사
    private bool CreatesCycle(
        CardInstance parent,
        CardInstance child)
    {
        CardInstance current = child;

        while (current != null)
        {
            if (current == parent)
                return true;

            current = current.childCard;
        }

        return false;
    }

    // 카드 연결 해제
    public void Disconnect(CardInstance child)
    {
        if (child == null || child.parentCard == null)
            return;

        CardInstance parent = child.parentCard;

        if (parent.childCard == child)
            parent.childCard = null;

        child.parentCard = null;

        Debug.Log("카드 연결 해제");
    }
}