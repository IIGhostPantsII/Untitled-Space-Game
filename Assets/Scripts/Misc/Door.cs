using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    public FMODUnity.EventReference moveEventPathOpen;
    public FMOD.Studio.EventInstance moveEventOpen;

    public FMODUnity.EventReference moveEventPathClose;
    public FMOD.Studio.EventInstance moveEventClose;

    public float doorSpeed = 2.0f;
    public float maxYPosition = 7.5f;
    public float lowYPosition = -30f;

    private float timer = 0.0f;
    private float doorDuration;

    [SerializeField] private Oxygen _subMeter;
 
    [HideInInspector] public bool isInsideTrigger = false;
    [HideInInspector] public bool isOutsideTrigger = false;

    private bool doorState = true;

    public static bool PauseMovement;

    void Start()
    {
        doorDuration = Random.Range(2f, 3f);
        moveEventOpen = RuntimeManager.CreateInstance(moveEventPathOpen);
        moveEventClose = RuntimeManager.CreateInstance(moveEventPathClose);
    }

    void Update()
    {
        if(isInsideTrigger)
        {
            MoveDoorDown();
        }
        else if(isOutsideTrigger)
        {
            MoveDoorUp();
        }

        Globals.SpatialSounds(moveEventOpen, gameObject);
        Globals.SpatialSounds(moveEventClose, gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && doorState || other.CompareTag("Monster") && doorState)
        {
            isInsideTrigger = true;
            isOutsideTrigger = false; // Reset the flag when inside the trigger
            moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            moveEventOpen.start();
            Globals.CheckLowpass(moveEventOpen, _subMeter);
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
                moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                moveEventOpen.start();
                Globals.CheckLowpass(moveEventOpen, _subMeter);
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
        if(transform.position.y > lowYPosition)
        {
            transform.Translate(Vector3.down * doorSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, lowYPosition, transform.position.z);
        }
    }

    private void MoveDoorUp()
    {
        if(transform.position.y < maxYPosition)
        {
            transform.Translate(Vector3.up * doorSpeed * Time.deltaTime);
        }
        else
        {
            isOutsideTrigger = false;
            transform.position = new Vector3(transform.position.x, maxYPosition, transform.position.z);
        }
    }

    IEnumerator Close()
    {
        yield return new WaitForSeconds(0.25f);
        moveEventClose.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        moveEventClose.start();
        Globals.CheckLowpass(moveEventClose, _subMeter);
    }

    public void ChangeDoorState()
    {
        doorState = !doorState;
        
        if(doorState)
        {
            isInsideTrigger = true;
            isOutsideTrigger = false;
            moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            moveEventOpen.start();
            Globals.CheckLowpass(moveEventOpen, _subMeter);
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
