using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainOxygen : MonoBehaviour
{
    [SerializeField] private Oxygen _oxygenScript;

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
            Debug.Log(InStation);
        }
    }

    void Update()
    {
        if(InStation)
        {
            _oxygenScript._oxygenMeter += 4.0f * Time.deltaTime;
        }
    }
}
