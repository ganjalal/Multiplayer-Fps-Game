using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{   
    [SerializeField] private NetworkManagerLobby networkManager = null;
    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private InputField ipAddressField = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        NetworkManagerLobby.onClientConnected += HandleClientConnected;
        NetworkManagerLobby.onClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerLobby.onClientConnected -= HandleClientConnected;
        NetworkManagerLobby.onClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressField.text;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();
        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }

}
