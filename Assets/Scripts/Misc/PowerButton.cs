using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class PowerButton : MonoBehaviour
{
    // Time in seconds it takes to press the power button
    public static float ButtonSpeed = 2f;

    [SerializeField] public ButtonType _buttonType;
    [SerializeField] private bool _multiPress = false;
    public bool IsOn;

    [ShowIf("ShouldShowTasks")] [AllowNesting] [SerializeField] private string _taskName;

    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] private Door _door;
    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] public TMP_Text _text;

    [ShowIf("ShouldShowPickup")] [AllowNesting] [SerializeField] private PickupAndPlace _pickUp;
    [ShowIf("ShouldShowPickup")] [AllowNesting] [SerializeField] private int _value;

    public float ButtonProgress;

    private bool _isFilling;
    private bool doorState = true;

    public event Action OnActivate;

    void Start()
    {
        if(_buttonType == ButtonType.Door)
        {
            ButtonSpeed = 0.25f;
            OnActivate += () => _door.ChangeDoorState();
        }
        else if(_buttonType == ButtonType.Pickup)
        {
            OnActivate += () => _pickUp.Pickup(_value);
        }
        else if(_buttonType == ButtonType.Place)
        {
            OnActivate += () => _pickUp.Place(_value);
        }
    }

    public void FillBar()
    {
        if (IsOn) return;

        _isFilling = true;
        ButtonProgress += Time.deltaTime / ButtonSpeed;
        if (ButtonProgress >= 1)
        {
            IsOn = true;
            ButtonProgress = 1;
            if (!string.IsNullOrEmpty(_taskName)) FindObjectOfType<TaskManager>().IncrementTask(_taskName);
            doorState = !doorState;
            if(doorState && _buttonType == ButtonType.Door)
            {
                _text.SetText("Close Door");
            }
            else if(_buttonType == ButtonType.Door)
            {
                _text.SetText("Open Door");
            }
            OnActivate?.Invoke();
        }
    }

    public void Update()
    {
        if (IsOn && !_multiPress) return;

        if (IsOn && _multiPress)
        {
            ButtonProgress -= (Time.deltaTime * 2) / ButtonSpeed;
            if (ButtonProgress <= 0) {
                ButtonProgress = 0;
                IsOn = false;
            }

            return;
        }

        if (!_isFilling && ButtonProgress > 0)
        {
            ButtonProgress -= (Time.deltaTime * 2) / ButtonSpeed;
            if (ButtonProgress <= 0) ButtonProgress = 0;
        }

        _isFilling = false;
    }

    private bool ShouldShowTasks()
    {
        return _buttonType == ButtonType.Place || _buttonType == ButtonType.Power;
    }

    private bool ShouldShowPickup()
    {
        return _buttonType == ButtonType.Place || _buttonType == ButtonType.Pickup;
    }
}

public enum ButtonType
{
    Power,
    Door,
    Pickup,
    Place,
}