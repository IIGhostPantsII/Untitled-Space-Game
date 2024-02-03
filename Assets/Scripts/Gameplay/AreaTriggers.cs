using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTriggers : MonoBehaviour
{
    [SerializeField] private float _xMinPos;
    [SerializeField] private float _xMaxPos;
    [SerializeField] private float _zMinPos;
    [SerializeField] private float _zMaxPos;

    [SerializeField] private Vector3 _left;
    [SerializeField] private Vector3 _right;
    
    public float GetPositions(string name)
    {
        if(name == "xMin")
        {
            return _xMinPos;
        }
        else if(name == "xMax")
        {
            return _xMaxPos;
        }
        else if(name == "zMin")
        {
            return _zMinPos;
        }
        else if(name == "zMax")
        {
            return _zMaxPos;
        }
        else
        {
            Debug.Log("GETPOS IS INVALID");
            return 0f;
        }
    }

    public Vector3 Leave()
    {
        int random = Random.Range(0, 2);

        if(random == 0)
        {
            return _left;
        }
        else
        {
            return _right;
        }
    }
}
