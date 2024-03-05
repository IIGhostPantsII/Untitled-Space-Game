using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _monsterFOV = 75f;

    private float timeNotSeen;

    private SoundPerception _sound;

    void Start()
    {
        timeNotSeen = 0f;
        _sound = GetComponent<SoundPerception>();
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
                if(hit.collider.CompareTag("Player") && Globals.CheckMonsterState("Idle") || hit.collider.CompareTag("Player") && Globals.CheckMonsterState("Patrol"))
                {
                    Globals.ChangeMonsterState("Chase");
                }
                else if(hit.collider.CompareTag("Player"))
                {
                    timeNotSeen = 0f;
                }
            }
        }
        else
        {
            if(Globals.CheckMonsterState("Chase") && timeNotSeen > 5f && !_sound.ChaseCheck())
            {
                Globals.ChangeMonsterState("Idle");
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