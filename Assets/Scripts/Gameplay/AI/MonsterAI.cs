using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [SerializeField] public List<Vector3> _pointsOfInterest;
    [SerializeField] public Vector3[] _spawnPoints;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _rotationSpeed = 8f;
    

    //Different AI Modes
    private bool idleMode = true;
    private bool patrolMode;
    private bool chaseMode;

    private int currentIndex = 0;

    void Start()
    {
        int spawnLength = _spawnPoints.Length;
        SpawnTheMonster(spawnLength);
    }

    void Update()
    {
        if(idleMode)
        {
            if (_pointsOfInterest.Count > 0)
            {
                MoveTowardsTarget(_pointsOfInterest[currentIndex]);

                if (Vector3.Distance(transform.position, _pointsOfInterest[currentIndex]) < 0.1f)
                {
                    currentIndex = (currentIndex + 1) % _pointsOfInterest.Count;
                }
            }
            else
            {
                Debug.LogError("No target pos");
            }
        }
    }

    void MoveTowardsTarget(Vector3 target)
    {
        float step = _speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        Vector3 direction = (target - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public void SpawnTheMonster(int chance)
    {
        int random = Random.Range(0, chance);
        gameObject.transform.position = _spawnPoints[random];
    }
}
