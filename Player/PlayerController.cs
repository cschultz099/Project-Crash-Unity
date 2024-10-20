using UnityEngine;
using Mirror;
using System.Collections;
using Rewired;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : NetworkBehaviour
{
    // Movement Settings
    [Header("Movement Settings")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    // Camera Settings
    [Header("Camera Settings")]
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    // Internal State (Not exposed in Inspector)
    private PlayerManager playerManager;
    private float mobilityModifier;
    private float originalRunningSpeed;
    private float originalGravity;
    private Player inputSystem;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    // Player Control Flags (Hidden in Inspector but accessible by other scripts)
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canRotate = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerManager = GetComponentInParent<PlayerManager>();
        inputSystem = ReInput.players.GetPlayer(0);

        originalRunningSpeed = runningSpeed;
        originalGravity = gravity;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(false);
        }
        else if (isLocalPlayer)
        {
            mobilityModifier = playerManager.MobilityStat();
            runningSpeed += runningSpeed * mobilityModifier;
            walkingSpeed += walkingSpeed * mobilityModifier;
            jumpSpeed += jumpSpeed * mobilityModifier;
        }
    }

    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = -transform.TransformDirection(Vector3.forward);
        Vector3 right = -transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = inputSystem.GetButton("Run");
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * inputSystem.GetAxis("Move Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * inputSystem.GetAxis("Move Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (inputSystem.GetButtonDown("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && canRotate)
        {
            rotationX += -inputSystem.GetAxis("Look Y Axis") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 180, 0);
            transform.rotation *= Quaternion.Euler(0, inputSystem.GetAxis("Look X Axis") * lookSpeed, 0);
        }
    }

    #region Player Buffs/Modifiers
    [ClientRpc]
    public void RpcIncreaseRunningSpeed(float speedIncrease, float powerUpDurration)
    {
        StartCoroutine(SpeedBuff(speedIncrease, powerUpDurration));
    }

    public IEnumerator SpeedBuff(float speedIncrease, float powerUpDurration)
    {
        runningSpeed += speedIncrease;
        yield return new WaitForSeconds(powerUpDurration);
        runningSpeed = originalRunningSpeed;
    }
    [ClientRpc]
    public void RpcIncreaseJumpHeight(float increase, float powerUpDurration)
    {
        StartCoroutine(JumpBuff(increase, powerUpDurration));
    }

    public IEnumerator JumpBuff(float increase, float powerUpDurration)
    {
        gravity -= increase;
        yield return new WaitForSeconds(powerUpDurration);
        gravity = originalGravity;
    }

    public void PerformDash(float dashSpeed, float dashDuration)
    {
        StartCoroutine(Dash(dashSpeed, dashDuration));
    }

    private IEnumerator Dash(float dashSpeed, float dashDuration)
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            characterController.Move(-transform.forward * dashSpeed * Time.deltaTime);
            yield return null;
        }
    }
    #endregion
}