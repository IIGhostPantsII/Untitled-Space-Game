using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitThenFadeIn : MonoBehaviour
{
    [SerializeField] private float _waitTime;
    [SerializeField] private float _fadeInTime;
    private Image _image;
    private Color _targetColor;
    private Color _startingColor;

    private float _waitTimer;
    private float _fadeTimer;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _targetColor = _image.color;
        _startingColor = _targetColor;
        _startingColor.a = 0;
        _image.color = _startingColor;
    }

    private void Update()
    {
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitTime)
        {
            _fadeTimer += Time.deltaTime;
            _image.color = Color.Lerp(_startingColor, _targetColor, _fadeTimer / _fadeInTime);

            if (_fadeTimer >= _fadeInTime)
            {
                _image.color = _targetColor;
                enabled = false;
            }
        }
        else
        {
            _image.color = _startingColor;
        }
    }
}
