using Mirror;
using UnityEngine;

public class WeaponOwnership : NetworkBehaviour
{
    [SyncVar]
    private GameObject owner;

    public void SetOwner(GameObject player)
    {
        owner = player;
    }

    public bool IsOwnedByLocalPlayer()
    {
        return owner != null && owner.GetComponent<NetworkIdentity>().isLocalPlayer;
    }
}
