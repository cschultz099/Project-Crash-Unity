using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.XR;
using System.Collections;
using UnityEngine.XR.Management;

public class Terminal : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField]
    private Player inputSystem;
    public GameObject playerPrefab;
    public GameObject vrPlayerPrefab;

    [Header("Terminal UI")]
    public GameObject terminalUI;
    [SerializeField] 
    private bool terminalActive = false;
    public TMP_InputField terminalInputField;
    public TMP_Text terminalOutputField;
    public ScrollRect scrollRect;

    [Header("Game Registry")]
    public GameObject[] registeredItems;
    public GameObject[] registeredEnemies;

    private void Start()
    {
        inputSystem = ReInput.players.GetPlayer(0);
        terminalInputField.onSubmit.AddListener(delegate { SubmitCommand(terminalInputField.text); });
    }

    void Update()
    {
        if (isLocalPlayer && inputSystem.GetButtonDown("Activate Terminal"))
        {
            terminalActive = !terminalActive;
            terminalUI.SetActive(terminalActive);

            if(terminalActive)
            {
                terminalInputField.text = "";
                terminalInputField.Select();
                terminalInputField.ActivateInputField();

                playerPrefab.GetComponent<PlayerController>().canMove = false;

                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else
            {
                playerPrefab.GetComponent<PlayerController>().canMove = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void SubmitCommand(string command)
    {
        terminalInputField.text = "";
        terminalInputField.ActivateInputField();
        ProcessCommand(command);
    }

    public void ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');

        if(parts.Length > 0)
        {
            switch(parts[0])
            {
                case "help":
                    terminalOutputField.text += "\nAvailable Commands: \n";
                    terminalOutputField.text += "help - Display this help message\n";
                    terminalOutputField.text += "clear - Clear the terminal\n";
                    terminalOutputField.text += "getgold [amount] - Get amount\n";
                    terminalOutputField.text += "heal - Heal the player\n";
                    terminalOutputField.text += "killall - Kill all enemies\n";
                    terminalOutputField.text += "load [sceneName] - Loads a scene\n";
                    terminalOutputField.text += "spawnenemy [enemyType] [amount] [x] [y] [z] - Spawns a specified amount of an enemy type at given coordinates. If no coordinates are provided, spawns in front of the player.\n";
                    terminalOutputField.text += "spawnitem [item] [amount] - Spawns an item\n";
                    terminalOutputField.text += "teleport [x] [y] [z]\n";
                    terminalOutputField.text += "enable_vr [true/false]";
                    //terminalOutputField.text += "upgrade - Upgrade your weapon\n";
                    break;
                case "clear":
                    terminalOutputField.text = "";
                    break;
                case "getgold":
                    if(parts.Length > 1)
                    {
                        int amount;
                        if (int.TryParse(parts[1], out amount))
                        {
                            gameObject.GetComponentInParent<PlayerManager>().ServerAddCurrency(amount);
                            terminalOutputField.text += "\nAdded " + amount + " gold!";
                        }
                        else
                        {
                            terminalOutputField.text += "\nInvalid amount: " + parts[1];
                        }
                    }
                    else
                    {
                        terminalOutputField.text += "\nUsage: getgold [amount]";
                    }
                    break;
                case "heal":
                    gameObject.GetComponentInParent<PlayerManager>().Heal(100);
                    terminalOutputField.text += "\nPlayer healed!";
                    break;
                case "killall":
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    foreach(GameObject enemy in enemies)
                    {
                        NetworkServer.Destroy(enemy);
                    }
                    terminalOutputField.text += "\nAll enemies killed!";
                    break;
                case "load":
                    if(parts.Length > 1)
                    {
                        string sceneName = parts[1];
                        GameManager.Singleton.AttemptSceneChange(sceneName);
                    }
                    else
                    {
                        terminalOutputField.text += "\nUsage: load [sceneName]";
                    }
                    break;
                case "spawnenemy":
                    string enemyType = parts[1];
                    if (parts.Length >= 6)
                    {
                        if (int.TryParse(parts[2], out int amount) &&
                            float.TryParse(parts[3], out float x) &&
                            float.TryParse(parts[4], out float y) &&
                            float.TryParse(parts[5], out float z))
                        {
                            Vector3 spawnPosition = new Vector3(x, y, z);
                            SpawnEnemiesOnCommand(enemyType, amount, spawnPosition);
                            terminalOutputField.text += $"\n{amount} {enemyType} enemy(ies) spawned at ({x},{y},{z}).";
                        }
                        else
                        {
                            terminalOutputField.text += "\nInvalid command format.";
                        }
                    }
                    else if (parts.Length == 3 && int.TryParse(parts[2], out int defaultAmount))
                    {
                        // Fallback position in front of the player
                        Vector3 spawnPosition = playerPrefab.transform.position + playerPrefab.transform.forward * 2; // Adjust the distance as needed
                        SpawnEnemiesOnCommand(enemyType, defaultAmount, spawnPosition);
                        terminalOutputField.text += $"\n{defaultAmount} {enemyType} enemy(ies) spawned in front of you.";
                    }
                    else
                    {
                        terminalOutputField.text += "\nUsage: spawnenemy [enemyType] [amount] [x] [y] [z]";
                        terminalOutputField.text += "\nOr: spawnenemy [enemyType] [amount] to spawn in front of the player";
                    }
                    break;
                case "spawnitem":
                    if(parts.Length >= 3)
                    {
                        string itemName = parts[1];
                        if (int.TryParse(parts[2], out int amount))
                        {
                            SpawnItem(itemName, amount);
                            terminalOutputField.text += $"\nSpawned {amount} {itemName}(s).";
                        }
                        else
                        {
                            terminalOutputField.text += "\nInvalid amount.";
                        }
                    }
                    else
                    {
                        terminalOutputField.text += "\nUsage: spawnitem [item] [amount]";
                    }
                    break;
                case "teleport":
                    if(parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float y) && float.TryParse(parts[3], out float z))
                        {
                            Vector3 tpPosition = new Vector3(x, y, z);
                            transform.parent.position = tpPosition;
                            terminalOutputField.text += $"\nTeleported to: ({x},{y},{z}).";
                        }
                        else
                        {
                            terminalOutputField.text += "\nUsage: teleport [x] [y] [z]";
                        }
                    }
                    break;
                case "enable_vr":
                    if (parts.Length > 1 && bool.TryParse(parts[1], out bool enableVR))
                    {
                        StartCoroutine(ToggleVRMode(enableVR));
                        terminalOutputField.text += "\nVR " + (enableVR ? "Enabled" : "Disabled");
                    }
                    else
                    {
                        terminalOutputField.text += "\nUsage: enable_vr [true/false]";
                    }
                    break;
                default:
                    terminalOutputField.text += "\nUnknown Command: " + command;
                    break;
            }
        }
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f; // Scroll to bottom
    }

    [Server]
    private void SpawnItem(string itemName, int amount)
    {
        if (registeredItems != null)
        {
            foreach (GameObject item in registeredItems)
            {
                if (item.name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Vector3 spawnPosition = playerPrefab.transform.position + transform.forward * 1;
                        RaycastHit hit;
                        if (Physics.Raycast(spawnPosition, Vector3.down, out hit))
                        {
                            spawnPosition = hit.point;
                            spawnPosition.y += 0.5f;
                            GameObject itemRef = Instantiate(item, spawnPosition, Quaternion.identity);
                            NetworkServer.Spawn(itemRef);
                        }
                    }
                    return;
                }
            }
            terminalOutputField.text += $"\nItem {itemName} not found.";
        }
        else
        {
            terminalOutputField.text += "\nItemsIndex not found in the scene.";
        }
    }

    [Server]
    private void SpawnEnemiesOnCommand(string enemyType, int amount, Vector3 spawnPosition)
    {
        GameObject enemyPrefab = FindEnemyPrefabByType(enemyType);

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy type not found: " + enemyType);
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(spawnedEnemy);
        }
    }

    private GameObject FindEnemyPrefabByType(string enemyType)
    {
        // Search in enemies array
        foreach (var enemy in registeredEnemies)
        {
            if (enemy.name.Equals(enemyType, StringComparison.OrdinalIgnoreCase))
            {
                return enemy;
            }
        }
        return null;
    }

    public IEnumerator ToggleVRMode(bool enableVR)
    {
        if (enableVR)
        {
            var xrGeneralSettings = XRGeneralSettings.Instance;
            xrGeneralSettings.Manager.InitializeLoaderSync();
            if (xrGeneralSettings.Manager.activeLoader != null)
            {
                playerPrefab.SetActive(false);
                vrPlayerPrefab.SetActive(true);

                xrGeneralSettings.Manager.StartSubsystems();
                Debug.Log("VR Subsystems Started");
            }
            else
            {
                Debug.LogError("Initializing XR Loader Failed");
            }
        }
        else
        {
            playerPrefab.SetActive(true);
            vrPlayerPrefab.SetActive(false);

            var xrGeneralSettings = XRGeneralSettings.Instance;
            xrGeneralSettings.Manager.StopSubsystems();
            xrGeneralSettings.Manager.DeinitializeLoader();
            Debug.Log("VR Subsystems Stopped");
        }
        yield return null;
    }
}