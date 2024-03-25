using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLight : MonoBehaviour
{
    [SerializeField] private Material _litMat;
    [SerializeField] private Animator _anim;
    [SerializeField] private string _animation;
    [SerializeField] private int _roomNum;
    [SerializeField] private TaskAreaController _linkedTaskArea;
    [SerializeField] private PowerButton _linkedButton;
    [SerializeField] private GameObject[] _lights;
    [SerializeField] private MoveObject _move;

    private void Start()
    {
        _linkedButton.OnActivate += TurnOnLight;

        if (_anim == null) _anim = GetComponent<Animator>();
    }

    private void TurnOnLight()
    {
        if (_linkedTaskArea != null)
        {
            _linkedTaskArea._roomPower = true;
            _linkedTaskArea.UpdatePAMode();
            Globals.RoomPower[_roomNum] = true;
            FindObjectOfType<RoomOrbController>().UpdateRoomInfo();
        }

        try { GetComponent<MeshRenderer>().material = _litMat; } catch { /* ignored */ }

        try
        {
            _anim.Play(_animation);
        } catch { /* ignored */ }

        foreach (GameObject light in _lights)
        {
            light.SetActive(true);
        }
        
        Globals.PowerCheck();

        if(Globals.AllPowerOn)
        {
            _move.Move();
        }
    }
}
