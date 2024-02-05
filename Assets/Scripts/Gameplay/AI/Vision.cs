using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _monsterFOV = 75f;

    private float timeNotSeen;

    void Start()
    {
        timeNotSeen = 0f;
    }

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
                if(hit.collider.CompareTag("Player") && Globals.CheckMonsterState("Idle"))
                {
                    Globals.ChangeMonsterState("Chase");
                    Debug.Log("Player detected!");
                }
                else if(hit.collider.CompareTag("Player"))
                {
                    timeNotSeen = 0f;
                }
            }
        }
        else
        {
            if(Globals.CheckMonsterState("Chase") && timeNotSeen > 5f)
            {
                Globals.ChangeMonsterState("Idle");
                Debug.Log("Switching Back");
            }
            else if(Globals.CheckMonsterState("Chase"))
            {
                timeNotSeen += Time.deltaTime;
            }
            else
            {
                timeNotSeen = 0f;
            }
        }
    }
}