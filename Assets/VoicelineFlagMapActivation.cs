using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class VoicelineFlagMapActivation : MonoBehaviour
{
    [SerializeField] private string _flagForActivation;
    [SerializeField] private GameObject _map;

    private bool _hasUpdated;
    private void Update()
    {
        if (_hasUpdated) return;
        if (!Globals.StoryFlags.Contains(_flagForActivation)) return;
        
        _hasUpdated = true;
        _map.SetActive(true);
    }
}