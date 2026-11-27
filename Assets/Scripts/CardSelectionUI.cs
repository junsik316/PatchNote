
using System.Collections.Generic;
using UnityEngine;

public class CardSelectionUI : MonoBehaviour
{
    public GameObject cardPanel;
    public GameObject cardPrefab;
    public Transform cardContainer;

    private List<CardUI> cardUIs = new List<CardUI>();

    public bool IsOpen => cardPanel.activeSelf;

    public void ShowCards(List<CardInstance> cards)
    {
        cardPanel.SetActive(true);

        ClearCards();

        for (int i = 0; i < cards.Count; i++)
        {
            GameObject obj =
                Instantiate(cardPrefab, cardContainer);

            CardUI ui = obj.GetComponent<CardUI>();

            ui.SetCard(cards[i].cardData);

            int index = i;

            ui.SetClickAction(() =>
                GameManager.Instance.cardManager.SendCard(index)
            );

            cardUIs.Add(ui);
        }
    }

    private void ClearCards()
    {
        foreach (CardUI ui in cardUIs)
        {
            Destroy(ui.gameObject);
        }

        cardUIs.Clear();
    }

    public void Close()
    {
        cardPanel.SetActive(false);
    }
}
