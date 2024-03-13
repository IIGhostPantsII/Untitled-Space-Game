using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    public FMODUnity.EventReference moveEventPathOpen;
    public FMOD.Studio.EventInstance moveEventOpen;

    public FMODUnity.EventReference moveEventPathClose;
    public FMOD.Studio.EventInstance moveEventClose;

    public float doorSpeed = 2.0f;
    public GameObject doorObj;
    [ReadOnly] public float maxYPosition = 7.5f;
    [ReadOnly] public float lowYPosition = -1.1f;

    private float timer = 0.0f;
    private float doorDuration;
    private PlayerController _player;
 
    [HideInInspector] public bool isInsideTrigger = false;
    [HideInInspector] public bool isOutsideTrigger = false;

    [SerializeField] [ColorUsageAttribute(true, true)]
    private Color UnlockedColor;
    [SerializeField] [ColorUsageAttribute(true, true)]
    private Color LockedColor;
    
    [SerializeField] private Light[] _lights;
    [SerializeField] private MeshRenderer[] _lightObjects;

    [SerializeField] public bool IsLocked = false;

    private bool LockedLights = false;
    private bool doorState = true;

    public static bool PauseMovement;

    void Start()
    {
        doorDuration = Random.Range(2f, 3f);
        moveEventOpen = RuntimeManager.CreateInstance(moveEventPathOpen);
        moveEventClose = RuntimeManager.CreateInstance(moveEventPathClose);
        maxYPosition = doorObj.transform.position.y;
        lowYPosition -= maxYPosition;
        _player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if(isOutsideTrigger || IsLocked)
        {
            MoveDoorUp();
        }
        else if(isInsideTrigger)
        {
            MoveDoorDown();
        }

        UpdateLockedLights();

        Globals.SpatialSounds(moveEventOpen, gameObject);
        Globals.SpatialSounds(moveEventClose, gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && doorState || other.CompareTag("Monster") && doorState)
        {
            isInsideTrigger = true;
            isOutsideTrigger = false; // Reset the flag when inside the trigger
            if (!IsLocked)
            {
                moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                moveEventOpen.start();
            }

            Globals.CheckLowpass(moveEventOpen, _player._subOxygen);
        }
    }

    private void UpdateLockedLights()
    {
        if (IsLocked && !LockedLights)
        {
            LockedLights = true;

            foreach (Light light in _lights)
            {
                light.color = LockedColor;
            }

            foreach (MeshRenderer renderer in _lightObjects)
            {
                renderer.material.SetColor("_EmissionColor", LockedColor);
            }
        } 
        else if (!IsLocked && LockedLights)
        {
            LockedLights = false;
            
            foreach (Light light in _lights)
            {
                light.color = UnlockedColor;
            }
            foreach (MeshRenderer renderer in _lightObjects)
            {
                renderer.material.SetColor("_EmissionColor", UnlockedColor);
            }
            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Monster") && !doorState)
        {
            NavMeshAgent monsterPathing = other.gameObject.GetComponent<NavMeshAgent>();
            timer += Time.deltaTime;
            Globals.LockMonsterMovement();
            monsterPathing.SetDestination(other.gameObject.transform.position);

            if(timer > doorDuration)
            {
                Globals.UnlockMonsterMovement();
                doorDuration = Random.Range(2f, 3f);
                timer = 0;
                doorState = true;
                isInsideTrigger = true;
                isOutsideTrigger = false;
                if (!IsLocked)
                {
                    moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    moveEventOpen.start();
                }

                Globals.CheckLowpass(moveEventOpen, _player._subOxygen);
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && doorState || other.CompareTag("Monster") && doorState)
        {
            isInsideTrigger = false;
            isOutsideTrigger = true;
            StartCoroutine(Close());
        }
    }

    private void MoveDoorDown()
    {
        if(doorObj.transform.position.y > lowYPosition)
        {
            var position = doorObj.transform.position;
            position = new Vector3(position.x, position.y - (doorSpeed * Time.deltaTime), position.z);
            doorObj.transform.position = position;
        }
        else
        {
            doorObj.transform.position = new Vector3(doorObj.transform.position.x, lowYPosition, doorObj.transform.position.z);
        }
    }

    private void MoveDoorUp()
    {
        if(doorObj.transform.position.y < maxYPosition)
        {
            var position = doorObj.transform.position;
            position = new Vector3(position.x, position.y + (doorSpeed * Time.deltaTime), position.z);
            doorObj.transform.position = position;
        }
        else
        {
            isOutsideTrigger = false;
            doorObj.transform.position = new Vector3(doorObj.transform.position.x, maxYPosition, doorObj.transform.position.z);
        }
    }

    IEnumerator Close()
    {
        yield return new WaitForSeconds(0.25f);
        if (!IsLocked)
        {
            moveEventClose.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            moveEventClose.start();
        }

        Globals.CheckLowpass(moveEventClose, _player._subOxygen);
    }

    public void ChangeDoorState()
    {
        doorState = !doorState;
        
        if(doorState)
        {
            isInsideTrigger = true;
            isOutsideTrigger = false;
            if (!IsLocked)
            {
                moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                moveEventOpen.start();
            }

            Globals.CheckLowpass(moveEventOpen, _player._subOxygen);
        }
        else if(doorState == false)
        {
            //Close the door
            isInsideTrigger = false;
            isOutsideTrigger = true;
            StartCoroutine(Close());
        }
    }
}
