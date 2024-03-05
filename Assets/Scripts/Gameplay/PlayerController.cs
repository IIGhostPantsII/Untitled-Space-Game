using System.Collections;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // FMOD Things
    public float eventInterval = 0.4f;
    private float timeSinceLastPlay = 0.0f;
    private bool isFirstInput = true;
    public FMODUnity.EventReference moveEventPath;
    public FMODUnity.EventReference jumpStartEventPath;
    public FMODUnity.EventReference jumpEndEventPath;
    public FMOD.Studio.EventInstance moveEvent;
    public FMOD.Studio.EventInstance jumpStartEvent;
    public FMOD.Studio.EventInstance jumpEndEvent;
    private FMOD.Studio.PLAYBACK_STATE playbackState;

    // Inspector Settings
    [SerializeField] private float _speed = 800f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _groundCheckRadius = 1.0f;
    [SerializeField] private float _mouseSensitivity = 5f;
    [SerializeField] private float _jumpVelocity = 1;
    [SerializeField] private float _jumpVelocityLowGrav = 1;
    [SerializeField] private float _gravity = 0.5f;
    [SerializeField] private float _gravityLowGrav = 0.5f;
    [SerializeField] private float _groundedGravity = 0.1f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Oxygen _oxygen;
    [SerializeField] private Oxygen _subOxygen;
    [SerializeField] public GameObject _astronaut;
    [SerializeField] public GameObject _firstPersonCam;
    [SerializeField] private GameObject _defaultUI;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _gameOverScreen;
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private GameObject _interactPrompt;
    [SerializeField] private GameObject _interactPromptDoor;
    [SerializeField] private GameObject _camObject;
    [SerializeField] private SoundPerception _monsterHearing;
    [SerializeField] private PowerButton[] _powerButtons;

    // INPUT BOOLS
    private bool isSprinting;
    private bool jumpPressed;
    private bool isMoving;
    private bool isCrouching;

    // Input system
    private PlayerInput input;
    private InputAction openMenu;

    // Other
    private bool delay;
    private bool isInLowGravity;
    private bool isJumping;
    private bool holdingSprint;
    private bool jumpSound = false;
    private float holdDuration = 0f;
    private Vector3 movement;
    private CharacterController charController;
    private Animator playerAnimator;
    private bool canInteract;
    private PowerButton currentButton;
    private AreaTriggers areaTrigger;

    public bool is2D;
    private int winCounter = 0;
    private bool _ladderMode;

    // Rotation - used for mouse input
    private float xRotation = 0f;

    // Camera Bobbing
    private float bobbingSpeed = 5f;
    private float bobbingAmount = 0.05f;
    private float bobbingTimer = 0f;
    private Vector3 originalCameraPosition;

    //Cinematic Attack stuff
    private CinemachineVirtualCamera cam;
    private CinemachineTransposer transposer;
    private float attackTimer = 0f;

    private void Awake()
    {
        input = new PlayerInput();
        
        input.Player.Pause.performed += PauseGame;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController = gameObject.GetComponent<CharacterController>();

        moveEvent = RuntimeManager.CreateInstance(moveEventPath);
        jumpStartEvent = RuntimeManager.CreateInstance(jumpStartEventPath);
        jumpEndEvent = RuntimeManager.CreateInstance(jumpEndEventPath);

        originalCameraPosition = new Vector3(0f, 2f, 0f);

        //foreach(PowerButton button in _powerButtons)
        //{
        //    button.OnActivate += IncrementWinCounter;
        //}
        
        if(_firstPersonCam != null)
        {
            StartCoroutine(TurnOn(4.5f));
        }
        else
        {
            Debug.Log("Broken for some reason");
        }

        cam = _camObject.GetComponent<CinemachineVirtualCamera>();
        transposer = cam.GetCinemachineComponent<CinemachineTransposer>();
    }

    void IncrementWinCounter()
    {
        winCounter += 1;

        if(winCounter >= _powerButtons.Length)
        {
            WinGame();
        }
    }

    void Update()
    {
        // INPUT VALUES
        jumpPressed = input.Player.Jump.ReadValue<float>() > 0.1f;
        isSprinting = input.Player.Sprint.ReadValue<float>() > 0.1f;

        if(Globals.Movement)
        {
            Oxygen.PauseDepletion = false;
            
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Screen.dpi;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Screen.dpi;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            _firstPersonCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            if(input.Player.Interact.ReadValue<float>() > 0.1 && currentButton != null)
            {
                currentButton.FillBar();
                _interactPrompt.GetComponentInChildren<Image>().fillAmount = currentButton.ButtonProgress;
                if(currentButton.IsOn)
                {
                    _interactPrompt.GetComponentInChildren<Image>().fillAmount = 0;
                    _interactPrompt.SetActive(false);
                }
            } 
            else if(currentButton != null)
            {
                
                _interactPrompt.GetComponentInChildren<Image>().fillAmount = currentButton.ButtonProgress;
            }

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
                if(Globals.Movement && charController.isGrounded && !isCrouching)
                {
                    moveEvent.start();
                    if(isSprinting)
                    {
                        _monsterHearing.ActionPerformed(1.1f);
                    }
                    else
                    {
                        _monsterHearing.ActionPerformed(1f);
                    }
                    Globals.SpatialSounds(moveEvent, gameObject);
                    Globals.CheckLowpass(moveEvent, _subOxygen);
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
                if(holdDuration >= 1f) // If hold duration is bigger then 1 second
                {
                    isCrouching = false;
                }
            }
        }
        else
        {
            
            Oxygen.PauseDepletion = true;
        }

        if(_subOxygen._oxygenMeter <= 0)
        {
            Globals.GameState = GameState.Lost;
            LoadGameOverMenu();
        } 
        else if(Globals.GameState == GameState.Victory)
        {
            LoadWinMenu();
        }

        if(Globals.GameState == GameState.Lost)
        {
            LoadGameOverMenu();
        }

        if(!Globals.Movement)
        {
            movement = new Vector3(0, movement.y, 0);
            return;
        }

        float grav = movement.y;
        Vector2 moveVector = input.Player.Move.ReadValue<Vector2>();

        if (!_ladderMode) movement = new Vector3(moveVector.x, movement.y, moveVector.y); 
        else movement = new Vector3(moveVector.x, moveVector.y, 0);
        
        movement = transform.TransformDirection(movement);
        
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

        isMoving = moveVector != Vector2.zero;
        
        if (_ladderMode) return;

        movement.y = grav;
        
        if(jumpPressed && !isJumping && charController.isGrounded)
        {
            isJumping = true;
            movement.y = isInLowGravity ? _jumpVelocityLowGrav : _jumpVelocity;
            jumpStartEvent.start();
            _monsterHearing.ActionPerformed(1.15f);
            Globals.SpatialSounds(jumpStartEvent, gameObject);
            Globals.CheckLowpass(jumpStartEvent, _subOxygen);
            jumpSound = true;
        } 
        else if(!jumpPressed && charController.isGrounded)
        {
            isJumping = false;
            if(jumpSound)
            {
                jumpEndEvent.start();
                _monsterHearing.ActionPerformed(1.3f);
                Globals.SpatialSounds(jumpEndEvent, gameObject);
                Globals.CheckLowpass(jumpEndEvent, _subOxygen);
                jumpSound = false;
            }
        }

        //Cinemachine Attack funcion
        //if(true)
        //{
        //    attackTimer += Time.deltaTime;
        //    Vector3 originalPos = transposer.m_FollowOffset;
        //    if(attackTimer < 2.5f)
        //    {
        //        transposer.m_FollowOffset = new Vector3(originalPos.x, originalPos.y, originalPos.z);
        //    }
        //    else if(attackTimer > 2.5f && attackTimer < 4.5f)
        //    {
        //        transposer.m_FollowOffset = Vector3.Lerp(originalPos, new Vector3(originalPos.x - 4f, originalPos.y - 3f, originalPos.z + 2f), 2f);
        //    }
        //    else if(attackTimer > 4.5f && attackTimer < 6.5f)
        //    {
        //        transposer.m_FollowOffset = Vector3.Lerp(new Vector3(originalPos.x - 4f, originalPos.y - 3f, originalPos.z + 2f), new Vector3(originalPos.x - 8f, originalPos.y, originalPos.z), 2f);
        //    }
        //}
        //else
        //{
        //    attackTimer = 0f;
        //}
    }

    void LateUpdate()
    {
        charController.Move(movement * (_speed * Time.deltaTime));

        HandleGravity();
    }

    private void HandleGravity()
    {
        if (_ladderMode) return;
        
        float gravity = isInLowGravity ? _gravityLowGrav : _gravity;

        if(charController.isGrounded)
        {
            movement.y = -_groundedGravity;
        }
        else
        {
            movement.y -= gravity * Time.deltaTime;
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
        if(!charController.isGrounded) return;
        
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
        MonsterAI.Reset();
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

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Button":
                if (other.gameObject.GetComponent<PowerButton>()._buttonType == ButtonType.Door)
                {
                    if (other.gameObject.GetComponent<PowerButton>()._isDoorOn)
                    {
                        other.gameObject.GetComponent<PowerButton>()._text.SetText("Close Door");
                    }
                    else
                    {
                        other.gameObject.GetComponent<PowerButton>()._text.SetText("Open Door");
                    }

                    _interactPromptDoor.SetActive(true);
                    canInteract = true;
                    currentButton = other.gameObject.GetComponent<PowerButton>();
                }
                else if (!other.gameObject.GetComponent<PowerButton>().IsOn)
                {
                    _interactPrompt.SetActive(true);
                    canInteract = true;
                    currentButton = other.gameObject.GetComponent<PowerButton>();
                }
                break;
            case "AreaTrigger":
                areaTrigger = other.gameObject.GetComponent<AreaTriggers>();
                break;
            case "LowGravTrigger":
                isInLowGravity = true;
                break;
            case "Ladder":
                _ladderMode = true;
                break;
        }
    }

    //So the footstep sounds changes
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            moveEvent.setParameterByName("FootstepType", 0);
        }
        else if(hit.collider.gameObject.layer == LayerMask.NameToLayer("TiledGround"))
        {
            moveEvent.setParameterByName("FootstepType", 1);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Button"))
        {
            _interactPrompt.GetComponentInChildren<Image>().fillAmount = 0;
            _interactPrompt.SetActive(false);
            _interactPromptDoor.SetActive(false);
            canInteract = false;
            currentButton = null;
        }
        else if (other.gameObject.CompareTag("LowGravTrigger"))
        {
            isInLowGravity = false;
        } 
        else if (other.gameObject.CompareTag("Ladder"))
        {
            _ladderMode = false;
        }
    }

    public void OpenFeedbackForm()
    {
        Application.OpenURL("https://forms.gle/j7rSG9gnh5Kt5FGYA");
    }
}
