using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterComebackTrigger : MonoBehaviour
{
    [SerializeField] private float _maxWaitTime;
    [SerializeField] private MonsterAI[] _monsters;

    private float _waitTimer;

    private void Start()
    {
        _waitTimer = _maxWaitTime;
        
        _monsters[Random.Range(0, _monsters.Length)].gameObject.SetActive(true);
    }

    private void Update()
    {
        _waitTimer -= Time.deltaTime;
        
        if (_waitTimer > 0) return;
        
        MonsterAI monster = _monsters[(int) Random.Range(0, _monsters.Length)];
        
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
