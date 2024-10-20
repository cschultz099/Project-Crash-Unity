using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DashAbility : AbilityBase
{
    public float dashSpeed = 50.0f;
    private PlayerController playerController;

    protected override void Start()
    {
        base.Start();

        if(GetComponent<PlayerController>() != null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isServer)
        {
            if(!other.GetComponentInChildren<PlayerController>().gameObject.GetComponent<DashAbility>())
            {
                other.GetComponentInChildren<PlayerController>().AddComponent<DashAbility>();
                other.transform.parent.GetComponentInChildren<AbilitySlot>().SetIcon(icon);
            }
            NetworkServer.Destroy(gameObject);
        }
    }

    protected override void Update()
    {
        if(inputSystem.GetButtonDown("Use Ability") && playerController != null)
        {
            playerController.PerformDash(dashSpeed, abilityDuration);
        }
    }
}
