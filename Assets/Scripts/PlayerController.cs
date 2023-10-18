using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Settings you can change in the inspector
    [SerializeField] private float _speed = 800f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    [SerializeField] private float _jumpForce = 15f;
    public float _mouseSensitivity = 5f;
    [SerializeField] private float _groundCheckRadius = 1.0f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _cameraTransform;
    //INPUT BOOLS
    private bool _isSprinting;
    private bool _isJumping;
    private bool _isGrounded;
    private bool _isDashing;

    //Using Unitys new input system
    PlayerInput _input;

    private Rigidbody _playerRigidbody;

    //Rotation - used for mouse input
    float _xRotation = 0f;

    bool _delay;
    public bool _shootDelay;
    public bool _reload;
    public bool _noAni;

    public int _ammo;

    private void Awake()
    {
        _input = new PlayerInput();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _playerRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //INPUT VALUES
        _isJumping = _input.Player.Jump.ReadValue<float>() > 0.1f;
        _isSprinting = _input.Player.Sprint.ReadValue<float>() > 0.1f;
        _isDashing = _input.Player.Dash.ReadValue<float>() > 0.1f;

        _isGrounded = Physics.SphereCast(transform.position, _groundCheckRadius, -Vector3.up, out RaycastHit hitInfo, 0.1f, _groundLayer);

        if(_isJumping && !_delay && _isGrounded)
        {
            StartCoroutine(InputDelay(0.15f));
            _delay = true;
            StartCoroutine(Jump());
        }

        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Screen.dpi * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Screen.dpi * Time.deltaTime;


        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void FixedUpdate()
    {
        float horizontal = _input.Player.Move.ReadValue<Vector2>().x;
        float vertical = _input.Player.Move.ReadValue<Vector2>().y;

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        movement = transform.TransformDirection(movement);
        movement.y = 0;

        if(_isSprinting)
        {
            movement *= _sprintMultiplier;
        }

        if(_isDashing && !_delay && movement.magnitude > 0)
        {
            StartCoroutine(InputDelay(3.0f));
            _delay = true;
            StartCoroutine(Dash(movement));
        }

        if(!_isGrounded)
        {
            movement.y -= 9.8f * Time.deltaTime;
        }

        _playerRigidbody.velocity = movement * _speed * Time.deltaTime;
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

    public IEnumerator ShootDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _shootDelay = false;
    }

    IEnumerator Dash(Vector3 soTrue)
    {
        for(int i = 20; i < 40; i++)
        {
            yield return new WaitForSeconds(0.01f);
            _playerRigidbody.AddForce(soTrue.normalized * i, ForceMode.Impulse);
        }
    }

    IEnumerator Jump()
    {
        for(int i = 5; i < _jumpForce; i++)
        {
            yield return new WaitForSeconds(0.005f);
            _playerRigidbody.AddForce(Vector3.up * i, ForceMode.Impulse);
        }
    }
}