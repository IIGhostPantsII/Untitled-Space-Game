using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class PlayerController : MonoBehaviour
{
    public FMODUnity.EventReference moveEventPath;
    public FMOD.Studio.EventInstance moveEvent;
    private FMOD.Studio.PLAYBACK_STATE playbackState;
    private float timeSinceLastPlay = 0.0f;
    public float eventInterval = 0.4f;
    private bool isEventPlaying = false;
    private bool isFirstInput = true;

    // Inspector Settings
    [SerializeField] private float _speed = 800f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _groundCheckRadius = 1.0f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _mouseSensitivity = 5f;
    [SerializeField] private Oxygen _oxygen;

    // INPUT BOOLS
    private bool _isSprinting;
    private bool _isJumping;
    private bool _isGrounded;

    // input system
    PlayerInput _input;

    private Rigidbody _playerRigidbody;

    bool _delay;

    private bool isMoving = false;

    //Rotation - used for mouse input
    float _xRotation = 0f;

    private void Awake()
    {
        _input = new PlayerInput();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _playerRigidbody = GetComponent<Rigidbody>();
        moveEvent = RuntimeManager.CreateInstance(moveEventPath);
    }

    void Update()
    {
        // INPUT VALUES
        _isJumping = _input.Player.Jump.ReadValue<float>() > 0.1f;
        _isSprinting = _input.Player.Sprint.ReadValue<float>() > 0.1f;

        if(Globals.Movement)
        {
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Screen.dpi * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Screen.dpi * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        _isGrounded = Physics.SphereCast(transform.position, _groundCheckRadius, -Vector3.up, out RaycastHit hitInfo, 0.1f, _groundLayer);

        if(_isJumping && !_delay && _isGrounded)
        {
            StartCoroutine(InputDelay(0.15f));
            _delay = true;
            StartCoroutine(Jump());
        }

        // Track time since last event played
        if(isMoving)
        {
            timeSinceLastPlay += Time.deltaTime;

            if(isFirstInput && timeSinceLastPlay >= eventInterval / 2)
            {
                isFirstInput = false;
            }
        }
        else
        {
            timeSinceLastPlay = 0.0f;
            isFirstInput = true;
        }

        if(isMoving && timeSinceLastPlay >= (isFirstInput ? eventInterval / 2 : eventInterval))
        {
            if(Globals.Movement)
            {
                moveEvent.start();
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
    }

    void FixedUpdate()
    {
        float horizontal = _input.Player.Move.ReadValue<Vector2>().x;
        float vertical = _input.Player.Move.ReadValue<Vector2>().y;

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        movement = transform.TransformDirection(movement);
        movement.y = 0;

        if(_isSprinting && !Oxygen.NoSprint)
        {
            eventInterval = 0.3f;
            movement *= _sprintMultiplier;
            _oxygen._depletionSpeed = 2.5f;
        }
        if(!_isSprinting)
        {
            eventInterval = 0.6f;
            _oxygen._depletionSpeed = 1.0f;
        }

        if(!_isGrounded)
        {
            movement.y -= 9.8f * Time.deltaTime;
        }

        if(Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if(Globals.Movement)
        {
            _playerRigidbody.velocity = movement * _speed * Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    IEnumerator InputDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _delay = false;
    }

    IEnumerator Jump()
    {
        for (int i = 5; i < _jumpForce; i++)
        {
            yield return new WaitForSeconds(0.005f);
            if(Globals.Movement)
            {
                _playerRigidbody.AddForce(Vector3.up * i, ForceMode.Impulse);
            }
                
        }
    }
}
