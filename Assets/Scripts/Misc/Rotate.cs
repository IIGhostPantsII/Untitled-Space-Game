using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Vector3 _moveSpeed;
    
    void Update() {
        transform.Rotate(_moveSpeed * Time.deltaTime);
    }
}
