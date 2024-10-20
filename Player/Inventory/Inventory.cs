using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class Inventory : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField]
    private Player inputSystem;
    public PlayerController playerController;

    [Header("Inventory UI")]
    public GameObject inventoryREF;

    void Awake()
    {
        inputSystem = ReInput.players.GetPlayer(0);
    }

    void Update()
    {
        if (isLocalPlayer && inputSystem.GetButtonDown("Open Inventory"))
        {
            bool menuIsActive = !inventoryREF.activeSelf;
            inventoryREF.SetActive(menuIsActive);

            Cursor.lockState = menuIsActive ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = menuIsActive;

            // Toggle player rotation based on the menu state
            if (playerController != null)
            {
                playerController.canRotate = !menuIsActive;
            }
        }
    }
}