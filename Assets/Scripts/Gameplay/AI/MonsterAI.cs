using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [SerializeField] public List<Vector3> _pointsOfInterest;
    [SerializeField] public Vector3[] _spawnPoints;
    [SerializeField] private float _rotationDuration = 3f;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _rotationSpeed = 8f;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private References _references;
    [SerializeField] private Animator monsterAni;

    private bool isIdling;
    private bool isMoving;
    private bool turnBack;
    private bool enteredChase;

    private int currentIndex = 0;
    private int idleChance = 20;

    private float rotationTimer = 0f;

    private Quaternion initialRotation;

    private NavMeshAgent monsterPathing;

    void Start()
    {
        int spawnLength = _spawnPoints.Length;
        monsterPathing = GetComponent<NavMeshAgent>();
        Globals.ChangeMonsterState("Idle");
        
    }

    void Update()
    {
        if(isIdling)
        {
            monsterAni.Play("idle");
        }
        else if(isMoving)
        {
            monsterAni.Play("walking");
        }

        if(Globals.IdleMode)
        {
            if(_pointsOfInterest.Count > 0 && currentIndex >= 0)
            {
                if(idleChance == 0)
                {
                    if(rotationTimer <= _rotationDuration)
                    {
                        isMoving = false;
                        isIdling = true;
                        MonsterIdle();
                        rotationTimer += Time.deltaTime;
                    }
                    else
                    {
                        isMoving = true;
                        isIdling = false;
                        idleChance = 20;
                        rotationTimer = 0f;
                    }
                }
                else
                {
                    if(isIdling)
                    {
                        //Turning the AI backwards is super mega broken rn, just ignore this bit
                        //int random = Random.Range(0, 20);
                        //int random = 1;
                        //if(random == 0)
                        //{
                        //    Debug.Log("THIS SHOULDNT HAPPEN WAIT WHAT?");
                        //    //turnBack = !turnBack;
                        //    if(turnBack)
                        //    {
                        //        currentIndex -= 2;
                        //    }
                        //    else
                        //    {
                        //        if((currentIndex + 3) < _pointsOfInterest.Count)
                        //        {
                        //            for(int i = _pointsOfInterest.Count - 1; i > (currentIndex + 3); i--)
                        //            {
                        //                _pointsOfInterest.RemoveAt(i);
                        //            }
                        //        }
                        //    }
                        //}
                        isIdling = false;
                    }

                    if(currentIndex >= 0)
                    {
                        MoveTowardsTarget(_pointsOfInterest[currentIndex]);
                    }

                    if(Vector3.Distance(transform.position, _pointsOfInterest[currentIndex]) < 0.1f)
                    {
                        idleChance = Random.Range(0, 15);
                        if(turnBack)
                        {
                            if(currentIndex > 0)
                            {
                                currentIndex = (currentIndex - 1);
                            }
                            else
                            {
                                currentIndex = 1;
                                //FOR THE FOR LOOP, I IS ONLY > 3 BECAUSE THE INITIAL SPAWN HAS 4 POINTS OF INTEREST, IF THIS CHANGES, ALSO CHANGE THE FOR LOOP
                                for(int i = _pointsOfInterest.Count - 1; i > 3; i--)
                                {
                                    _pointsOfInterest.RemoveAt(i);
                                }
                                _references.ResetAI();
                                turnBack = false;
                            }
                        }
                        else
                        {
                            if(currentIndex < _pointsOfInterest.Count - 1)
                            {
                                currentIndex = (currentIndex + 1);
                            }
                        }
                    }
                }
            }
            else
            {
                //Hard Reset
                isIdling = false;
                turnBack = false;
                _references.ResetAI();
                currentIndex = 0;
            }
        }
        else if(Globals.ChaseMode)
        {
            if(!enteredChase)
            {
                isIdling = false;
                isMoving = false;
                monsterAni.Play("roar");
                enteredChase = true;
            }
            else if(Globals.AnimationOver)
            {
                Debug.Log("Chasing");
                monsterPathing.SetDestination(_playerTransform.position);
            } 
        }
    }

    void MoveTowardsTarget(Vector3 target)
    {
        isMoving = true;
        float step = _speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        //To flip the direction, flip transform and target around
        Vector3 direction = (target - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public void SpawnTheMonster(int chance)
    {
        int random = Random.Range(0, chance);
        gameObject.transform.position = _spawnPoints[random];
    }
    
    void MonsterIdle()
    {
        if(rotationTimer == 0f)
        {
            initialRotation = transform.rotation;
        }

        Quaternion leftRotation = Quaternion.Euler(0f, -45f, 0f) * initialRotation;
        Quaternion rightRotation = Quaternion.Euler(0f, 45f, 0f) * initialRotation;

        float pauseDuration = 0.5f;
        float halfDuration = (_rotationDuration - (pauseDuration * 2f)) / 2f;

        if(rotationTimer < halfDuration)
        {
            float t = rotationTimer / halfDuration;
            transform.rotation = Quaternion.Lerp(initialRotation, leftRotation, t);
        }
        else if(rotationTimer > halfDuration + pauseDuration && rotationTimer < (_rotationDuration - (pauseDuration / 2f)))
        {
            float t = (rotationTimer - (halfDuration + pauseDuration)) / halfDuration;
            transform.rotation = Quaternion.Lerp(leftRotation, rightRotation, t);
        }
        else if(rotationTimer > halfDuration && rotationTimer < (_rotationDuration - (pauseDuration / 2f)))
        {
            transform.rotation = leftRotation;
        }
        else
        {
            transform.rotation = rightRotation;
        }
    }
}
