using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBuff : PowerUpBase
{
    public float speedIncrease = 10.0f;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isServer)
        {
            ApplyBuffToAllPlayers();
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    private void ApplyBuffToAllPlayers()
    {
        var players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            player.RpcIncreaseRunningSpeed(speedIncrease, powerUpDurration);
        }
    }
}
