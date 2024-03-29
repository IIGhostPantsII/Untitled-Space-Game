using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] private Vector3 _pos;

    public void Move()
    {
        gameObject.transform.position = _pos;
    }
}
