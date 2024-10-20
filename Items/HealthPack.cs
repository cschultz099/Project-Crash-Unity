using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : NetworkBehaviour
{
    public int healthHealed = 20;
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
        if (other.CompareTag("Player") && isServer)
        {
            other.GetComponentInParent<PlayerManager>().Heal(healthHealed);
            NetworkServer.Destroy(gameObject);
        }
    }
}
