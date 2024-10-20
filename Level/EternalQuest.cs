using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EternalQuest : NetworkBehaviour
{
    private GameManager gameManager;

    #region Enemy Management
    [Header("Enemy Management")]
    public float enemyHealthScaleDifficulty = 0.1f; // 10% increase in health per stage
    public float enemyDamageScaleDifficulty = 0.1f; // 10% increase in damage per stage
    public float endlessModeScaling = 1.1f; // Increase enemy count by 10% per stage beyond initial stages TODO
    #endregion

    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    void Update()
    {
        if (isServer)
        {
            if (SceneManager.GetSceneByName("EternalQuest").isLoaded)
            {

            }
        }
    }
}