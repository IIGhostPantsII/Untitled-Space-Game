using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TaskIconController _taskIcon;
    [SerializeField] private GameObject _taskListUI;
    
    [SerializeField] private RoomTasks _debugTaskList;
    
    
    private bool _inTask;
    [SerializeField] private RoomTasks _currentTaskList;
    private List<TaskIconController> _taskIcons = new List<TaskIconController>();
    private Animator _anim;

    private InActiveRoom _roomNode;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }
    
    [Button]
    public void EnterDebugTask()
    {
        EnterTaskArea(_debugTaskList);
    }

    public void IncrementTask(string taskName)
    {
        for (int i = 0; i < _currentTaskList.Tasks.Length; i++)
        {
            if (_currentTaskList.Tasks[i].TaskName.Equals(taskName))
            {
                _taskIcons[i].IncrementTaskStep();
                _currentTaskList.Tasks[i].CompletedSteps += 1;
            }
        }
    } 
    
    public void EnterTaskArea(RoomTasks taskList)
    {
        foreach (TaskIconController obj in _taskIcons)
        {
            Destroy(obj.gameObject);
        }

        _taskIcons = new List<TaskIconController>();

        if (!Globals.RoomTasks.Contains(taskList))
        {
            Debug.Log($"Adding room \"{taskList.RoomName}\" to global list");
            Globals.RoomTasks.Add(taskList);
        }

        _currentTaskList = taskList;
        _roomNameText.text = taskList.RoomName;
        foreach (Task task in taskList.Tasks)
        {
            TaskIconController obj = Instantiate(_taskIcon.gameObject, _taskListUI.transform).GetComponent<TaskIconController>();
            _taskIcons.Add(obj);
            obj.gameObject.SetActive(true);
            
            obj.SetTask(task.TaskName, task.TotalSteps, task.CompletedSteps);
        }

        _inTask = true;
        _anim.Play("ListAppear", 0, 0);
    }

    [Button]
    public void ExitTaskArea()
    {
        _inTask = false;
        _currentTaskList = new RoomTasks();
        _anim.Play("ListDisappear", 0, 0);
    }

    private void OnEnable()
    {
        if (_inTask)
        {
            _anim.Play("ListAppear", 0, 1);
            _anim.Update(0);
        }
        else
        {
            _anim.Play("ListDisappear", 0, 1);
            _anim.Update(0);
        }
    }
}
