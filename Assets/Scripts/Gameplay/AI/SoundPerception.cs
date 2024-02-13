using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoundPerception : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _threshold;

    private float distance;
    private float combined;

    private int counter;

    public void ActionPerformed(float value)
    {
        distance = Vector3.Distance(gameObject.transform.position, _playerTransform.position);
        Debug.Log("Distance between object1 and object2: " + distance);
        combined = distance / value;
        if(_threshold / 5 > combined && !Globals.ChaseMode)
        {
            Globals.ChangeMonsterState("Chase");
        }
        else if(_threshold > combined && !Globals.ChaseMode)
        {
            Globals.ChangeMonsterState("Patrol");
        }
    }

    public bool ChaseCheck()
    {
        if(_threshold / 2 > combined)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 RandomizedPlayerPos()
    {
        Vector3 randomPosition = new Vector3(Random.Range(_playerTransform.position.x - 10f, _playerTransform.position.x + 10f), 0.67f, Random.Range(_playerTransform.position.z - 10f, _playerTransform.position.z + 10f));
        
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            Vector3 newPosition = new Vector3(hit.position.x, 0.67f, hit.position.z);
            return newPosition;
        }
        else
        {
            counter++;
            if(counter > 99)
            {
                counter = 0;
                Debug.Log("Player Not In Walkable Area, Resetting...");
                Globals.ChangeMonsterState("Idle");
                return Vector3.zero;
            }
            return RandomizedPlayerPos();
        }
    }
}
