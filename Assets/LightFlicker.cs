using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private Material _mat;
    [SerializeField] private float _maxFlickerAmount;
    
    private bool _isFlickering = false;
    public event Action OnLightUpdate;

    private void Update()
    {
        if (!_isFlickering)
        {
            StartCoroutine(Flicker());
        }
    }

    private IEnumerator Flicker()
    {
        _isFlickering = true;
        
        _mat.SetInt("_LightOn", 0);
        OnLightUpdate?.Invoke();
        yield return new WaitForSeconds(Random.Range(0.01f, _maxFlickerAmount));
        
        _mat.SetInt("_LightOn", 1);
        OnLightUpdate?.Invoke();
        yield return new WaitForSeconds(Random.Range(0.01f, _maxFlickerAmount));

        _isFlickering = false;
    }
}
