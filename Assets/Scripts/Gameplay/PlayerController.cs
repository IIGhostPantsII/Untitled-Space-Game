using System.Collections;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _groundCheckRadius = 1.0f;
    [SerializeField] private float _mouseSensitivity = 5f;
    [SerializeField] private float _jumpVelocity = 1;
    [SerializeField] private float _gravity = 0.5f;
    [SerializeField] private float _groundedGravity = 0.1f;
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
    [SerializeField] public GameObject _interactPrompt;
    [SerializeField] public PowerButton[] _powerButtons;

    // INPUT BOOLS
    private bool isSprinting;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isMoving;
    private bool isCrouching;

    // Input system
    private PlayerInput input;
    private InputAction _openMenu;

    // Other
    private bool delay;
    private bool isJumping;
    private bool holdingSprint;
    private bool jumpSound = false;
    private float holdDuration = 0f;
    private Vector3 movement;
    private CharacterController _charController;
    private Animator playerAnimator;
    private bool _canInteract;
    private PowerButton _currentButton;

    public bool is2D;
    private int _winCounter = 0;

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

        _charController = gameObject.GetComponent<CharacterController>();

        moveEvent = RuntimeManager.CreateInstance(moveEventPath);

        jumpEvent = RuntimeManager.CreateInstance(jumpEventPath);

        originalCameraPosition = new Vector3(0f, 2f, 0f);

        foreach (PowerButton button in _powerButtons)
        {
            button.OnActivate += IncrementWinCounter;
        }
        
        if(_firstPersonCam != null)
        {
            StartCoroutine(TurnOn(4.5f));
        }
    }

    void IncrementWinCounter()
    {
        _winCounter += 1;

        if (_winCounter >= _powerButtons.Length)
        {
            WinGame();
        }
    }

    void Update()
    {
        // INPUT VALUES
        jumpPressed = input.Player.Jump.ReadValue<float>() > 0.1f;
        isSprinting = input.Player.Sprint.ReadValue<float>() > 0.1f;
        isGrounded = Physics.SphereCast(transform.position, _groundCheckRadius, -Vector3.up, out RaycastHit hitInfo, 0.1f, _groundLayer);

        if(Globals.Movement)
        {
            Oxygen.PauseDepletion = false;
            
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Screen.dpi;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Screen.dpi;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            _firstPersonCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            if (input.Player.Interact.ReadValue<float>() > 0.1 && _currentButton != null)
            {
                _currentButton.FillBar();
                _interactPrompt.GetComponentInChildren<Image>().fillAmount = _currentButton.ButtonProgress;
                if (_currentButton.IsOn)
                {
                    _interactPrompt.GetComponentInChildren<Image>().fillAmount = 0;
                    _interactPrompt.SetActive(false);
                }
            } 
            else if (_currentButton != null)
            {
                
                _interactPrompt.GetComponentInChildren<Image>().fillAmount = _currentButton.ButtonProgress;
            }

            if (isMoving)
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

            if (isMoving && timeSinceLastPlay >= (isFirstInput ? eventInterval / 2 : eventInterval))
            {
                if(Globals.Movement && _charController.isGrounded && !isCrouching)
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
        } 
        else if (Globals.GameState == GameState.Victory)
        {
            LoadWinMenu();
        }

        if(Globals.GameState == GameState.Lost)
        {
            LoadGameOverMenu();
        }

        if (!Globals.Movement)
        {
            movement = new Vector3(0, movement.y, 0);
            return;
        }

        float grav = movement.y;
        Vector2 moveVector = input.Player.Move.ReadValue<Vector2>();

        movement = new Vector3(moveVector.x, movement.y, moveVector.y);
        movement = transform.TransformDirection(movement);
        
        if (Oxygen.NoSprint)
        {
            isSprinting = false;
        }
        else if (isSprinting && !isCrouching && !Oxygen.NoSprint)
        {
            eventInterval = 0.3f;
            movement *= _sprintMultiplier;
            _oxygen._depletionSpeed = _oxygen._sprintDepletionSpeed;
            bobbingSpeed = 10f;
            bobbingAmount = 0.2f;
        }
        else if (isCrouching)
        {
            eventInterval = 0.6f;
            _oxygen._depletionSpeed = _oxygen._crouchDepletionSpeed;
            bobbingSpeed = 3f;
            bobbingAmount = 0.2f;
        }
        else if (!isSprinting)
        {
            eventInterval = 0.6f;
            _oxygen._depletionSpeed = _oxygen._normalDepletionSpeed;
            bobbingSpeed = 5f;
            bobbingAmount = 0.1f;
        }

        if (isCrouching)
        {
            movement /= _sprintMultiplier;
            originalCameraPosition = Vector3.MoveTowards(originalCameraPosition, new Vector3(0f, 1f, 0f), Time.deltaTime * _crouchSpeed);
            //transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
        else
        {
            originalCameraPosition = Vector3.MoveTowards(originalCameraPosition, new Vector3(0f, 2f, 0f), Time.deltaTime * _crouchSpeed);
        }

        isMoving = moveVector != Vector2.zero;
        
        movement.y = grav;
        
        if (jumpPressed && !isJumping && _charController.isGrounded)
        {
            isJumping = true;
            movement.y = _jumpVelocity;
        } 
        else if (!jumpPressed && _charController.isGrounded)
        {
            isJumping = false;
        }
    }

    void LateUpdate()
    {
        _charController.Move(movement * (_speed * Time.deltaTime));

        HandleGravity();
        
        Debug.Log($@"The player <color=red>{_charController.isGrounded switch
        {
            true => "is",
            false => "is not"
        }}</color> grounded");
    }

    private void HandleGravity()
    {
        if (_charController.isGrounded)
        {
            movement.y = -_groundedGravity;
        }
        else
        {
            movement.y -= _gravity * Time.deltaTime;
        }
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
        if (!_charController.isGrounded) return;
        
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
        Globals.GameState = GameState.Main;
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
        if (Globals.Movement)
        {
            isCrouching = !isCrouching;
        }
    }

    private void HoldingSprint()
    {
        if (Globals.Movement)
        {
            holdingSprint = true;
        }
    }

    private void ResetSprintTimer()
    {
        if (Globals.Movement)
        {
            holdingSprint = false;
            holdDuration = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Button"))
        {
            if (!other.gameObject.GetComponent<PowerButton>().IsOn)
            {
                _interactPrompt.SetActive(true);
                _canInteract = true;
                _currentButton = other.gameObject.GetComponent<PowerButton>();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Button"))
        {
            _interactPrompt.GetComponentInChildren<Image>().fillAmount = 0;
            _interactPrompt.SetActive(false);
            _canInteract = false;
            _currentButton = null;
        }
    }
}
