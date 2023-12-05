using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using UnityEngine.Serialization;

public class Oxygen : MonoBehaviour
{
    public static bool NoSprint;
    public static bool NoOxygen;

    [SerializeField] public float _oxygenMeter = 100.0f;
    [SerializeField] public float _depletionSpeed = 1.0f;

    [SerializeField] private Color _goodState;
    [SerializeField] private Color _warning;
    [SerializeField] private Color _low;

    private RectTransform _rect;
    private Image _image;

    // thresholds for color transitions
    [SerializeField] private float warningThreshold = 50.0f;
    [SerializeField] private float lowThreshold = 20.0f;

    [SerializeField] private bool _subMeter;
    [SerializeField] private Oxygen _mainOxygen;
    [SerializeField] private GameObject _lowOxy;

    // FMOD Things
    public FMODUnity.EventReference moveEventPath;
    public FMOD.Studio.EventInstance moveEvent;
    bool _playOnce = true;

    void Start()
    {
        _rect = gameObject.GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        if(moveEventPath.ToString() != null)
        {
            moveEvent = RuntimeManager.CreateInstance(moveEventPath);
        }
    }

    void Update()
    {
        if(!NoSprint && !_subMeter || !GainOxygen.InStation && !_subMeter)
        {
            _oxygenMeter -= _depletionSpeed * Time.deltaTime;
        }

        if(!_subMeter)
        {
            _oxygenMeter = Mathf.Clamp(_oxygenMeter, 0f, 100f);

            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, _oxygenMeter);
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, -534.0f + ((_oxygenMeter * 2.221f))); // Set your desired position
    
            UpdateColor();
    
            NoSprint = _oxygenMeter <= 0;
        }
        else
        {
            if(_mainOxygen != null)
            {
                if(_mainOxygen._oxygenMeter <= 0)
                {
                    if(!NoOxygen || !GainOxygen.InStation)
                    {
                        _oxygenMeter -= _depletionSpeed * Time.deltaTime;
                    }

                    if(_playOnce)
                    {
                        moveEvent.start();
                        StartCoroutine(LowOxygen(3f));
                        _lowOxy.SetActive(true);
                        _playOnce = false;
                    }

                    _oxygenMeter = Mathf.Clamp(_oxygenMeter, 0f, 100f);

                    _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, _oxygenMeter);
                    _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, -534.0f + ((_oxygenMeter * 1.105f))); // Set your desired position
            
                    UpdateColor();
            
                    NoOxygen = _oxygenMeter <= 0;
                }
                else if(_mainOxygen._oxygenMeter >= 0.1f)
                {
                    _playOnce = true;
                }
            }
        }
    }

    void UpdateColor()
    {
        if(_oxygenMeter > warningThreshold)
        {
            _image.color = _goodState;
        }
        else if(_oxygenMeter > lowThreshold)
        {
            _image.color = _warning;
        }
        else
        {
            _image.color = _low;
        }
    }

    IEnumerator LowOxygen(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _lowOxy.SetActive(false);
    }
}
