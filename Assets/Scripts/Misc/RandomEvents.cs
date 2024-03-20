using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class RandomEvents : MonoBehaviour
{
    [SerializeField] public EventType _eventType;
    [SerializeField] public int _odds;

    [ShowIf("_eventType", EventType.Move)] [AllowNesting] [SerializeField] public bool _zAxis;
    [ShowIf("_eventType", EventType.Move)] [AllowNesting] [SerializeField] public float _distance;

    [ShowIf("_eventType", EventType.Reposition)] [AllowNesting] [SerializeField] public Vector3 _pos;
    [ShowIf("_eventType", EventType.Reposition)] [AllowNesting] [SerializeField] public Quaternion _rotation;

    void Start()
    {
        int random = Random.Range(0, _odds);
        if(random == _odds - 1)
        {
            if(_eventType == EventType.Move)
            {
                Debug.Log(random);
                if(_zAxis)
                {
                    Vector3 newPosition = transform.position;
                    newPosition.z += _distance;
                    transform.position = newPosition;
                }
                else
                {
                    Vector3 newPosition = transform.position;
                    newPosition.x += _distance;
                    transform.position = newPosition;
                }
            }
            else if(_eventType == EventType.Reposition)
            {
                gameObject.transform.position = _pos;
                gameObject.transform.rotation = _rotation;
            }
        }
    }
}

public enum EventType
{
    Move,
    Reposition
}