using Mirror;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShopMenu;

public class Gun : InventoryItem, IWeapon
{
    private PlayerManager playerManager;
    [Header("Gun")]
    [SerializeField] private Player inputSystem;
    public Transform firepoint;

    [Header("Projectile Settings")]
    public GameObject projectile;
    public float maxShootDistance = 100f;
    public LayerMask hitLayers;

    [Header("Weapon Properties")]
    public int bulletDamage = 20;

    [Header("Sound Effects")]
    public AudioClip gunshotSFX;

    // Sets the damange of the bullet
    public int Damage
    {
        get { return bulletDamage; }
        set { bulletDamage = value; }
    }

    void Start()
    {
        if(isLocalPlayer)
        {
            // Set Modified Attack Damage
            playerManager = GetComponentInParent<PlayerManager>();
            var damageModifier = playerManager.RangedAttackStat();
            var modifiedDamage = bulletDamage + (bulletDamage * damageModifier);
            bulletDamage = Mathf.RoundToInt(modifiedDamage);
            inputSystem = ReInput.players.GetPlayer(0);
        }
    }

    void Update()
    {
        var ownership = GetComponent<WeaponOwnership>();
        if (ownership != null && ownership.IsOwnedByLocalPlayer() && inputSystem.GetButtonDown("Use Weapon"))
        {
            CmdFire();
        }
    }

    [Command]
    void CmdFire()
    {
        // Raycast for hit detection
        RaycastHit hit;
        bool didHit = Physics.Raycast(firepoint.position, firepoint.forward, out hit, maxShootDistance, hitLayers);

        if (didHit)
        {
            // Hit something
            var hitObject = hit.collider.gameObject;
            var enemyHealth = hitObject.GetComponent<AIBaseLogic>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(bulletDamage, playerManager);
            }
            SpawnBulletVisuals(firepoint.position, firepoint.rotation);
        }
        else
        {
            // No hit, but still spawn the bullet for visual effects
            SpawnBulletVisuals(firepoint.position, firepoint.rotation);
        }
        RPCPlaySFX();
    }

    [ClientRpc]
    void RPCPlaySFX()
    {
        GetComponentInParent<AudioManager>().PlaySFX(gunshotSFX, 0.1f);
    }


    void SpawnBulletVisuals(Vector3 position, Quaternion rotation)
    {
        var projectileInstance = Instantiate(projectile, position, rotation);
        NetworkServer.Spawn(projectileInstance);
        Destroy(projectileInstance, 5.0f);
    }
}
