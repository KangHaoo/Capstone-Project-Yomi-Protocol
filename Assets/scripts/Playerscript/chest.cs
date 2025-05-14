using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
    public GameObject chestUI;
    public Button[] cardButtons;
    public List<Card> allCards;

    private bool playerInRange = false;
    private bool chestOpened = false;

    void Update()
    {
        if (playerInRange && !chestOpened && Input.GetKeyDown(KeyCode.F))
        {
            OpenChest();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void OpenChest()
    {
        chestOpened = true;
        chestUI.SetActive(true);
        ShowRandomCards();
    }

    void ShowRandomCards()
    {
        if (allCards.Count < 3)
        {
            Debug.LogWarning("Not enough cards to display.");
            return;
        }

        // Shuffle and get 3 random cards
        List<Card> shuffled = new List<Card>(allCards);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            Card selectedCard = shuffled[i];

            // Set button text
            Text buttonText = cardButtons[i].GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = selectedCard.cardName;

            // Set button icon
            Transform iconTransform = cardButtons[i].transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = selectedCard.cardIcon;
                    iconImage.enabled = true;
                    Debug.Log($"Assigned icon to card {i}: {selectedCard.cardIcon.name}");
                }
            }

            // Optional: clear and assign click behavior
            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => OnCardSelected(selectedCard));
        }
    }

    void OnCardSelected(Card selectedCard)
    {
        Debug.Log("You selected card: " + selectedCard.cardName);
        // Optional: Add card to inventory, close UI, etc.
    }
}


[System.Serializable]
public class Card
{
    public string cardName;
    public Sprite cardIcon;
}
