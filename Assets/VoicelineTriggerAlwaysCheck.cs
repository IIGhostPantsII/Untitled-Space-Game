using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoicelineTriggerAlwaysCheck : MonoBehaviour
{
    [SerializeField] private string[] _voicelines;
    [SerializeField] private string[] _flags;

    private bool _playerColliding;
    private VoicelineController _voiceineController;

    private void Start()
    {
        _voiceineController = FindObjectOfType<VoicelineController>();
    }

    private void Update()
    {
        if (!_playerColliding) return;
        if (_flags.Length >= 1)
            if (!Globals.CheckStoryFlags(_flags)) return;

        _voiceineController.PlaySound(_voicelines[Random.Range(0, _voicelines.Length)]);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Killing myself");
            _playerColliding = true;
        }
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _playerColliding = false;
    }
}
