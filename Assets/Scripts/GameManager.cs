using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameManager : NetworkBehaviour
{   
    public static GameManager Instance;
    public Text scoreText;
    public GameObject gameOverPanel;

    [SyncVar(hook = nameof(OnGameEnded))] public bool gameEnded = false;
    
    public override void OnStartServer()
    {
        Debug.Log("Server: Setting networked GameManager instance");
        Instance = this;  // NETWORKED object
    }

    public override void OnStartClient()
    {
        Debug.Log("Client: Setting networked GameManager instance");  
        Instance = this;  // NETWORKED object
    }

    void OnGameEnded(bool oldValue, bool newValue)
    {   
        gameOverPanel.SetActive(newValue);
    }

}
