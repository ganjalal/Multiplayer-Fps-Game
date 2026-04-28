using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerNameInput : MonoBehaviour
{   
    [Header("UI")]
    [SerializeField] private InputField nameInputField = null;
    [SerializeField] private Button continueButton = null;
    
    public static string DisplayName{get;private set;}
    private const string playerPrefsNameKey = "Player Name";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SetupInputField();    
    }

    private void SetupInputField()
    {
        if(!PlayerPrefs.HasKey(playerPrefsNameKey))
            return;

        string defaultName = PlayerPrefs.GetString(playerPrefsNameKey);
        nameInputField.text = defaultName;
        SetPlayerName(defaultName);    
    }

    public void SetPlayerName(string name)
    {
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;
        PlayerPrefs.SetString(playerPrefsNameKey,DisplayName);
    }
}
