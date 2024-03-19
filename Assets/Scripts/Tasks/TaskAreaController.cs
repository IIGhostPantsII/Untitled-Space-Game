using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskAreaController : MonoBehaviour
{
    [SerializeField] private RoomTasks _tasks;

    private TaskManager _taskManager;

    private void Start()
    {
        _taskManager = FindObjectOfType<TaskManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _taskManager.EnterTaskArea(_tasks);
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
