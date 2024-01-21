using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _monsterFOV = 75f;

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if(angleToPlayer < _monsterFOV / 2f)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, directionToPlayer, out hit, 100000000f))
            {
                if(hit.collider.CompareTag("Player"))
                {
                    Globals.ChangeMonsterState("Chase");
                    Debug.Log("Player detected!");
                }
            }
        }
    }
}