using UnityEngine;

public class ChestScript : MonoBehaviour
{
    public GameObject player;  // The player object
    public float interactionRange = 3f;  // Distance to interact with the chest
    public string interactionKey = "f";  // Key to open the chest

    public Renderer cardRenderer;  // The renderer of the card object (or UI image)
    public Texture[] cardTextures;  // Array of card textures

    private bool isPlayerInRange = false;
    private bool isChestOpen = false;

    void Update()
    {
        // Check if player is in range and press the interaction key
        if (isPlayerInRange && !isChestOpen && Input.GetKeyDown(interactionKey))
        {
            OpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void OpenChest()
    {
        isChestOpen = true;
        Debug.Log("Chest Opened!");

        // Randomize and set a texture for the card
        int randomIndex = Random.Range(0, cardTextures.Length);
        cardRenderer.material.mainTexture = cardTextures[randomIndex];
    }

    private void OnGUI()
    {
        // Show interaction prompt when the player is in range
        if (isPlayerInRange && !isChestOpen)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 100, 200, 30), "Press F to open chest");
        }
    }
}
