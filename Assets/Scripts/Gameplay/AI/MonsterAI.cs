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
    [SerializeField] private Animator _monsterAni;
    [SerializeField] private Animator _hit;

    private bool isIdling;
    private bool isMoving;
    private bool turnBack;
    private bool enteredChase;
    private bool enteredIdle;
    private bool hitDelay;

    private int currentIndex = 0;
    private int idleChance = 20;

    //Counters
    //Note to self, change this logic later, it sucks but faster so can get everything done
    private int hitCounter = 0;
    private int idleCounter = 0;

    private float rotationTimer = 0f;
    private float tempMonsterSpeed = 0f;
    private float tempMonsterAcceleration = 0f;
    private float tempMonsterAniSpeed = 0f;
    private float timeOffset = 0f;

    //Floats for IdleMode
    private float xMinPos;
    private float xMaxPos;
    private float zMinPos;
    private float zMaxPos;

    private Quaternion initialRotation;

    private NavMeshAgent monsterPathing;

    private AreaTriggers areaTrigger;

    private Vector3 randomPosition;

    void Start()
    {
        int spawnLength = _spawnPoints.Length;
        monsterPathing = GetComponent<NavMeshAgent>();
        Globals.ChangeMonsterState("Idle");
        enteredIdle = true;
    }

    void Update()
    {
        if(isIdling)
        {
            _monsterAni.Play("idle");
        }
        else if(isMoving)
        {
            _monsterAni.Play("walking");
        }

        if(Globals.IdleMode)
        {
            if(enteredIdle)
            {
                enteredIdle = false;
                RandomizeDestination();
            }

            // Check if the agent has reached its destination
            if(!monsterPathing.pathPending && monsterPathing.remainingDistance < 0.1f)
            {
                idleChance = Random.Range(0, 1);
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
                        RandomizeDestination();
                    }
                }
                else
                {
                    RandomizeDestination();
                }
            }
        }
        else if(Globals.ChaseMode)
        {
            if(!enteredChase)
            {
                isIdling = false;
                isMoving = false;
                _monsterAni.Play("roar");
                enteredChase = true;
                monsterPathing.speed = 15;
                tempMonsterSpeed = 15;
                tempMonsterAcceleration = 85;
                tempMonsterAniSpeed = 1;
                timeOffset = Time.time + 2.166f;
            }
            else if(Globals.AnimationOver)
            {
                float newTime = Time.time - timeOffset;
                monsterPathing.SetDestination(_playerTransform.position);
                //These numbers scare me I dont remember what they mean
                _monsterAni.speed = Mathf.Clamp(tempMonsterAniSpeed + ((newTime) * 0.18f), 0.33f, 5f);
                monsterPathing.speed = Mathf.Clamp(tempMonsterSpeed + ((newTime) * 2f), 5f, 50f);
                monsterPathing.acceleration = Mathf.Clamp(tempMonsterAcceleration - ((newTime) * 7f), 35f, 100f);
            } 
        }
    }
    
    void RandomizeDestination()
    {
        if(areaTrigger != null)
        {
            xMinPos = areaTrigger.GetPositions("xMin");
            xMaxPos = areaTrigger.GetPositions("xMax");
            zMinPos = areaTrigger.GetPositions("zMin");
            zMaxPos = areaTrigger.GetPositions("zMax");

            randomPosition = new Vector3(Random.Range(xMinPos, xMaxPos), 0.67f, Random.Range(zMinPos, zMaxPos));
            
            NavMeshHit hit;
            //If pos is on the navmesh
            if(NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                idleCounter++;
                isMoving = true;
                isIdling = false;
                //The Y position doesn't matter for the navmesh
                Vector3 newPosition = new Vector3(hit.position.x, 0.67f, hit.position.z);
                if(idleCounter > 2)
                {
                    Vector3 leaving = areaTrigger.Leave();
                    monsterPathing.SetDestination(leaving);
                }
                else
                {
                    monsterPathing.SetDestination(newPosition);
                }
            }
            else
            {
                RandomizeDestination();
            }
        }
        else
        {
            Debug.Log("Idle mode brokey");
        }
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

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && Globals.ChaseMode && !hitDelay)
        {
            if(hitCounter == 6 && Globals.GameState != GameState.Victory)
            {
                Globals.GameState = GameState.Lost;
            }
            else
            {
                hitDelay = true;
                _monsterAni.Play("swipe");
                monsterPathing.speed = 5f;
                tempMonsterSpeed = 5f;
                tempMonsterAcceleration = 100f;
                tempMonsterAniSpeed = 1f;
                timeOffset = Time.time;
                StartCoroutine(Hit());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("AreaTrigger"))
        {
            areaTrigger = other.GetComponent<AreaTriggers>();
        }
    }

    IEnumerator Hit()
    {
        hitCounter++;
        yield return new WaitForSeconds(0.5f);
        _hit.gameObject.SetActive(true);
        tempMonsterAniSpeed = 0.33f;
        yield return new WaitForSeconds(0.75f);
        _hit.gameObject.SetActive(false);
        hitDelay = false;
    }
}