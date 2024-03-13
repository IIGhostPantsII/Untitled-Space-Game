using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class VoicelineFlagDoorActivation : MonoBehaviour
{
    [SerializeField] private string _flagForActivation;
    [SerializeField] [Label("Turn On When Activated")] private bool _turnOn;
    [SerializeField] private Door _door;

    private bool _hasUpdated;
    
    private void Start()
    {
        _door.IsLocked = _turnOn;
    }

    private void Update()
    {
        if (_hasUpdated) return;

        if (!Globals.StoryFlags.Contains(_flagForActivation)) return;
        
        _hasUpdated = true;
        _door.IsLocked = !_door.IsLocked;
    }
}
