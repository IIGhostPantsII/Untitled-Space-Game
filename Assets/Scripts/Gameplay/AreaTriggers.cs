using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTriggers : MonoBehaviour
{
    [SerializeField] private Vector3[] _wonderingAreasLeft;
    [SerializeField] private Vector3[] _wonderingAreasRight;

    [SerializeField] private GameObject[] _otherAreaTriggers;

    [SerializeField] private References _references;
 
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Monster"))
        {
            if(_references != null && !Globals.GetOriginalTriggers)
            {
                for(int i = 0; i < _otherAreaTriggers.Length; i++)
                {
                    _references.AddTriggers(_otherAreaTriggers[i]);
                }

                Globals.OriginalTriggersCompleted();
            }
            GameObject monsterObject = other.gameObject;
            MonsterAI monsterAIScript = monsterObject.GetComponent<MonsterAI>();
            int random = Random.Range(0, 2);
            //if random == 0
            if(true)
            {
                for(int i = 0; i < _wonderingAreasLeft.Length; i++)
                {
                    monsterAIScript._pointsOfInterest.Add(_wonderingAreasLeft[i]);
                }
            }
            else
            {
                for(int i = 0; i < _wonderingAreasRight.Length; i++)
                {
                    monsterAIScript._pointsOfInterest.Add(_wonderingAreasRight[i]);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Monster"))
        {
            for(int j = 0; j < _otherAreaTriggers.Length; j++)
            {
                _otherAreaTriggers[j].SetActive(false);
            }
        }
    }
}
