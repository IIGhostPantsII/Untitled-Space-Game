using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private Material _mat;
    [SerializeField] private float _maxFlickerAmount;
    
    private bool _isFlickering = false;

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
        yield return new WaitForSeconds(Random.Range(0.01f, _maxFlickerAmount));
        
        _mat.SetInt("_LightOn", 1);
        yield return new WaitForSeconds(Random.Range(0.01f, _maxFlickerAmount));

        _isFlickering = false;
    }
}
