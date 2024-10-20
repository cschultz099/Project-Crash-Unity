using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodeBase : MonoBehaviour
{
    [Header("Enemy Management")]
    public float enemyHealthScaleDifficulty = 0.1f; // 10% increase in health per stage
    public float enemyDamageScaleDifficulty = 0.1f; // 10% increase in damage per stage
    public float endlessModeScaling = 1.1f; // Increase enemy count by 10% per stage beyond initial stages
    private GameObject[] enemySpawns;
    private GameObject[] randomEnemySpawns;
}
