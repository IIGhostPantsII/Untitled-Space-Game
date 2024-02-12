using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoundPerception : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _threshold;

    private float distance;

    public void ActionPerformed(float value)
    {
        distance = Vector3.Distance(gameObject.transform.position, _playerTransform.position);
        Debug.Log("Distance between object1 and object2: " + distance);
        float combined = distance / value;
        Debug.Log(combined);
        if(_threshold / 5 > combined && !Globals.ChaseMode)
        {
            Globals.ChangeMonsterState("Chase");
        }
        else if(_threshold > combined && !Globals.ChaseMode)
        {
            Globals.ChangeMonsterState("Patrol");
        }
    }

    public Vector3 RandomizedPlayerPos()
    {
        Vector3 randomPosition = new Vector3(Random.Range(_playerTransform.position.x - 10f, _playerTransform.position.x + 10f), 0.67f, Random.Range(_playerTransform.position.z - 10f, _playerTransform.position.z + 10f));
        
        NavMeshHit hit;
        // If pos is on the navmesh
        if(NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            // The Y position doesn't matter for the navmesh
            Vector3 newPosition = new Vector3(hit.position.x, 0.67f, hit.position.z);
            return newPosition;
        }
        else
        {
            // Return the result of the recursive call
            return RandomizedPlayerPos();
        }
    }
}
