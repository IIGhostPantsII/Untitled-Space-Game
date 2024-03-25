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
    public static float ButtonSpeed = 1.5f;

    [SerializeField] public ButtonType _buttonType;
    [SerializeField] private bool _multiPress = false;
    public bool IsOn;

    [ShowIf("ShouldShowTasks")] [AllowNesting] [SerializeField] private string _taskName;

    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] private Door _door;
    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] public TMP_Text _text;

    [ShowIf("ShouldShowPickup")] [AllowNesting] [SerializeField] private PickupAndPlace _pickUp;
    [ShowIf("_buttonType", ButtonType.Pickup)] [AllowNesting] [SerializeField] private bool _automatic;
    [ShowIf("_buttonType", ButtonType.Pickup)] [AllowNesting] [SerializeField] private int _value;

    [ShowIf("_buttonType", ButtonType.Place)] [AllowNesting] [SerializeField] public PowerButton[] _pickupItems;

    [ShowIf("ShouldShowTextPrompt")] [AllowNesting] [SerializeField] public string _taskText;

    [ShowIf("_buttonType", ButtonType.End)] [AllowNesting] [SerializeField] public EndgameLogic _end;
    [ShowIf("_buttonType", ButtonType.End)] [AllowNesting] [SerializeField] public GameObject _light;
    [ShowIf("_buttonType", ButtonType.End)] [AllowNesting] [SerializeField] public GameObject _lowGrav;

    public float ButtonProgress;

    private bool _isFilling;

    [HideInInspector] public bool pickedUp;
    [HideInInspector] public bool doorState = true;

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
            OnActivate += () => _pickUp.Pickup(_value, _automatic);
        }
        else if(_buttonType == ButtonType.Place)
        {
            OnActivate += () => _pickUp.Place();
        }
        else if(_buttonType == ButtonType.Disappear)
        {
            OnActivate += () => gameObject.SetActive(false);
        }
        else if(_buttonType == ButtonType.Fill)
        {
            GameObject child = gameObject.transform.GetChild(0).gameObject;
            OnActivate += () => child.SetActive(true);
        }
        else if(_buttonType == ButtonType.End)
        {
            OnActivate += () => _light.SetActive(true);
            OnActivate += () => _lowGrav.SetActive(false);
            OnActivate += () => _end.StartEndgame();
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
            doorState = !doorState;

            switch(_buttonType)
            {
                case ButtonType.Door:
                    if(doorState)
                    {
                        _door.IsLocked = false;
                        _text.SetText("Close Door");
                    }
                    else
                    {
                        _door.IsLocked = true;
                        _text.SetText("Open Door");
                    }
                    break;

                case ButtonType.Pickup:
                    pickedUp = true;
                    break;
            }

            
            OnActivate?.Invoke();

            if(_taskName != null && _buttonType == ButtonType.Place || _taskName != null && _buttonType == ButtonType.Pickup && _automatic)
            {
                for(int i = 0; i < _pickUp.counter; i++)
                {
                    FindObjectOfType<TaskManager>().IncrementTask(_taskName);
                }
                _pickUp.counter = 0;
                return;
            }

            if (!string.IsNullOrEmpty(_taskName)) FindObjectOfType<TaskManager>().IncrementTask(_taskName);

            if (!_multiPress) enabled = false;

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
        return _buttonType == ButtonType.Place || _buttonType == ButtonType.Disappear || _buttonType == ButtonType.Pickup && _automatic || _buttonType == ButtonType.Fill;
    }

    private bool ShouldShowPickup()
    {
        return _buttonType == ButtonType.Place || _buttonType == ButtonType.Pickup;
    }

    private bool ShouldShowTextPrompt()
    {
        return _buttonType == ButtonType.Disappear || _buttonType == ButtonType.Fill || _buttonType == ButtonType.End;
    }
}

public enum ButtonType
{
    Door,
    Pickup,
    Place,
    Disappear,
    Fill,
    End
}