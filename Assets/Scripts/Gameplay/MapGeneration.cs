using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    [SerializeField] private GameObject[] _areas;

    [SerializeField] private Vector3[] _areaPosition;
    [SerializeField] private Quaternion[] _areaRotation;

    void Awake()
    {
        List<int> usedIndices = new List<int>();

        //Just cuz the rooms object is fucked, if I fix rooms then I can get rid of this
        for(int i = 0; i < _areaPosition.Length; i++)
        {
            _areaPosition[i].y += 5.69f;
            _areaPosition[i].z += 559.6f;
        }

        for(int j = 0; j < _areaPosition.Length; j++)
        {
            int random = Random.Range(0, _areaPosition.Length);

            do
            {
                Debug.Log("Same number, randomizing again");
                random = Random.Range(0, _areaPosition.Length);
            } 
            while(usedIndices.Contains(random));

            Debug.Log($"Got {random}, placing caf in this pos {_areaPosition[random]}");

            _areas[j].transform.position = _areaPosition[random];
            _areas[j].transform.rotation = _areaRotation[random];

            usedIndices.Add(random);
        }
    }
}
