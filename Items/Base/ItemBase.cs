using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : NetworkBehaviour
{
    public float rotationSpeed = 60f;
    public float moveSpeed = 2f;
    public float moveHeight = 0.07f;

    private Vector3 startPositon;

    protected virtual void Start()
    {
        startPositon = transform.position;
    }

    protected virtual void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float newY = Mathf.Sin(Time.time * moveSpeed) * moveHeight + startPositon.y;
        transform.position = new Vector3(startPositon.x, newY, startPositon.z);
    }
    protected virtual void OnTriggerEnter(Collider other)
    {

    }
}