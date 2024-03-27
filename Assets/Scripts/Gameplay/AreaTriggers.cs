using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTriggers : MonoBehaviour
{
    [SerializeField] private float _xMinPos;
    [SerializeField] private float _xMaxPos;
    [SerializeField] private float _zMinPos;
    [SerializeField] private float _zMaxPos;

    [SerializeField] private Vector3[] _leavingPoints;

    [SerializeField] private bool _north = true;
    [SerializeField] private bool _south = true;
    [SerializeField] private bool _east = true;
    [SerializeField] private bool _west = true;
    
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

    public Vector3 Leave(int value)
    {
        int random = Random.Range(0, value);

        if(random == 0)
        {
            return _leavingPoints[random];
        }
        else if(random == 1)
        {
            return _leavingPoints[random];
        }
        else if(random == 2)
        {
            return _leavingPoints[random];
        }
        else
        {
            return _leavingPoints[0];
        }
    }

    public int PickDirection()
    {
        List<int> directions = new List<int>();

        if(_north)
        {
            directions.Add(0);
        }
        if(_south)
        {
            directions.Add(180);
        }
        if(_east)
        {
            directions.Add(90);
        }
        if(_west)
        {
            directions.Add(270);
        }

        if(directions.Count == 0)
        {
            return 0;
        }
        
        int randomIndex = UnityEngine.Random.Range(0, directions.Count);

        return directions[randomIndex];
    }
}
