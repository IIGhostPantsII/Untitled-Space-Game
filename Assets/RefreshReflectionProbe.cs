using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshReflectionProbe : MonoBehaviour
{
    [SerializeField] private float _refreshRate;
    [SerializeField] private float _maxDistance = 100;
    
    private ReflectionProbe _reflection;
    private Transform _player;

    private bool _playerInRange;

    private void Start()
    {
        _reflection = GetComponent<ReflectionProbe>();
        _player = FindObjectOfType<PlayerController>().transform;

        FindObjectOfType<LightFlicker>().OnLightUpdate += () => Refresh();
    }

    private void Refresh()
    {
        if (Vector3.Distance(_player.position, transform.position) < _maxDistance || _playerInRange) _reflection.RenderProbe();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }
}
