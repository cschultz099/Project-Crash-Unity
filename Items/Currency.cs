using Mirror;
using UnityEngine;

public class Currency : NetworkBehaviour
{
    public int currencyWorth = 1;
    public float rotationSpeed = 60f;
    public float moveSpeed = 2f;
    public float moveHeight = 0.07f;

    private Vector3 startPositon;

    void Start()
    {
        startPositon = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        
        float newY = Mathf.Sin(Time.time * moveSpeed) * moveHeight + startPositon.y;
        transform.position = new Vector3(startPositon.x, newY, startPositon.z);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && isServer)
        {
            UpdateCurrencyForAllPlayers(currencyWorth);
            NetworkServer.Destroy(gameObject);
        }
    }
    [Server]
    private void UpdateCurrencyForAllPlayers(int amount)
    {
        var players = FindObjectsOfType<PlayerManager>();
        foreach (var player in players)
        {
            player.ServerAddCurrency(amount);
        }
    }
}
