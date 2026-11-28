using System.Collections.Generic;
using UnityEngine;

// Resolves the same sequence of root chains shown in InventoryUI.
// No base or final physics values are kept here.
public sealed class CardEffectEvaluator
{
    public float EvaluatePhysics(IReadOnlyList<CardInstance> inventory,
        EffectTarget target, PhysicsType property, float baseValue)
    {
        if (inventory == null)
            return baseValue;

        float value = baseValue;
        foreach (CardInstance root in inventory)
        {
            if (root == null || root.parentCard != null)
                continue;

            EffectTarget chainTarget = EffectTarget.Player; // Legacy chains.
            CardInstance propertyCard = root;
            if (root is TargetCardInstance subject)
            {
                chainTarget = subject.target;
                propertyCard = root.childCard;
            }

            if (chainTarget != target ||
                !(propertyCard is PhysicsCardInstance physics) ||
                physics.physicsType != property)
                continue;

            var visited = new HashSet<CardInstance> { root, propertyCard };
            CardInstance current = propertyCard.childCard;
            while (current != null && visited.Add(current))
            {
                if (current is NumberCardInstance number)
                    value = ApplyNumber(value, number);
                current = current.childCard;
            }

            if (current != null)
                Debug.LogWarning("카드 체인에 순환 참조가 있습니다.");
        }
        return value;
    }

    private static float ApplyNumber(float value, NumberCardInstance card)
    {
        float number = card.numberValue;
        float result;
        switch (card.cardOperator)
        {
            case CardOperator.Multiply: result = value * number; break;
            case CardOperator.Divide:
                if (Mathf.Approximately(number, 0f))
                {
                    Debug.LogWarning("0으로 나눌 수 없습니다.");
                    return value;
                }
                result = value / number;
                break;
            case CardOperator.Add: result = value + number; break;
            case CardOperator.Subtract: result = value - number; break;
            default: return value;
        }

        if (float.IsNaN(result) || float.IsInfinity(result))
        {
            Debug.LogWarning("유효하지 않은 카드 계산 결과입니다.");
            return value;
        }
        return result;
    }
}
