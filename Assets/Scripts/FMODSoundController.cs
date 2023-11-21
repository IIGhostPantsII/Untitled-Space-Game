using UnityEngine;
using FMODUnity;

public class FMODSoundController : MonoBehaviour
{
    public EventReference fmodEvent;

    private FMOD.Studio.EventInstance soundEvent;

    void Start()
    {
        soundEvent = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        soundEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
        soundEvent.start();
    }

    void Update()
    {
        // You can update sound parameters or handle other logic here
    }

    void OnDestroy()
    {
        soundEvent.release();
    }
}
