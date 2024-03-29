using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskAreaController : MonoBehaviour
{
    [SerializeField] private RoomTasks _tasks;
    public bool _roomPower;

    private TaskManager _taskManager;
    private VoicelineController _voiceline;

    private void Start()
    {
        _taskManager = FindObjectOfType<TaskManager>();
        _voiceline = FindObjectOfType<VoicelineController>();
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
            
            _voiceline.TogglePAMode(!_roomPower);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _taskManager.ExitTaskArea();
        }
    }
}
