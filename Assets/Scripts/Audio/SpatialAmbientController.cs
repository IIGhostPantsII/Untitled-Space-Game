using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class SpatialAmbientController : MonoBehaviour
{
    [SerializeField] private GameObject[] _soundObjects;

    [SerializeField] private float minDelay = 60f;
    [SerializeField] private float maxDelay = 120f;

    int random;
    float randomTime;

    void Start()
    {
        randomTime = Random.Range(minDelay, maxDelay);
        StartCoroutine(Ambience());
    }

    IEnumerator Ambience()
    {
        while(true)
        {
            yield return new WaitForSeconds(randomTime);
            PlayRandomAmbience();
        }
    }

    void PlayRandomAmbience()
    {
        random = Random.Range(0, _soundObjects.Length);
        randomTime = Random.Range(minDelay, maxDelay);
        
        _soundObjects[random].SetActive(false);
        _soundObjects[random].SetActive(true);
    }
}

