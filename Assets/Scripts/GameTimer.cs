using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;

public class GameTimer : NetworkBehaviour
{
    [SerializeField] private float matchDuration = 120f;
    [SyncVar(hook = nameof(OnTimeChanged))] public float timeRemaining;

    [Header("UI References")]
    public Text timeText;
    public GameObject gameOverPanel;
    public Text winnerText;

    public Button nextRoundButton;
    
    public override void OnStartServer()
    {
        timeRemaining = matchDuration;
        nextRoundButton.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isServer || GameManager.Instance.gameEnded) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
        }
    }

    private void OnTimeChanged(float oldTime, float newTime)
    {
        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        if (timeText != null)
            timeText.text = $"{minutes:00}:{seconds:00}";
    }

    [Server]
    private void EndGame()
    {   
        GameManager.Instance.gameEnded = true;
        // Find player with most kills (assumes PlayerHealthScore with 'score' SyncVar)
        var players = FindObjectsOfType<PlayerHealthScore>();
        var topPlayer = players.OrderByDescending(p => p.score).FirstOrDefault();
        int topScore = 0;
        if(topPlayer != null)
            topScore = topPlayer.score;

        int index = 0;
        foreach(var player in players)
        {
            player.ServerResetForRound(index++);
        }
    
        if (topPlayer != null)
        {
            RpcAnnounceWinner(topScore, topPlayer.netIdentity.connectionToClient.identity.name);
        }
        else
        {
            RpcAnnounceWinner(0, "No one");
        }
    }

    [Server]
    public void StartNextRound()
    {   
        GameManager.Instance.gameEnded = false;
        timeRemaining = matchDuration;
    }

    [ClientRpc]
    private void RpcAnnounceWinner(int topKills, string winnerName)
    {   
        if (winnerText != null && topKills != 0)
            winnerText.text = $"Winner: {winnerName} with {topKills} kills!";
        else
            winnerText.text = $"No one Killed Anyone!!";    
    }
    
    public void OnQuitButton()
    {
        if (NetworkServer.active)  // Host or dedicated server
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.active)  // Pure client
            NetworkManager.singleton.StopClient();
    
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

}