using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlacement : MonoBehaviour
{
    [SerializeField] private Transform[] _placements;

    void Start()
    {
        int random = Random.Range(0, _placements.Length);
        
        transform.position = _placements[random].position;
        transform.rotation = _placements[random].rotation;
    }
}
