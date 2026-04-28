using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class NetworkManagerLobby : NetworkManager
{   
    [SerializeField] private int minPlayers;
    [Scene] [SerializeField] private string menuScene = string.Empty;
    
    [Header("Room")]
    [SerializeField] private GameObject roomPlayerPrefab = null;
    [Header("Game")]
    [SerializeField] private GameObject gamePlayerPrefab = null;
    [SerializeField] private GameObject playerSpawnSystem = null;

    public static event Action onClientConnected;
    public static event Action onClientDisconnected;
    public static event Action<NetworkConnectionToClient> OnServerReadied;
    public List<NetworkRoomPlayerLobby> RoomPlayers{get;} = new List<NetworkRoomPlayerLobby>();

    public List<NetworkGamePlayerLobby> GamePlayers{get;} = new List<NetworkGamePlayerLobby>();
    public override void OnClientConnect()
    {       
        base.OnClientConnect();
        onClientConnected?.Invoke();

        StartCoroutine(DelayedAddPlayer());
    } 

    IEnumerator DelayedAddPlayer()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (NetworkClient.connection != null && !NetworkClient.connection.isReady)
        NetworkClient.Ready();  // Mark client ready first
    
        NetworkClient.AddPlayer();  // Now safe to spawn player
        
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        onClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {   
        if(numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().name != "Lobby")
        {   
            conn.Disconnect();
            return;
        }
    } 

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {   
        if(SceneManager.GetActiveScene().name == "Lobby")
        {   
            bool isLeader = RoomPlayers.Count == 0;
            GameObject roomPlayerInstance = Instantiate(roomPlayerPrefab);
            roomPlayerInstance.GetComponent<NetworkRoomPlayerLobby>().IsLeader = isLeader;
            NetworkServer.AddPlayerForConnection(conn,roomPlayerInstance);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();
            RoomPlayers.Remove(player);
            NotifyPlayerOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

    public void NotifyPlayerOfReadyState()
    {
        foreach(var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if(numPlayers < minPlayers) return false;

        foreach(var player in RoomPlayers)
        {
            if(!player.IsReady) return false;
        }

        return true;
    }

    public void StartGame()
    {      
            if(!IsReadyToStart()) return;
            ServerChangeScene("Playground");
    }

    public override void ServerChangeScene(string newSceneName)
    {   
        if(SceneManager.GetActiveScene().name == "Lobby" && newSceneName.StartsWith("Playground"))
        {
            for (int i = RoomPlayers.Count-1; i >= 0; i-- )
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.GetComponent<NetworkGamePlayerLobby>().SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn,gamePlayerInstance);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(sceneName.StartsWith("Playground"))
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    }
} 
