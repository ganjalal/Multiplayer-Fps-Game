using UnityEngine;
using Mirror;

public class PlayerShootingRigidbody : NetworkBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform shootPoint;      // Gun muzzle position
    [SerializeField] private GameObject bulletPrefab;    // Bullet prefab w/ Rigidbody + NetworkIdentity
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float fireRate = 10f;       // Shots per second

    private float lastShootTime;

    void Update()
    {
        if (!isLocalPlayer || GameManager.Instance.gameEnded) return;

        if (Input.GetButton("Fire1") && Time.time >= lastShootTime + (1f / fireRate))
        {
            CmdShoot();
            lastShootTime = Time.time;
        }
    }

    [Command(requiresAuthority = false)]
    void CmdShoot()
    {
        // Server spawns bullet
        Quaternion rotation = shootPoint.rotation*Quaternion.Euler(90,0,0);
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, rotation);
        bullet.GetComponent<Bullet>().shooterIdentity = GetComponent<NetworkIdentity>();
        bullet.GetComponent<Rigidbody>().linearVelocity = cam.transform.forward * bulletSpeed;
        
        NetworkServer.Spawn(bullet);
        
        // Play effects
        RpcShootEffects();
        
        // Auto-destroy after 5 seconds
        Destroy(bullet, 5f);
    }

    [TargetRpc]
    void RpcShootEffects()
    {
        
    }
}
