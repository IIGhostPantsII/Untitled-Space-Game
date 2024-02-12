using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Door : MonoBehaviour
{
    public FMODUnity.EventReference moveEventPathOpen;
    public FMOD.Studio.EventInstance moveEventOpen;

    public FMODUnity.EventReference moveEventPathClose;
    public FMOD.Studio.EventInstance moveEventClose;

    public float doorSpeed = 2.0f;
    public float maxYPosition = 7.5f;
    public float lowYPosition = -30f;

    [SerializeField] private Oxygen _subMeter;
 
    private bool isInsideTrigger = false;
    private bool isOutsideTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Monster"))
        {
            isInsideTrigger = true;
            isOutsideTrigger = false; // Reset the flag when inside the trigger
            moveEventOpen.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            moveEventOpen.start();
            Globals.CheckLowpass(moveEventOpen, _subMeter);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Monster"))
        {
            isInsideTrigger = false;
            isOutsideTrigger = true;
            StartCoroutine(Close());
        }
    }

    void Start()
    {
        moveEventOpen = RuntimeManager.CreateInstance(moveEventPathOpen);
        moveEventClose = RuntimeManager.CreateInstance(moveEventPathClose);
    }

    private void Update()
    {
        if (isInsideTrigger)
        {
            MoveDoorDown();
        }
        else if (isOutsideTrigger)
        {
            MoveDoorUp();
        }

        Globals.SpatialSounds(moveEventOpen, gameObject);
        Globals.SpatialSounds(moveEventClose, gameObject);
    }

    private void MoveDoorDown()
    {
        if (transform.position.y > lowYPosition)
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
        if (transform.position.y < maxYPosition)
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
}
