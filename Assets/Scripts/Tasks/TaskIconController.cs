using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class TaskIconController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Animator _anim;

    [HideInInspector] public bool TaskDone;
    
    private string _taskName;
    private int _stepsCompleted;
    private int _maxSteps;
    
    public void SetTask(string taskName, int maxSteps, int stepsCompleted)
    {
        _taskName = taskName;
        _maxSteps = maxSteps;
        _stepsCompleted = stepsCompleted;
        
        _text.text = maxSteps > 1 ? $"{taskName} ({stepsCompleted}/{maxSteps})" : taskName;
    }

    [Button()]
    public void IncrementTaskStep()
    {
        _stepsCompleted += 1;
        if (_stepsCompleted >= _maxSteps)
        {
            FinishTask();
        }
            
        _text.text = _maxSteps > 1 ? $"{_taskName} ({_stepsCompleted}/{_maxSteps})" : _taskName;
    }
    
    [Button]
    private void FinishTask()
    {
        TaskDone = true;
        _anim.Play("TaskDone");
    }

    private void OnEnable()
    {
        if (_stepsCompleted >= _maxSteps)
        {
            _anim.Play("TaskDone", 0, 1);
        }
    }
}
