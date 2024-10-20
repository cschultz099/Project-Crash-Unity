using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerSubMenu : NetworkBehaviour
{
    private CustomNetworkManager manager;

    [Header("Player Settings")]
    public PlayerController playerController;
    [SerializeField]
    private Player inputSystem;

    [Header("UI References")]
    public GameObject subUIREF;
    public GameObject inventoryUI;

    void Awake()
    {
        inputSystem = ReInput.players.GetPlayer(0);
        manager = CustomNetworkManager.singleton;
    }

    void Update()
    {
        if (isLocalPlayer && inputSystem.GetButtonDown("Activate Menu"))
        {
            bool menuIsActive = !subUIREF.activeSelf;
            subUIREF.SetActive(menuIsActive);
            inventoryUI.SetActive(false);

            Cursor.lockState = menuIsActive ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = menuIsActive;

            // Toggle player movement and rotation based on the menu state
            if (playerController != null)
            {
                playerController.canMove = !menuIsActive;
                playerController.canRotate = !menuIsActive;
            }
        }
    }
    public void returnToMenu()
    {
        manager.serverShutdownProcess();
    }

    public void returnToGame()
    {
        subUIREF.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure player can move and rotate again
        if (playerController != null)
        {
            playerController.canMove = true;
            playerController.canRotate = true;
        }
    }

    public void options()
    {

    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
