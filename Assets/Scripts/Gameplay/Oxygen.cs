using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class Oxygen : MonoBehaviour
{
    public static bool NoSprint;
    public static bool NoOxygen;
    public static bool PauseDepletion;

    public float _oxygenMeter = 100.0f;
    public float _normalDepletionSpeed = 1f;
    public float _crouchDepletionSpeed = 0.5f;
    public float _sprintDepletionSpeed = 2.5f;

    [SerializeField] private Color goodState;
    [SerializeField] private Color warning;
    [SerializeField] private Color low;
    private Color _currentCol;

    private Slider _oxyLevel;
    [SerializeField] private Image _image;
    [HideInInspector] public float _depletionSpeed = 1.0f;

    // thresholds for color transitions
    [SerializeField] private float warningThreshold = 50.0f;
    [SerializeField] private float lowThreshold = 20.0f;
    [SerializeField] public float _time = 0f;
    [SerializeField] private bool _subMeter;
    [SerializeField] private Oxygen _mainOxygen;
    [SerializeField] private Animator _lowOxy;

    [Space] [SerializeField] private Volume _volume; [Space]
    
    // FMOD Things
    public FMODUnity.EventReference eventPath;
    public FMOD.Studio.EventInstance eventInstance;
    public FMODUnity.EventReference whiteNoiseEventPath;
    public FMOD.Studio.EventInstance whiteNoiseEventInstance;
    public FMODUnity.EventReference heartbeatEventPath;
    public FMOD.Studio.EventInstance heartbeatEventInstance;
    bool _playOnce = true;

    void Start()
    {
        _oxyLevel = gameObject.GetComponent<Slider>();

        if(eventPath.ToString() != null)
        {
            eventInstance = RuntimeManager.CreateInstance(eventPath);
        }

        if(whiteNoiseEventPath.ToString() != null)
        {
            whiteNoiseEventInstance = RuntimeManager.CreateInstance(whiteNoiseEventPath);
        }

        if(heartbeatEventPath.ToString() != null)
        {
            heartbeatEventInstance = RuntimeManager.CreateInstance(heartbeatEventPath);
        }
    }

    void Update()
    {
        UpdateColor();
        
        if (PauseDepletion) return;

        if (!NoSprint && !_subMeter || !GainOxygen.InStation && !_subMeter)
        {
            _oxygenMeter -= _depletionSpeed * Time.deltaTime;
        }

        if (!_subMeter)
        {
            _oxygenMeter = Mathf.Clamp(_oxygenMeter, 0f, 100f);

            _oxyLevel.value = _oxygenMeter / 100f;

            NoSprint = _oxygenMeter <= 0;
        }
        else if (_mainOxygen != null)
        {
            _volume.weight = (1 - (_oxygenMeter / 100f)) * 3;
            
            if (_mainOxygen._oxygenMeter <= 0)
            {
                if(!NoOxygen || !GainOxygen.InStation)
                {
                    _oxygenMeter -= _depletionSpeed * Time.deltaTime;
                }

                if(_playOnce)
                {
                    _time = 0f;
                    eventInstance.start();
                    whiteNoiseEventInstance.start();
                    heartbeatEventInstance.start();
                    _lowOxy.gameObject.SetActive(true);
                    
                    AnimationClip[] clips = _lowOxy.runtimeAnimatorController.animationClips;
                    
                    StartCoroutine(LowOxygen(clips[0].length));
                    _playOnce = false;
                }

                if(NoSprint && eventInstance.isValid() || NoSprint && whiteNoiseEventInstance.isValid() || NoSprint && heartbeatEventInstance.isValid())
                {
                    _time += Time.deltaTime;
                    float clampedTime = Mathf.Clamp(_time * 4, 0f, 100f);
                    eventInstance.setParameterByName("Lowpass",(_oxygenMeter * 220));
                    whiteNoiseEventInstance.setParameterByName("Volume",(clampedTime));
                    heartbeatEventInstance.setParameterByName("Volume",(clampedTime));
                }

                _oxygenMeter = Mathf.Clamp(_oxygenMeter, 0f, 100f);

                _oxyLevel.value = _oxygenMeter / 100f;

                NoOxygen = _oxygenMeter <= 0;
            }
            else
            {
                _playOnce = true;
            }
        }
    }

    void UpdateColor()
    {
        float alpha = _image.color.a;
        
        if(_oxygenMeter <= lowThreshold && (_currentCol != low))
        {
            _currentCol = low;
            StartCoroutine(TransitionColor(_image.color, low));
        }
        else if(_oxygenMeter <= warningThreshold && _oxygenMeter > lowThreshold && (_currentCol != warning))
        {
            _currentCol = warning;
            StartCoroutine(TransitionColor(_image.color, warning));
        }
        else if (_oxygenMeter > warningThreshold && _currentCol != goodState)
        {
            _currentCol = goodState;
            StartCoroutine(TransitionColor(_image.color, goodState));
        }

        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
    }

    private IEnumerator TransitionColor(Color col1, Color col2)
    {
        float time = 0f;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;

            _image.color = Color.Lerp(col1, col2, time / duration);
            yield return null;
        }

        _image.color = col2;
    }

    IEnumerator LowOxygen(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _lowOxy.gameObject.SetActive(false);
    }
}
