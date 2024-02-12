using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLight : MonoBehaviour
{
    [SerializeField] private Material _litMat;
    [SerializeField] private PowerButton _linkedButton;
    [SerializeField] private GameObject[] _lights;

    private void Start()
    {
        _linkedButton.OnActivate += TurnOnLight;
    }

    private void TurnOnLight()
    {
        GetComponent<MeshRenderer>().material = _litMat;
        foreach (GameObject light in _lights)
        {
            light.SetActive(true);
        }
    }
}
