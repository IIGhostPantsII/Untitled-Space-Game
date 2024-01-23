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
    private bool hitDelay;

    private int currentIndex = 0;
    private int idleChance = 20;

    //Note to self, change this logic later, it sucks but faster so can get everything done
    int hitCounter = 0;

    private float rotationTimer = 0f;
    private float tempMonsterSpeed = 0f;
    private float tempMonsterAcceleration = 0f;
    private float tempMonsterAniSpeed = 0f;
    private float timeOffset = 0f;

    private Quaternion initialRotation;

    private NavMeshAgent monsterPathing;

    void Start()
    {
        int spawnLength = _spawnPoints.Length;
        monsterPathing = GetComponent<NavMeshAgent>();
        Globals.ChangeMonsterState("Idle");

        hitCounter = 0;
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
                _monsterAni.speed = Mathf.Clamp(tempMonsterAniSpeed + ((newTime) * 0.18f), 0.33f, 5f);
                monsterPathing.speed = Mathf.Clamp(tempMonsterSpeed + ((newTime) * 2f), 5f, 50f);
                monsterPathing.acceleration = Mathf.Clamp(tempMonsterAcceleration - ((newTime) * 7f), 35f, 100f);
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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Globals.ChaseMode && !hitDelay)
        {
            if(hitCounter == 3)
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
