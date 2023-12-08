using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Speakers : MonoBehaviour
{
    [SerializeField] private RandomChatter _chatter;
    [SerializeField] private Oxygen _oxygen;

    private FMOD.Studio.EventInstance eventInstance;

    private bool doSomething;

    void Update()
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;
        eventInstance.getPlaybackState(out playbackState);

        if (eventInstance.isValid())
        {
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
            //if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED && doSomething)
            //{
            //    doSomething = false;
            //    Debug.Log($"dosmt {doSomething}");
            //    _chatter.GoAgain();
            //}
        }
    }

    public void PlaySound(FMODUnity.EventReference reference, bool afterEffect)
    {
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(reference);
        eventInstance.start();

        if(Oxygen.NoSprint)
        {
            eventInstance.setParameterByName("Lowpass",(_oxygen._oxygenMeter * 220));
        }
        else
        {
            eventInstance.setParameterByName("Lowpass",22000);
        }

        if(afterEffect)
        {
            doSomething = true;
        }
    }
}
