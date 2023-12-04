using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainOxygen : MonoBehaviour
{
    [SerializeField] private Oxygen _oxygenScript;
    [SerializeField] private Oxygen _subOxygenScript;

    public static bool InStation;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            InStation = true;
            Debug.Log(InStation);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            InStation = false;
        }
    }

    void Update()
    {
        if(InStation)
        {
            if(_subOxygenScript._oxygenMeter >= 100f)
            {
                _oxygenScript._oxygenMeter += 4.0f * Time.deltaTime;
            }
            else
            {
                _subOxygenScript._oxygenMeter += 8.0f * Time.deltaTime;
            }
        }
    }
}
