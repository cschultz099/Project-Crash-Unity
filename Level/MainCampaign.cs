using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCampaign : NetworkBehaviour
{
    private GameManager gameManager;

    #region Stage Management
    [Header("Stage Management")]
    [ReadOnly] public readonly int initialStageCount = 5;
    public readonly List<string> stageNames = new() { "Stage 1", "Stage 2", "Stage 3", "Stage 4", "Stage 5" };
    #endregion

    #region Enemy Management
    [Header("Enemy Management")]
    public float enemyHealthScaleDifficulty = 0.1f; // 10% increase in health per stage
    public float enemyDamageScaleDifficulty = 0.1f; // 10% increase in damage per stage
    public float endlessModeScaling = 1.1f; // Increase enemy count by 10% per stage beyond initial stages TODO
    #endregion

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }
}