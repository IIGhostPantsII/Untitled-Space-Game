using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;

public class PlayerController2 : MonoBehaviour
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(Globals.Movement)
        {
            //This code is for playing the footsteps sound, needed
            if(isMoving && timeSinceLastPlay >= (isFirstInput ? eventInterval / 2 : eventInterval))
            {
                if(Globals.Movement && isGrounded && !isCrouching)
                {
                    moveEvent.start();
                    //Lowpass Things
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

            //To stop the footsteps sound
            if(!isMoving)
            {
                FMOD.Studio.PLAYBACK_STATE playbackState;
                moveEvent.getPlaybackState(out playbackState);

                if(playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    moveEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
            }

            //When you land on the ground, you can change the way it works but where the oxygenNosprint is keep that the same
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
        }
    }

    void FixedUpdate()
    {
        //Scuffed as hell
        if(Globals.Movement)
        {
            //Make it so the player cant sprint, keep            
            if(Oxygen.NoSprint)
            {
                isSprinting = false;
            }
            //Make footsteps sound go faster, and make the oxygen deplete faster
            else if(isSprinting && !isCrouching && !Oxygen.NoSprint)
            {
                eventInterval = 0.3f;
                //movement *= _sprintMultiplier;
                _oxygen._depletionSpeed = 0.25f;
                bobbingSpeed = 10f;
                bobbingAmount = 0.2f;
            }
            else if(!isSprinting)
            {
                eventInterval = 0.6f;
                _oxygen._depletionSpeed = 0.1f;
                bobbingSpeed = 5f;
                bobbingAmount = 0.1f;
            }
        }
    }
}
