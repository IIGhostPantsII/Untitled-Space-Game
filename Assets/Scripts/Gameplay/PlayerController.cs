using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    // FMOD Things
    public float eventInterval = 0.4f;
    private float timeSinceLastPlay = 0.0f;
    private bool isFirstInput = true;
    public FMODUnity.EventReference moveEventPath;
    public FMOD.Studio.EventInstance moveEvent;
    private FMOD.Studio.PLAYBACK_STATE playbackState;

    // Inspector Settings
    [SerializeField] private float _speed = 800f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _groundCheckRadius = 1.0f;
    [SerializeField] private float _mouseSensitivity = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Oxygen _oxygen;
    [SerializeField] private Oxygen _subOxygen;
    [SerializeField] public GameObject _astronaut;
    [SerializeField] public GameObject _firstPersonCam;

    // INPUT BOOLS
    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;
    private bool isMoving;
    private bool isCrouching;

    // Input system
    private PlayerInput input;

    // Other
    private bool delay;
    private bool jumpOver = true;
    private bool holdingSprint;
    private float holdDuration = 0f;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;

    public bool is2D;

    // Rotation - used for mouse input
    private float xRotation = 0f;

    // Camera Bobbing
    private float bobbingSpeed = 5f;
    private float bobbingAmount = 0.05f;
    private float bobbingTimer = 0f;
    private Vector3 originalCameraPosition;

    private void Awake()
    {
        input = new PlayerInput();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerRigidbody = gameObject.GetComponent<Rigidbody>();

        moveEvent = RuntimeManager.CreateInstance(moveEventPath);

        originalCameraPosition = new Vector3(0f, 2f, 0f);
        
        if(_firstPersonCam != null)
        {
            StartCoroutine(TurnOn(4.5f));
        }
    }

    void Update()
    {
        // INPUT VALUES
        isJumping = input.Player.Jump.ReadValue<float>() > 0.1f;
        isSprinting = input.Player.Sprint.ReadValue<float>() > 0.1f;
        isGrounded = Physics.SphereCast(transform.position, _groundCheckRadius, -Vector3.up, out RaycastHit hitInfo, 0.1f, _groundLayer);

        if(Globals.Movement)
        {

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Screen.dpi * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Screen.dpi * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            _firstPersonCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            if(isMoving)
            {
                bobbingTimer += Time.deltaTime * bobbingSpeed;
                float bobbingOffset = Mathf.Sin(bobbingTimer) * bobbingAmount;
                _firstPersonCam.transform.localPosition = originalCameraPosition + new Vector3(bobbingOffset / 1.5f, bobbingOffset, 0f);
                timeSinceLastPlay += Time.deltaTime;

                if(isFirstInput && timeSinceLastPlay >= eventInterval / 2)
                {
                    isFirstInput = false;
                }
            }
            else
            {
                timeSinceLastPlay = 0.0f;
                bobbingTimer = 0f;
                isFirstInput = true;
                _firstPersonCam.transform.localPosition = originalCameraPosition;
            }

            if(isMoving && timeSinceLastPlay >= (isFirstInput ? eventInterval / 2 : eventInterval))
            {
                if(Globals.Movement && isGrounded && !isCrouching)
                {
                    moveEvent.start();
                    if(Oxygen.NoSprint)
                    {
                        moveEvent.setParameterByName("Lowpass",(_subOxygen._oxygenMeter * 220));
                    }
                    else
                    {
                        moveEvent.setParameterByName("Lowpass",22000);
                    }
                    timeSinceLastPlay = 0.0f;
                }
            }

            if(!isMoving)
            {
                FMOD.Studio.PLAYBACK_STATE playbackState;
                moveEvent.getPlaybackState(out playbackState);

                if(playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    moveEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
            }

            if(holdingSprint)
            {
                holdDuration += Time.deltaTime;
                if (holdDuration >= 1f) // If hold duration is bigger then 1 second
                {
                    isCrouching = false;
                }
            }
        }
    }

    void FixedUpdate()
    {
        //Scuffed as hell
        if(Globals.Movement)
        {
            float horizontal = input.Player.Move.ReadValue<Vector2>().x;
            float vertical = input.Player.Move.ReadValue<Vector2>().y;

            Vector3 movement;
            if(is2D)
            {
                if (vertical != 0) movement = new Vector3(0, 0, vertical);
                else movement = new Vector3(0, 0, horizontal);
            }
            else movement = new Vector3(horizontal, 0, vertical);
            movement = transform.TransformDirection(movement);
            movement.y = 0;
            
            if(Oxygen.NoSprint)
            {
                isSprinting = false;
            }
            else if(isSprinting && !isCrouching && !Oxygen.NoSprint)
            {
                eventInterval = 0.3f;
                movement *= _sprintMultiplier;
                _oxygen._depletionSpeed = 0.25f;
                bobbingSpeed = 10f;
                bobbingAmount = 0.2f;
            }
            else if(isCrouching)
            {
                eventInterval = 0.6f;
                _oxygen._depletionSpeed = 0.05f;
                bobbingSpeed = 3f;
                bobbingAmount = 0.2f;
            }
            else if(!isSprinting)
            {
                eventInterval = 0.6f;
                _oxygen._depletionSpeed = 0.1f;
                bobbingSpeed = 5f;
                bobbingAmount = 0.1f;
            }

            if(isCrouching)
            {
                movement /= _sprintMultiplier;
                originalCameraPosition = Vector3.MoveTowards(originalCameraPosition, new Vector3(0f, 1f, 0f), Time.deltaTime * _crouchSpeed);
                //transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
            else
            {
                originalCameraPosition = Vector3.MoveTowards(originalCameraPosition, new Vector3(0f, 2f, 0f), Time.deltaTime * _crouchSpeed);
            }

            if(!isGrounded && jumpOver)
            {
                Vector3 gravityVector = -500f * Vector3.up; // Define the gravity vector
                playerRigidbody.AddForce(gravityVector, ForceMode.Acceleration);
            }

            if(Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }

            playerRigidbody.velocity = movement * _speed * Time.deltaTime;

            if(isJumping && !delay && isGrounded && Globals.Movement)
            {
                StartCoroutine(InputDelay(0.15f));
                delay = true;
                StartCoroutine(Jump());
            }
        }
    }

    private void OnEnable()
    {
        input.Enable();

        // Toggle crouch
        input.Player.Crouch.performed += ctx => ToggleCrouch();

        // If sprint is being held while crouching
        input.Player.Sprint.started += ctx => HoldingSprint();
        input.Player.Sprint.canceled += ctx => ResetSprintTimer();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    IEnumerator InputDelay(float seconds)
    {
        for(int i = 0; i < 10; i++)
        {
            originalCameraPosition = originalCameraPosition - new Vector3(0f, 0.15f, 0f);
            yield return new WaitForSeconds(seconds / 15f);
        }
        for(int j = 0; j < 5; j++)
        {
            originalCameraPosition = originalCameraPosition + new Vector3(0f, 0.3f, 0f);
            yield return new WaitForSeconds(seconds / 15f);
        }
        delay = false;
    }

    IEnumerator Jump()
    {
        float currentForce = 0f;
        float timeToReachMaxForce = 1.0f; // Adjust this value to control the speed of the jump

        while (currentForce < _jumpForce)
        {
            float forceStep = _jumpForce / timeToReachMaxForce * Time.deltaTime * 6.0f;
            currentForce = Mathf.Min(currentForce + forceStep, _jumpForce);

            // Adjusting force application to simulate a more dynamic jump
            float forceMultiplier = 1.0f - Mathf.Pow(1.0f - (currentForce / _jumpForce), 2);
            playerRigidbody.AddForce(Vector3.up * _jumpForce * forceMultiplier, ForceMode.Impulse);

            yield return null; // Wait for the next frame
        }

        jumpOver = false;

        yield return new WaitForSeconds(0.25f);

        jumpOver = true;
    }

    IEnumerator TurnOn(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _firstPersonCam.SetActive(true);
        _astronaut.SetActive(false);
    }

    private void ToggleCrouch()
    {
        if(Globals.Movement)
        {
            isCrouching = !isCrouching;
        }
    }

    private void HoldingSprint()
    {
        if(Globals.Movement)
        {
            holdingSprint = true;
        }
    }

    private void ResetSprintTimer()
    {
        if(Globals.Movement)
        {
            holdingSprint = false;
            holdDuration = 0f;
        }
    }
}
