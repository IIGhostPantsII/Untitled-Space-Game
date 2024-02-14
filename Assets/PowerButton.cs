using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerButton : MonoBehaviour
{
    // Time in seconds it takes to press the power button
    public static float ButtonSpeed = 2f;
    
    public bool IsOn;
    public float ButtonProgress;

    private bool _isFilling;
    public event Action OnActivate;

    public void FillBar()
    {
        if (IsOn) return;

        _isFilling = true;
        ButtonProgress += Time.deltaTime / ButtonSpeed;
        if (ButtonProgress >= 1)
        {
            IsOn = true;
            ButtonProgress = 1;
            OnActivate?.Invoke();
        }
    }

    public void Update()
    {
        if (IsOn) return;

        if (!_isFilling && ButtonProgress > 0)
        {
            ButtonProgress -= (Time.deltaTime * 2) / ButtonSpeed;
            if (ButtonProgress <= 0) ButtonProgress = 0;
        }

        _isFilling = false;
    }
}

