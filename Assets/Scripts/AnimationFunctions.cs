using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AnimationFunctions : MonoBehaviour
{
    public static string Name;

    public EventReference[] events;
    private FMOD.Studio.EventInstance eventInstance;

    [SerializeField] private GameObject _object;

    public void LockPlayerMovement()
    {
        Globals.LockMovement();
    }

    public void UnlockPlayerMovement()
    {
        Globals.UnlockMovement();
    }
    
    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

    public void SetStringName(string name)
    {
        Name = name;
    }

    public void TriggerAnimation(string name)
    {
        GameObject gameAni = GameObject.Find(Name);
        Animator animator = gameAni.GetComponent<Animator>();
        animator.Play(name);
    }
    
    public void PlayFMODEvent(int value)
    {
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(events[value]);
        eventInstance.start();
    }

    private void Update()
    {
        // Update the FMOD event instance position based on the GameObject's position
        if (eventInstance.isValid())
        {
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
        }
    }

    public void ReleaseFMODEvent()
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        eventInstance.release();
    }

    public void PlayOtherAnimation(string clip)
    {
        Animator _animator = gameObject.GetComponent<Animator>();
        Debug.Log("Playing " + clip);
        _animator.Play(clip);
    }

    //Let scripts know that animation is over
    public void Finish()
    {
        Globals.FinishedAnimation();
    }

    public void DisableAnimator()
    {
        Animator pain = gameObject.GetComponent<Animator>();
        pain.enabled = false;
    }

    public void DeathAnimation(string name)
    {
        Animator animator = _object.GetComponent<Animator>();
        animator.Play(name);
    }
}
