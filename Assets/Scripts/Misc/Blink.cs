using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Blink : MonoBehaviour
{
    private Animator _anim;
    private float _timer;
    private bool _enabled = true;

    public string _animName = "Blink";
    
    void Awake()
    {
        _timer = Random.Range(0f, 7f);
        _anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (_anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1f)
        {
            _anim.Play("Null", 0, 0);
        }
        
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _timer = Random.Range(1f, 7f);
            if (_enabled) _anim.Play(_animName, 0, 0);
        }
    }

    public void BlinkEnabled()
    {
        _enabled = true;
    }
    
    public void BlinkDisabled()
    {
        _enabled = false;
        _anim.Play("Null", 1, 0);
        _anim.Update(Time.deltaTime);
    }
    
    [Button]
    private void StartBlink()
    {
        _timer = -12;
    }
}
