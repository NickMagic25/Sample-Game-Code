/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------



    Anything that I (Nick) add to this script will be 
    documetned in this script and in version control
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : Damageable {

    private float NORMAL_FOV;
    private float HOOKSHOT_FOV;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private Transform hookshotTransform;
    // layermask to see if what is raycast to is grappleable
    [SerializeField] private LayerMask grappleable;

    


    private CharacterController characterController;
    private float cameraVerticalAngle;
    private float characterVelocityY;
    private Vector3 characterVelocityMomentum;
    private Camera playerCamera;
    private CameraFov cameraFov;
    private State state;
    private Vector3 hookshotPosition;
    private float hookshotSize;
    private bool jumpUsed;
    private bool secondJumpReady;
    private bool hookshotUsed;


    /* 
     New state sprinting
     */
    private enum State {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer,
        Sprinting,
    }

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        cameraFov = playerCamera.GetComponent<CameraFov>();
        Cursor.lockState = CursorLockMode.Locked;
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
        jumpUsed = false;
        secondJumpReady = false;
        hookshotUsed = false;
        currentHealth = maxHealth;
        NORMAL_FOV = playerCamera.fieldOfView;
        HOOKSHOT_FOV = NORMAL_FOV * 1.2f;
    }

    private void Update() {
        switch (state) {
            default:
            case State.Normal:
                HandleCharacterLook();
                HandleCharacterMovement();
                HandleHookshotStart();
                break;
            case State.HookshotThrown:
                HandleHookshotThrow();
                HandleCharacterLook();
                HandleCharacterMovement();
                break;
            case State.HookshotFlyingPlayer:
                HandleCharacterLook();
                HandleHookshotMovement();
                break;

            /* 
             when sprinting player can only look around and move
             */
            case State.Sprinting:
                HandleCharacterLook();
                HandleCharacterMovement();
                break;
        }

    }

    private void HandleCharacterLook() {
        float lookX = Input.GetAxisRaw("Mouse X");
        float lookY = Input.GetAxisRaw("Mouse Y");

        // Rotate the transform with the input speed around its local Y axis
        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        // Add vertical inputs to the camera's vertical angle
        cameraVerticalAngle -= lookY * mouseSensitivity;

        // Limit the camera's vertical angle to min/max
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        // Apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }

    /* 
     Changes from Code Monkey Original:
        -Double Jump
        -Sprinting
     
     */
    private void HandleCharacterMovement() {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");


        float moveSpeed = 20f;
        float sprintMultiplier = 1f;


        /* 
         can only enter sprinting state if sprint is held down, player
         currently in normal state, and is grounded changes FOV slightly
         */
        if (Input.GetButton("Sprint") && state == State.Normal && characterController.isGrounded)
        {
            sprintMultiplier = 2.5f;
            cameraFov.SetCameraFov(NORMAL_FOV+10f);
            state = State.Sprinting;
        }

        /* 
         returns to normal state if sprint is let go and state is sprinting
         or if the character is not grounded and state is sprinting
         */
        else if ((!Input.GetButton("Sprint") && state == State.Sprinting) || (!characterController.isGrounded && state == State.Sprinting))
        {
            cameraFov.SetCameraFov(NORMAL_FOV);
            state = State.Normal;
        }


        Vector3 characterVelocity = transform.right * moveX * moveSpeed * sprintMultiplier + transform.forward * moveZ * moveSpeed * sprintMultiplier;


        float primeJumpSpeed = 30f;
        float secondJumpSpeed = 35f;

        // primary jump
        if (characterController.isGrounded)
        {
            jumpUsed = false;
            secondJumpReady=false;
            hookshotUsed = false;
            characterVelocityY = 0f;
            // Jump
            if (TestInputJump())
            {
                jump(primeJumpSpeed);
                jumpUsed = true;
                secondJumpReady = true;
            }
        }
        else if (secondJumpReady && jumpUsed)
        {
            if (TestInputJump())
            {
                jump(secondJumpSpeed);
                secondJumpReady = false;
            }
        }

        else if (characterVelocity.y <= 0 && jumpUsed == false) 
        {
            if (TestInputJump()) 
            {
                jump(secondJumpSpeed);
                jumpUsed = true;    
            }

        }

        // Apply gravity to the velocity
        float gravityDownForce = -60f;
        characterVelocityY += gravityDownForce * Time.deltaTime;


        // Apply Y velocity to move vector
        characterVelocity.y = characterVelocityY;

        // Apply momentum
        characterVelocity += characterVelocityMomentum;

        // Move Character Controller
        characterController.Move(characterVelocity * Time.deltaTime);

        // Dampen momentum
        if (characterVelocityMomentum.magnitude > 0f) {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if (characterVelocityMomentum.magnitude < .0f) {
                characterVelocityMomentum = Vector3.zero;
            }
        }
    }

    private void jump(float jumpSpeed) 
    {
        characterVelocityY = jumpSpeed;
    }

    private void ResetGravityEffect() {
        characterVelocityY = 0f;
    }

    /*
     max distance added, as well as a way to see if the opject is able to be grappled to
     */
    private void HandleHookshotStart() {

        float maxDistance = 150f;

        if (TestInputDownHookshot() && !hookshotUsed) {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, maxDistance, grappleable)) {
                // Hit something
                hookshotPosition = raycastHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                state = State.HookshotThrown;
                hookshotUsed = true;
            }
        }
    }

    private void HandleHookshotThrow() {
        hookshotTransform.LookAt(hookshotPosition);

        float hookshotThrowSpeed = 200f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookshotPosition)) {
            state = State.HookshotFlyingPlayer;
            cameraFov.SetCameraFov(HOOKSHOT_FOV);
        }
    }


    /*
     fixed a bug in code monkey's project that did not shorten
     the hookshot length
     
     */
    private void HandleHookshotMovement() {
        jumpUsed = true;
        hookshotTransform.LookAt(hookshotPosition);

        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 20f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 5f;

        // Move Character Controller
        characterController.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        // handle size of hookshot

        hookshotSize -= hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        float reachedHookshotPositionDistance = 1f;
        if (Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPositionDistance) {
            // Reached Hookshot Position
            StopHookshot(1f,hookshotDir,hookshotSpeed);
        }

        if (TestInputDownHookshot()) {
            // Cancel Hookshot
            StopHookshot(2f,hookshotDir,hookshotSpeed); //new method by me
        }

        if (TestInputJump()) {
            // Cancelled with Jump
            StopHookshot(7f, hookshotDir, hookshotSpeed); //new method by me
        }
    }

    private bool TestInputDownHookshot() {
        return Input.GetKeyDown(KeyCode.F);
    }

    private bool TestInputJump() {
        return Input.GetKeyDown(KeyCode.Space);
    }


    /* edited the StopHookshot function from codemonkey so 
       that it takes in a momentum speed so you don't need
       just jump in order to have momentum when stopping
    */
    private void StopHookshot(float momentumExtraSpeed, Vector3 hookshotDir,float hookshotSpeed)
    {
        characterVelocityMomentum = hookshotDir * hookshotSpeed * momentumExtraSpeed;
        float jumpSpeed = 40f;
        characterVelocityMomentum += Vector3.up * jumpSpeed;
        state = State.Normal;
        ResetGravityEffect();
        hookshotTransform.gameObject.SetActive(false);
        cameraFov.SetCameraFov(NORMAL_FOV);
    }

    public override void Death() 
    { 

    }


}
