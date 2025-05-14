using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player References")]
    public PlayerHealth playerHealth;

    private const string PlayerHealthKey = "PlayerHealth"; // Player health save key

    void Awake()
    {
        // Singleton pattern: ensure only one instance of GameManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep GameManager between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load health from PlayerPrefs
        if (playerHealth != null)
        {
            float savedHealth = PlayerPrefs.GetFloat(PlayerHealthKey, playerHealth.maxHealth); // Load saved health or use maxHealth if no saved value
            playerHealth.SetHealth(savedHealth); // Set player health to saved value
        }
    }

    // Call this function to save health when changing scenes
    public void SavePlayerHealth()
    {
        if (playerHealth != null)
        {
            PlayerPrefs.SetFloat(PlayerHealthKey, playerHealth.currentHealth); // Save current health
            PlayerPrefs.Save(); // Ensure data is written
        }
    }

    // Example: Save health when the player goes to a new scene
    public void ChangeScene(string sceneName)
    {
        SavePlayerHealth(); // Save health before changing the scene
        SceneManager.LoadScene(sceneName); // Load the new scene
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // Press H to take damage
        {
            DamagePlayer(10f); // Deal 10 damage
        }

        if (Input.GetKeyDown(KeyCode.J)) // Press J to heal
        {
            HealPlayer(5f); // Heal 5 health
        }
    }

    // Method to damage the player
    public void DamagePlayer(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(amount); // Call the TakeDamage method from PlayerHealth
        }
    }

    // Method to heal the player
    public void HealPlayer(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(amount); // Call the Heal method from PlayerHealth
        }
    }
}
