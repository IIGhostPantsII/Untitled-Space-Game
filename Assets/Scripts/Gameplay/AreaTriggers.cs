using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTriggers : MonoBehaviour
{
    [SerializeField] private Vector3[] _wonderingAreas;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Monster"))
        {
            GameObject monsterObject = other.gameObject;
            MonsterAI monsterAIScript = monsterObject.GetComponent<MonsterAI>();
            for(int i = 0; i < _wonderingAreas.Length; i++)
            {
                monsterAIScript._pointsOfInterest.Add(_wonderingAreas[i]);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
