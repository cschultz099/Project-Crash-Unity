using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBuff : PowerUpBase
{
    public float jumpHeightIncrease = 10.0f;

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
            player.RpcIncreaseJumpHeight(jumpHeightIncrease, powerUpDurration);
        }
    }
}
