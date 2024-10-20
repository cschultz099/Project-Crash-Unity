using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class AIBaseLogic : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("All recognized player objects in the game")]
    [ReadOnly] protected GameObject[] players;

    [Tooltip("Targeted player")]
    [ReadOnly] public GameObject player;

    private NavMeshAgent navAgent;

    [Header("AI Stats")]
    [Tooltip("Maximum health of the AI")]
    public int maxHealth = 100;

    [SyncVar]
    [Tooltip("Current health of the AI")]
    [ReadOnly] public int currentHealth = 100;

    [Tooltip("Damage dealt by AI")]
    public int attackDamage = 20;

    [Tooltip("Cooldown period between damages")]
    public float damageCooldown = 1f;

    [Tooltip("Time required to windup an attack")]
    public float windupAttack = 2f;

    private float nextDamageTime = 0f;
    private float nextWindupAttackTime = 0f;

    [Space]

    [Header("AI Controller")]
    [Tooltip("Initial detection range to player")]
    public float initialDetectionRange = 10f;

    [Tooltip("Detection range for proximity to the player")]
    public float proximityDetectionRange = 5f;

    [Tooltip("Field of view for detecting players")]
    public float detectionFOV = 120f; // DesiredFOV/2

    [Tooltip("Stopping distance from the target player")]
    public float stoppingDistance = 1.5f;

    [Header("AI SFX")]
    public AudioClip[] hurtSFX;
    public AudioClip[] walkingSFX;
    public AudioClip[] closeRangeAttackSFX;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.stoppingDistance = stoppingDistance;
    }

    protected virtual void Update()
    {
        if (!isServer) return;

        // The most basic of AI functionality
        FindPlayers();
    }

    #region Enemy Controller Logic
    [Server]
    protected virtual void ControlPlayer()
    {
        if (player != null)
        {
            navAgent.SetDestination(player.transform.position);
        }
    }

    #endregion

    #region Enemy AI Logic
    [Server]
    // A base method for universal use
    protected void FindPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    [Server]
    protected virtual void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayerObj = null;

        foreach (GameObject playerObj in players)
        {
            float distance = Vector3.Distance(transform.position, playerObj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayerObj = playerObj;
            }
        }
        player = closestPlayerObj;
    }

    // TODO: Initital player detection radius should be different from the following distance
    [Server]
    protected virtual void FindClosestPlayerInSight()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayerRef = null;

        // Iterate through all found players
        foreach (GameObject playerObj in players)
        {
            Vector3 directionToPlayer = playerObj.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float distanceToPlayer = directionToPlayer.magnitude;

            if (angleToPlayer <= detectionFOV)
            {
                // Raycast from AI to player to check for line of sight
                if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, proximityDetectionRange))
                {
                    // Check if the hit object is indeed the player we are checking against
                    if (hit.collider.gameObject == playerObj && distanceToPlayer < closestDistance)
                    {
                        closestDistance = distanceToPlayer;
                        closestPlayerRef = playerObj;
                    }
                }
            }
        }

        player = closestPlayerRef;

        // Handle case where no player is in sight
        if (closestPlayerRef == null)
        {
            player = null;
        }
    }

    [Server]
    protected virtual void FindClosestPlayerByProximity()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayerRef = null;

        foreach (GameObject playerObj in players)
        {
            float distance = Vector3.Distance(transform.position, playerObj.transform.position);
            if (distance < proximityDetectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayerRef = playerObj;
            }
        }
        player = closestPlayerRef;

        if (closestPlayerRef == null)
        {
            player = null;
        }
    }

    [Server]
    protected virtual void FindClosestPlayerInSightWithProximity()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayerRef = null;

        // Iterate through all found players
        foreach (GameObject playerObj in players)
        {
            Vector3 directionToPlayer = playerObj.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer < proximityDetectionRange && angleToPlayer <= detectionFOV)
            {
                RaycastHit hit;
                // Raycast from AI to player to check for line of sight
                if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, proximityDetectionRange))
                {
                    // Check if the hit object is indeed the player we are checking against
                    if (hit.collider.gameObject == playerObj && distanceToPlayer < closestDistance)
                    {
                        closestDistance = distanceToPlayer;
                        closestPlayerRef = playerObj;
                    }
                }
            }
        }

        player = closestPlayerRef;

        // Handle case where no player is in sight
        if (closestPlayerRef == null)
        {
            player = null;
        }
    }

    [Server]
    protected virtual void ReturnBackToOrigin()
    {

    }

    [Server]
    protected virtual void WanderTillPlayerTargeted()
    {

    }

    [Server]
    protected virtual void LookAtClosestPlayer()
    {
        if (player!= null)
        {
            Vector3 directionAwayFromPlayer = player.transform.position - transform.position;
            directionAwayFromPlayer.y = 0; // This ensures the NPC only rotates on the Y axis
            if (directionAwayFromPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionAwayFromPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
    }

    [Server]
    protected virtual void CloseRangeAttack()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= navAgent.stoppingDistance)
            {
                // Check if we're past the cooldown period and not currently in a windup phase
                if (Time.time >= nextDamageTime && nextWindupAttackTime == 0)
                {
                    // Start the windup period
                    nextWindupAttackTime = Time.time + windupAttack;
                }

                // If we're in the windup phase and the windup time has elapsed
                if (nextWindupAttackTime != 0 && Time.time >= nextWindupAttackTime)
                {
                    // Perform the attack
                    player.GetComponentInParent<PlayerManager>().TakeDamage(attackDamage);
                    PlayCloseAttackSFX();
                    // Set the next time an attack can be made
                    nextDamageTime = Time.time + damageCooldown;
                    // Reset the windup timer for the next attack
                    nextWindupAttackTime = 0; // Make sure to reset this so the process can start again
                }
            }
            //Debug.Log($"Time: {Time.time}, NextWindup: {nextWindupAttackTime}, NextDamage: {nextDamageTime}");
        }
    }

    [Server]
    protected virtual void ShootAttack()
    {

    }

    [Server]
    public virtual void TakeDamage(int damage, PlayerManager playerAttacking)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        PlayHurtSFX(); // TODO: Sound gets cut off early when death occurs

        if (currentHealth <= 0)
        {
            Die(playerAttacking);
        }
    }

    [Server]
    protected virtual void Die(PlayerManager playerAttacking)
    {
        NetworkServer.Destroy(gameObject);
        SpawnLoot(playerAttacking);
    }

    [Server]
    protected virtual void SpawnLoot(PlayerManager playerAttacking)
    {
        if (gameObject.TryGetComponent<EnemyLoot>(out var enemyLoot))
        {
            var luckModifier = playerAttacking.LuckStat();
            enemyLoot.SpawnCurrency(luckModifier);
            enemyLoot.SpawnHealthPack(luckModifier);
            enemyLoot.SpawnLoot(luckModifier);
        }
    }
    #endregion

    #region SFX
    [ClientRpc]
    public void PlayHurtSFX()
    {
        StartCoroutine(GetComponent<AudioManager>().PlayRandomSFXAtChanceWithCooldown(hurtSFX, 0.5f, 5f));
    }

    [ClientRpc]
    public void PlayCloseAttackSFX()
    {
        GetComponent<AudioManager>().PlayRandomSFX(closeRangeAttackSFX);
    }
    #endregion
}