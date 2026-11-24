using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardUI : MonoBehaviour
{
    public Button cardButton;

    public Image cardImage;
    public TMP_Text cardName;
    public TMP_Text cardDescription;

    public void SetCard(CardData card)
    {
        cardName.text = card.cardName;
        cardDescription.text = card.description;

        // 추후 CardData에 이미지 필드를 추가하면 연결
         cardImage.sprite = card.cardImage;
    }

    public void SetClickAction(Action action)
    {
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() => action());
    }
}