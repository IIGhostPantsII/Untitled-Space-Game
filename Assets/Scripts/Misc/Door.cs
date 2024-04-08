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
    private float maxYPosition;
    public float lowYPosition = -1.1f;

    private float timer = 0.0f;
    private float doorDuration;
    private PlayerController player;
    private BoxCollider col;
 
    [HideInInspector] public bool isInsideTrigger = false;
    [HideInInspector] public bool isOutsideTrigger = false;

    [SerializeField] [ColorUsageAttribute(true, true)]
    private Color UnlockedColor;
    [SerializeField] [ColorUsageAttribute(true, true)]
    private Color LockedColor;
    
    [SerializeField] [ColorUsageAttribute(true, true)]
    private Color UnlockedColorAlt;
    [SerializeField] [ColorUsageAttribute(true, true)]
    private Color LockedColorAlt;
    
    [SerializeField] private Light[] _lights;
    [SerializeField] private MeshRenderer[] _lightObjects;

    [SerializeField] public bool IsLocked = false;
    private bool lockedUp;

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
        player = FindObjectOfType<PlayerController>();
        col = doorObj.GetComponent<BoxCollider>();

        if(IsLocked)
        {
            col.enabled = true;
        }
        else
        {
            col.enabled = false;
        }
    }

    void Update()
    {
        if(isOutsideTrigger || IsLocked && !lockedUp)
        {
            MoveDoorUp();
        }
        else if(isInsideTrigger && !IsLocked)
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
            if(!IsLocked)
            {
                moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                moveEventOpen.start();
            }

            Globals.CheckLowpass(moveEventOpen, player._subOxygen);
        }
    }

    public void UpdateLockedLights(bool forceUpdate = false)
    {
        if((IsLocked && !LockedLights) || forceUpdate)
        {
            LockedLights = true;

            foreach (Light light in _lights)
            {
                light.color = Globals.AltDoorColors ? LockedColorAlt : LockedColor;
            }

            foreach (MeshRenderer renderer in _lightObjects)
            {
                renderer.material.SetColor("_EmissionColor", Globals.AltDoorColors ? LockedColorAlt : LockedColor);
            }
        } 
        else if((!IsLocked && LockedLights) || forceUpdate)
        {
            LockedLights = false;
            
            foreach (Light light in _lights)
            {
                light.color = Globals.AltDoorColors ? UnlockedColorAlt : UnlockedColor;
            }
            foreach (MeshRenderer renderer in _lightObjects)
            {
                renderer.material.SetColor("_EmissionColor", Globals.AltDoorColors ? UnlockedColorAlt : UnlockedColor);
            }
            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Monster") && !doorState)
        {
            Globals.LockMonsterMovement();
            
            if(gameObject.CompareTag("EndgameDoor") && EndgameLogic.Started && EndgameLogic.CanEndGame)
            {
                EndgameLogic.EndGame();
            }
            else
            {
                NavMeshAgent monsterPathing = other.gameObject.GetComponent<NavMeshAgent>();
                timer += Time.deltaTime;
                monsterPathing.SetDestination(other.gameObject.transform.position);

                if(timer > doorDuration)
                {
                    Globals.UnlockMonsterMovement();
                    doorDuration = Random.Range(2f, 3f);
                    timer = 0;
                    doorState = true;
                    isInsideTrigger = true;
                    isOutsideTrigger = false;

                    IsLocked = false;

                    moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    moveEventOpen.start();

                    Globals.CheckLowpass(moveEventOpen, player._subOxygen);
                    
                }
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
            lockedUp = true;
            isOutsideTrigger = false;
            doorObj.transform.position = new Vector3(doorObj.transform.position.x, maxYPosition, doorObj.transform.position.z);
        }
    }

    IEnumerator Close()
    {
        yield return new WaitForSeconds(0.25f);

        moveEventClose.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        moveEventClose.start();

        Globals.CheckLowpass(moveEventClose, player._subOxygen);
    }

    public void ChangeDoorState()
    {
        doorState = !IsLocked;

        if(IsLocked)
        {
            col.enabled = true;
            lockedUp = false;
            if(isInsideTrigger)
            {
                StartCoroutine(Close());
            }
        }
        else
        {
            col.enabled = false;
            if(isInsideTrigger)
            {
                moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                moveEventOpen.start();
                Globals.CheckLowpass(moveEventOpen, player._subOxygen);
            }
        }
    }
}
