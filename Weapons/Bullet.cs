using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 20f;

    void Update()
    {
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        NetworkServer.Destroy(gameObject);
    }
}
