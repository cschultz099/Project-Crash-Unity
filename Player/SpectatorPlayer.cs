using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class SpectatorPlayer : MonoBehaviour
{
    // Spectator Settings
    [Header("Specator Settings")]
    public float moveSpeed = 10.0f;
    public float turnSpeed = 3.0f;

    // Internal State (Not exposed in Inspector)
    private Player inputSystem;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start()
    {
        inputSystem = ReInput.players.GetPlayer(0);
    }

    void Update()
    {
        // Camera Rotation
        if(Input.GetMouseButton(1))
        {
            yaw += turnSpeed * inputSystem.GetAxis("Look X Axis");
            pitch -= turnSpeed * inputSystem.GetAxis("Look Y Axis");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Camera Movement
        float xAxis = inputSystem.GetAxis("Move Horizontal") * moveSpeed * Time.deltaTime;
        float zAxis = inputSystem.GetAxis("Move Vertical") * moveSpeed * Time.deltaTime;

        // Adjust speed with Left Shift
        if(Input.GetKey(KeyCode.LeftShift))
        {
            xAxis *= 2;
            zAxis *= 2;
        }

        // Move the camera
        transform.Translate(xAxis, 0, zAxis);

        // Elevation Control Keyboard
        if(inputSystem.GetButton("Elevate Down"))
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
        }
        if (inputSystem.GetButton("Elevate Up"))
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
        }

        // Elevation Control Controller
        float elevationInput = inputSystem.GetAxis("Elevation Control");
        transform.Translate(Vector3.up * elevationInput * moveSpeed * Time.deltaTime, Space.World);
    }
}
