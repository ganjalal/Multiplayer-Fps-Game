using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour 
{
    [HideInInspector] public NetworkIdentity shooterIdentity;
    [SerializeField] private int damage = 25;
    
    void OnTriggerEnter(Collider collider)
    {   
        var victimIdentity = collider.GetComponent<NetworkIdentity>();    
        
        // Only server processes hits
        if (!isServer) return;
        if(victimIdentity != null && victimIdentity != shooterIdentity)
        {   
            var healthScore = victimIdentity.GetComponent<PlayerHealthScore>();
            if (healthScore != null)
            {
                healthScore.ServerTakeDamage(damage,shooterIdentity);
            }
        }
        
        // Destroy on all clients
        NetworkServer.Destroy(gameObject);
    }
}
