using System.Collections;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // FMOD Things
    public float eventInterval = 0.4f;
    private float timeSinceLastPlay = 0.0f;
    private bool isFirstInput = true;
    public FMODUnity.EventReference moveEventPath;
    public FMODUnity.EventReference jumpEventPath;
    public FMOD.Studio.EventInstance moveEvent;
    public FMOD.Studio.EventInstance jumpEvent;
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
    [SerializeField] public GameObject _defaultUI;
    [SerializeField] public GameObject _pauseMenu;
    [SerializeField] public GameObject _gameOverScreen;
    [SerializeField] public GameObject _victoryScreen;

    // INPUT BOOLS
    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;
    private bool isMoving;
    private bool isCrouching;

    // Input system
    private PlayerInput input;
    private InputAction _openMenu;

    // Other
    private bool delay;
    private bool jumpOver = true;
    private bool holdingSprint;
    private bool jumpSound = false;
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
        
        input.Player.Pause.performed += PauseGame;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerRigidbody = gameObject.GetComponent<Rigidbody>();

        moveEvent = RuntimeManager.CreateInstance(moveEventPath);

        jumpEvent = RuntimeManager.CreateInstance(jumpEventPath);

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
        Debug.Log(isGrounded);

        if(Globals.Movement)
        {
            Oxygen.PauseDepletion = false;
            
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Screen.dpi;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Screen.dpi;

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

            if(isGrounded && jumpSound)
            {
                jumpSound = false;
                jumpEvent.start();
                Debug.Log("Hi!");
                if(Oxygen.NoSprint)
                {
                    jumpEvent.setParameterByName("Lowpass",(_subOxygen._oxygenMeter * 220));
                }
                else
                {
                    jumpEvent.setParameterByName("Lowpass",22000);
                }
            }
            else if(!isGrounded)
            {
                jumpSound = true;
            }
        }
        else
        {
            
            Oxygen.PauseDepletion = true;
        }

        if (_subOxygen._oxygenMeter <= 0)
        {
            Globals.GameState = GameState.Lost;
            LoadGameOverMenu();
        } else if (Globals.GameState == GameState.Victory)
        {
            LoadWinMenu();
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;
        
        //Scuffed as hell
        if(Globals.Movement)
        {
            float horizontal = input.Player.Move.ReadValue<Vector2>().x;
            float vertical = input.Player.Move.ReadValue<Vector2>().y;

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
                _oxygen._depletionSpeed = _oxygen._sprintDepletionSpeed;
                bobbingSpeed = 10f;
                bobbingAmount = 0.2f;
            }
            else if(isCrouching)
            {
                eventInterval = 0.6f;
                _oxygen._depletionSpeed = _oxygen._crouchDepletionSpeed;
                bobbingSpeed = 3f;
                bobbingAmount = 0.2f;
            }
            else if(!isSprinting)
            {
                eventInterval = 0.6f;
                _oxygen._depletionSpeed = _oxygen._normalDepletionSpeed;
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

            if(isJumping && !delay && isGrounded && Globals.Movement)
            {
                StartCoroutine(InputDelay(0.15f));
                delay = true;
                StartCoroutine(Jump());
            }
        }
        
        playerRigidbody.velocity = movement * _speed * Time.deltaTime;
    }

    [Button()]
    private void WinGame()
    {
        Globals.GameState = GameState.Victory;
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

    private void LoadGameOverMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Globals.LockMovement();
        
        _defaultUI.SetActive(false);
        _pauseMenu.SetActive(false);
        _gameOverScreen.SetActive(true);
    }
    
    private void LoadWinMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Globals.LockMovement();
        
        _defaultUI.SetActive(false);
        _pauseMenu.SetActive(false);
        _victoryScreen.SetActive(true);
    }
    
    private void PauseGame(InputAction.CallbackContext obj)
    {
        EventSystem.current.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Globals.LockMovement();
        
        _defaultUI.SetActive(false);
        _pauseMenu.SetActive(true);
    }

    public void UnpauseGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Globals.UnlockMovement();
        
        _defaultUI.SetActive(true);
        _pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                     Application.Quit();
        #endif
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
