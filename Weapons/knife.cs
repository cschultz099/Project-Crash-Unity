using Mirror;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShopMenu;

public class knife : InventoryItem, IWeapon
{
    private PlayerManager playerManager;
    [Header("Knife")]
    private Player inputSystem;

    [Header("Lunge Settings")]
    public float lungeDistance = 1f;
    public float lungeSpeed = 10f;
    private bool isLunging = false;
    private Vector3 originalPosition;

    [Header("Properties")]
    public int damage = 50;

    [Header("Sound Effects")]
    public AudioClip[] sfx;

    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    void Start()
    {
        if(isLocalPlayer)
        {
            // Set Modified Attack Damage
            playerManager = GetComponentInParent<PlayerManager>();
            var damageModifier = playerManager.meleeAttackStat();
            var modifiedDamage = damage + (damage * damageModifier);
            damage = Mathf.RoundToInt(modifiedDamage);
            inputSystem = ReInput.players.GetPlayer(0);
            originalPosition = weaponPrefab.transform.localPosition;
        }
    }

    void Update()
    {
        if (!isLunging && originalPosition == Vector3.zero)
        {
            originalPosition = weaponPrefab.transform.localPosition;
        }
        if(gameObject.activeSelf == false)
        {
            OnWeaponSwitchedAway();
        }

        var ownership = GetComponent<WeaponOwnership>();
        if (ownership != null && ownership.IsOwnedByLocalPlayer() && inputSystem.GetButtonDown("Use Weapon") && !isLunging)
        {
            CmdStartLunge();
        }
    }

    [Command]
    void CmdStartLunge()
    {
        RpcDoLunge();
    }

    [ClientRpc]
    void RpcDoLunge()
    {
        StartCoroutine(Lunge());
    }

    IEnumerator Lunge()
    {
        GetComponentInParent<AudioManager>().PlayRandomSFX(sfx);
        isLunging = true;
        Vector3 originalPosition = weaponPrefab.transform.localPosition;
        Vector3 targetPosition = originalPosition + -Vector3.forward * lungeDistance;

        while(Vector3.Distance(weaponPrefab.transform.localPosition, targetPosition) > 0.01f)
        {
            weaponPrefab.transform.localPosition = Vector3.MoveTowards(weaponPrefab.transform.localPosition, targetPosition, lungeSpeed * Time.deltaTime);
            yield return null;
        }
        while(Vector3.Distance(weaponPrefab.transform.localPosition, originalPosition) > 0.01f)
        {
            weaponPrefab.transform.localPosition = Vector3.MoveTowards(weaponPrefab.transform.localPosition, originalPosition, lungeSpeed * Time.deltaTime);
            yield return null;
        }
        isLunging = false;
    }
    public void OnWeaponSwitchedAway()
    {
        ResetKnifePosition();
    }
    public void ResetKnifePosition()
    {
        if (isLunging)
        {
            StopCoroutine("Lunge");
            weaponPrefab.transform.localPosition = originalPosition;
            isLunging = false;
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        AIBaseLogic enemyHealth = other.GetComponent<AIBaseLogic>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, playerManager);
        }
    }
}
