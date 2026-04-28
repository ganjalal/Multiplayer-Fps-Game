using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;

public class PlayerHealthScore : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth;
    
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public Slider slider;
    private FPSController fps;

    [SyncVar(hook = nameof(OnScoreChanged))]
    public int score = 0;
    
    public Text scoreText;

    private void Start()
    {   fps = GetComponent<FPSController>();
        if (isServer)
        {
            currentHealth = maxHealth;
        }
    }

    public override void OnStartLocalPlayer()
    {
        StartCoroutine(SetupUI());
    }

    IEnumerator SetupUI()
    {
        yield return null;
        scoreText = GameManager.Instance.scoreText;
    }


    // This is called on SERVER only, triggered by a client Command
    [Server]
    public void ServerTakeDamage(int damage, NetworkIdentity attacker)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0 && isServer)
        {
            HandleDeath(attacker);
        }
    }

    [Server]
    private void HandleDeath(NetworkIdentity attacker)
    {   
        if(attacker != null && attacker.connectionToClient != connectionToClient)
        {   
            PlayerHealthScore killerScore = attacker.GetComponent<PlayerHealthScore>();
            if(killerScore != null)
            {   
                killerScore.score += 1;
            }
        }
        
        int rand = Random.Range(0,PlayerSpawnSystem.spawnPoints.Count);
        Vector3 pos = PlayerSpawnSystem.spawnPoints[rand].position;
        RpcRespawn(pos);
        currentHealth = maxHealth;
    }

    [ClientRpc]
    void RpcRespawn(Vector3 pos)
    {
        transform.position = pos;
        fps?.SetCanMove(false);

        StartCoroutine(RespawnDelay());
    }
    IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(1.5f);
        fps?.SetCanMove(true);
    }
    
    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        slider.value = newHealth;
    }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {   
        if(scoreText == null) return;
        scoreText.text = $"kills: {score}";
    }

    [Server]
    public void ServerResetForRound(int index)
    {
        score = 0;
        currentHealth = maxHealth;
        OnScoreChanged(0, 0);
        OnHealthChanged(maxHealth, maxHealth);
        Vector3 pos = PlayerSpawnSystem.spawnPoints[index].position;
        RpcRespawn(pos);
    }

}
