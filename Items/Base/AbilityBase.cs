using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class AbilityBase : NetworkBehaviour
{
    public Player inputSystem;
    public float abilityCooldown = 10f;
    public float abilityDuration = 0.1f;
    public Sprite icon;

    protected virtual void Start()
    {
        inputSystem = ReInput.players.GetPlayer(0);
    }

    protected virtual void Update()
    {

    }

    protected virtual void OnTriggerEnter(Collider other)
    {

    }
}
