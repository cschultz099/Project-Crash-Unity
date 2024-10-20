using Mirror;
using UnityEngine;
using Rewired;

public class PlayerShopTrigger : NetworkBehaviour
{
    private Player inputSystem;
    public GameObject shopMenuUI;
    PlayerController player;
    private bool playerIsInTrigger = false;

    private void Start()
    {
        inputSystem = ReInput.players.GetPlayer(0);
    }

    void Update()
    {
        if(player != null && player.isLocalPlayer && playerIsInTrigger && inputSystem.GetButtonDown("Interact"))
        {
            ShopOpen();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponentInParent<NetworkIdentity>().isLocalPlayer)
        {
            var tempPlayer = other.GetComponentInChildren<PlayerController>();
            if (tempPlayer != null)
            {
                player = tempPlayer;
            }
        }
        playerIsInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponentInParent<NetworkIdentity>().isLocalPlayer)
        {
            var tempPlayer = other.GetComponentInChildren<PlayerController>();
            if (tempPlayer != null)
            {
                player = tempPlayer;
            }
        }
        playerIsInTrigger = false;
    }

    public void ShopOpen()
    {
        // Enable the shop UI for the local player who entered the trigger
        shopMenuUI.SetActive(true);
        shopMenuUI.GetComponentInParent<ShopMenu>().SetCurrentPlayer(player.GetComponentInParent<NetworkIdentity>());
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        player.GetComponent<PlayerController>().canMove = false;
        player.GetComponent<PlayerController>().canRotate = false;
    }

    public void ShopClose()
    {
        // Disable the shop UI when the local player exits the trigger
        shopMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.canMove = true;
        player.canRotate = true;
    }
}
