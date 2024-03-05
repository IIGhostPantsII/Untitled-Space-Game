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

    [ShowIf("_buttonType", ButtonType.Power)] [AllowNesting] [SerializeField] private string _taskName;
    [ShowIf("_buttonType", ButtonType.Power)] [AllowNesting] public bool IsOn;

    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] private Door _door;
    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] public bool _isDoorOn = true;
    [ShowIf("_buttonType", ButtonType.Door)] [AllowNesting] [SerializeField] public TMP_Text _text;

    public float ButtonProgress;

    private bool _isFilling;
    public event Action OnActivate;

    void Start()
    {
        if(_buttonType == ButtonType.Door)
        {
            ButtonSpeed = 0.5f;
        }

        OnActivate += () => _door.ChangeDoorState();
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
}

public enum ButtonType
{
    Power,
    Door,
}