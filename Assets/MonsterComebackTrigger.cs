using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterComebackTrigger : MonoBehaviour
{
    [SerializeField] private float _maxWaitTime;

    private float _waitTimer;

    private void Update()
    {
        _waitTimer -= Time.deltaTime;
        
        if (_waitTimer > 0) return;

        GameObject[] monsterList = GameObject.FindGameObjectsWithTag("Monster");

        MonsterAI monster = monsterList[(int) Random.Range(0, monsterList.Length)].GetComponent<MonsterAI>();
        
        monster.Comeback();
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            _waitTimer = _maxWaitTime;
        }
    }
}
