using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Unity.Collections;

public class GainOxygen : MonoBehaviour
{
    [ReadOnly] private PlayerController _player;

    public static bool InStation;

    // FMOD Things
    public FMODUnity.EventReference eventPath;
    public FMOD.Studio.EventInstance eventInstance;

    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        
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
                eventInstance.setParameterByName("Lowpass",(_player._subOxygen._oxygenMeter * 220));
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
            _player._subOxygen._time -= Time.deltaTime * 2;
            if(_player._subOxygen._oxygenMeter >= 100f)
            {
                _player._oxygen._oxygenMeter += 8.0f * Time.deltaTime;
            }
            else
            {
                _player._subOxygen._oxygenMeter += 16.0f * Time.deltaTime;
            }
        }
    }
}
