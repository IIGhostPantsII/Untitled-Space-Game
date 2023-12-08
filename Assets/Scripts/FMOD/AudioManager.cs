using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference[] _sounds;
    [SerializeField] private FMODUnity.EventReference[] _secondarySounds;

    public static FMOD.Studio.EventInstance[] SoundsInstance;
    public static FMOD.Studio.EventInstance[] SecondarySoundsInstance;

    void Start()
    {
        SoundsInstance = new FMOD.Studio.EventInstance[_sounds.Length];
        SecondarySoundsInstance = new FMOD.Studio.EventInstance[_secondarySounds.Length];

        for(int i = 0; i < _sounds.Length; i++)
        {
            SoundsInstance[i] = RuntimeManager.CreateInstance(_sounds[i]);
        }

        for(int j = 0; j < _secondarySounds.Length; j++)
        {
            SecondarySoundsInstance[j] = RuntimeManager.CreateInstance(_secondarySounds[j]);
        }
    }

    public static void PlaySound(int clip)
    {
        SoundsInstance[clip].start();
    }

    public static void PlaySecondarySound(int clip)
    {
        SecondarySoundsInstance[clip].start();
    }

    public static void StopPlaySound(int clip)
    {
        SoundsInstance[clip].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SoundsInstance[clip].start();
    }

    public void ChangeParameter(FMOD.Studio.EventInstance eventInstance, string parameter, float number)
    {
        eventInstance.setParameterByName(parameter, number);
    }
}
