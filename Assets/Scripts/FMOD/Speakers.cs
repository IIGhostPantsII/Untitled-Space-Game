using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Speakers : MonoBehaviour
{
    [SerializeField] private RandomChatter _chatter;
    [SerializeField] private Oxygen _oxygen;

    private FMOD.Studio.EventInstance eventInstance;

    public static bool DoSomething;

    void Update()
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;
        eventInstance.getPlaybackState(out playbackState);

        if (eventInstance.isValid())
        {
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
            Debug.Log($"dosmt {DoSomething}");
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED && DoSomething)
            {
                DoSomething = false;
                _chatter.GoAgain();
            }
        }

        if(Oxygen.NoSprint)
        {
            eventInstance.setParameterByName("Lowpass",(_oxygen._oxygenMeter * 220));
        }
        else
        {
            eventInstance.setParameterByName("Lowpass",22000);
        }
    }

    public void PlaySound(FMODUnity.EventReference reference, bool afterEffect)
    {
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(reference);
        eventInstance.start();

        if(afterEffect)
        {
            DoSomething = true;
        }
    }
}
