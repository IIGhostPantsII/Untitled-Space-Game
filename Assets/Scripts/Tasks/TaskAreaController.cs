using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class TaskAreaController : MonoBehaviour
{
    [SerializeField] private RoomTasks _tasks;
    public bool _roomPower;
    [SerializeField] private int _roomNum;

    private TaskManager _taskManager;
    private VoicelineController _voiceline;
    [SerializeField] [ReadOnly] private InActiveRoom _roomNode;

    private void Start()
    {
        _taskManager = FindObjectOfType<TaskManager>();
        _voiceline = FindObjectOfType<VoicelineController>();


        _roomNode = FindObjectOfType<RoomOrbController>()._rooms[_roomNum].GetComponent<InActiveRoom>();
    }

    public void UpdatePAMode()
    {
        _voiceline.TogglePAMode(!_roomPower);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _taskManager.EnterTaskArea(_tasks);
            _roomNode.ToggleEnterRoom(true);
            
            _voiceline.TogglePAMode(!_roomPower);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _taskManager.ExitTaskArea();
            _roomNode.ToggleEnterRoom(false);
        }
    }
}
