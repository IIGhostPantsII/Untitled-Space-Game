using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class GainOxygen : MonoBehaviour
{
    [SerializeField] private Oxygen _oxygenScript;
    [SerializeField] private Oxygen _subOxygenScript;

    public static bool InStation;

    // FMOD Things
    public FMODUnity.EventReference eventPath;
    public FMOD.Studio.EventInstance eventInstance;

    void Start()
    {
        if(eventPath.ToString() != null)
        {
            eventInstance = RuntimeManager.CreateInstance(eventPath);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            InStation = true;
            eventInstance.start();
            Oxygen.PauseDepletion = true;
            if(Oxygen.NoSprint)
            {
                eventInstance.setParameterByName("Lowpass",(_subOxygenScript._oxygenMeter * 220));
            }
            else
            {
                eventInstance.setParameterByName("Lowpass",22000);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Oxygen.PauseDepletion = false;
            InStation = false;
        }
    }

    void Update()
    {
        if(InStation)
        {
            _subOxygenScript._time -= Time.deltaTime * 2;
            if(_subOxygenScript._oxygenMeter >= 100f)
            {
                _oxygenScript._oxygenMeter += 4.0f * Time.deltaTime;
            }
            else
            {
                _subOxygenScript._oxygenMeter += 8.0f * Time.deltaTime;
            }
        }
    }
}
