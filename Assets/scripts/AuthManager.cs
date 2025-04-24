using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    // Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference dbReference;

    // Login variables
    [Header("Login")]
    public TMP_InputField loginField; // Both username or email input
    public TMP_InputField passwordLoginField; // Password field
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    // Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    // Firebase paths (move paths to a configurable structure)
    private string playersPath = "players";
    private string inventoryPath = "inventory";
    private string playerStatsPath = "playerStats";
    private string swordUpgradesPath = "swordUpgrades";
    private string resolvesPath = "resolves";
    private string usernamesPath = "Usernames";

    private static AuthManager instance;

    void Start()
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log($"Player is still logged in: {auth.CurrentUser.Email}");
        }
        else
        {
            Debug.Log("No user is logged in.");
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth and Database");
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void LoginButton()
    {
        StartCoroutine(Login(loginField.text, passwordLoginField.text));
    }

    private IEnumerator Login(string loginInput, string password)
    {
        if (string.IsNullOrEmpty(loginInput))
        {
            warningLoginText.text = "Missing username/email";
            yield break;
        }

        if (IsEmail(loginInput))
        {
            StartCoroutine(LoginWithEmail(loginInput, password));
        }
        else
        {
            StartCoroutine(LoginWithUsername(loginInput, password));
        }
    }

    private bool IsEmail(string input)
    {
        return input.Contains("@");
    }

    private IEnumerator LoginWithUsername(string username, string password)
    {
        var getUserIdTask = dbReference.Child(usernamesPath).Child(username).GetValueAsync();
        yield return new WaitUntil(() => getUserIdTask.IsCompleted);

        if (getUserIdTask.Exception != null || !getUserIdTask.Result.Exists)
        {
            warningLoginText.text = "Username not found!";
            yield break;
        }

        string userId = getUserIdTask.Result.Value.ToString();

        var getEmailTask = dbReference.Child(playersPath).Child(userId).Child("email").GetValueAsync();
        yield return new WaitUntil(() => getEmailTask.IsCompleted);

        if (getEmailTask.Exception != null || !getEmailTask.Result.Exists)
        {
            warningLoginText.text = "Failed to retrieve user email!";
            yield break;
        }

        string email = getEmailTask.Result.Value.ToString();
        StartCoroutine(LoginWithEmail(email, password));
    }

    private IEnumerator LoginWithEmail(string email, string password)
    {
        Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogWarning($"Failed to login task with {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            message = GetErrorMessage(errorCode);
            warningLoginText.text = message;
        }
        else
        {
            User = loginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
        }
    }

    private string GetErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail: return "Missing Email";
            case AuthError.MissingPassword: return "Missing Password";
            case AuthError.WrongPassword: return "Wrong Password";
            case AuthError.InvalidEmail: return "Invalid Email";
            case AuthError.UserNotFound: return "Account does not exist";
            default: return "Unknown Error";
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (string.IsNullOrEmpty(_username))
        {
            warningRegisterText.text = "Missing Username";
            yield break;
        }
        if (_password != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Passwords do not match!";
            yield break;
        }

        Task<AuthResult> registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {registerTask.Exception}");
            FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Register Failed!";
            message = GetErrorMessage(errorCode);
            warningRegisterText.text = message;
        }
        else
        {
            User = registerTask.Result.User;

            if (User != null)
            {
                UserProfile profile = new UserProfile { DisplayName = _username };
                Task ProfileTask = User.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(() => ProfileTask.IsCompleted);

                if (ProfileTask.Exception != null)
                {
                    Debug.LogWarning($"Failed to update profile: {ProfileTask.Exception}");
                    warningRegisterText.text = "Username Set Failed!";
                }
                else
                {
                    Debug.Log($"Successfully created user: {User.DisplayName}, UID: {User.UserId}");
                    StartCoroutine(SaveUserData(_username, _email, User.UserId));
                    warningRegisterText.text = "Registration Successful!";
                }
            }
        }
    }

    private IEnumerator SaveUserData(string username, string email, string uid)
    {
        // Create instances of the data classes
        PlayerStatsData playerStats = new PlayerStatsData(); // You can modify this dynamically
        InventoryData inventoryData = new InventoryData(); // Contains health potion with value 0
        SwordUpgradeData swordUpgrade = new SwordUpgradeData(); // Default sword
        ResolvePointsData resolvePointsData = new ResolvePointsData(); // Default resolve points

        // Convert to dictionaries for Firebase saving
        Dictionary<string, object> playerData = new Dictionary<string, object>
    {
        { "uid", uid },
        { "email", email }
    };

        // Save player data
        yield return StartCoroutine(SaveData(playersPath, username, playerData));

        // Save inventory data (health potion will be included with value 0)
        yield return StartCoroutine(SaveData(inventoryPath, username, inventoryData.ToDictionary()));

        // Save player stats
        yield return StartCoroutine(SaveData(playerStatsPath, username, playerStats.ToDictionary()));

        // Save sword upgrade
        yield return StartCoroutine(SaveData(swordUpgradesPath, username, swordUpgrade.ToDictionary()));

        // Save resolve points
        yield return StartCoroutine(SaveData(resolvesPath, username, resolvePointsData.ToDictionary()));
    }


    private IEnumerator SaveData(string path, string username, Dictionary<string, object> data)
    {
        var saveTask = dbReference.Child(path).Child(username).SetValueAsync(data);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Exception != null)
        {
            Debug.LogWarning($"Failed to save data at {path}: {saveTask.Exception}");
        }
    }
}

// PlayerStatsData Class
public class PlayerStatsData
{
    public int strength;
    public int vitality;
    public int lethality;
    public int rechargerArmor;
    public int stamina;

    public PlayerStatsData(int strength = 1, int vitality = 1, int lethality = 1, int rechargerArmor = 1, int stamina = 1)
    {
        this.strength = strength;
        this.vitality = vitality;
        this.lethality = lethality;
        this.rechargerArmor = rechargerArmor;
        this.stamina = stamina;
    }

    // Convert to Dictionary
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "strength", strength },
            { "vitality", vitality },
            { "lethality", lethality },
            { "rechargerArmor", rechargerArmor },
            { "stamina", stamina }
        };
    }
}

// InventoryData Class
public class InventoryData
{
    public List<Item> items = new List<Item>();

    // Add default health potion item with value 0
    public InventoryData()
    {
        items.Add(new Item("Health Potion", 0));
    }

    // Convert to Dictionary
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> inventoryDict = new Dictionary<string, object>();

        foreach (var item in items)
        {
            inventoryDict[item.name] = item.value;
        }

        return inventoryDict;
    }
}

// Item Class for Inventory
public class Item
{
    public string name;
    public int value;

    public Item(string name, int value)
    {
        this.name = name;
        this.value = value;
    }
}


// SwordUpgradeData Class
public class SwordUpgradeData
{
    public int upgradeLevel = 0;

    // Convert to Dictionary
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "upgradeLevel", upgradeLevel }
        };
    }
}

// ResolvePointsData Class
public class ResolvePointsData
{
    public int resolvePoints = 0;

    // Convert to Dictionary
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "resolvePoints", resolvePoints }
        };
    }
}

