using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class RandomAmbientNoises : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference[] _ambientSounds;

    private FMOD.Studio.EventInstance[] events;

    private float minDelay = 20f;
    private float maxDelay = 60f;

    int random;

    void Start()
    {
        events = new FMOD.Studio.EventInstance[_ambientSounds.Length];
        for(int i = 0; i < _ambientSounds.Length; i++)
        {
            events[i] = FMODUnity.RuntimeManager.CreateInstance(_ambientSounds[i]);
        }
        StartCoroutine(Ambience());
    }

    IEnumerator Ambience()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            PlayRandomAmbience();
        }
    }

    void PlayRandomAmbience()
    {
        random = Random.Range(0,100);
        if(random < 5)
        {
            events[0].start();
        }
        else if(random > 4 && random < 50)
        {
            events[1].start();
        }
        else
        {
            events[2].start();
        }
    }
}
