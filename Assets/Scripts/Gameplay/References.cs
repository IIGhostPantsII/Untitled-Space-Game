using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class References : MonoBehaviour
{
    [SerializeField] public GameObject[] _areaTriggers;
    [SerializeField] public List<GameObject> _originalTriggers;

    public static GameObject[] EverySingleAreaTrigger;

    void Start()
    {
        EverySingleAreaTrigger = new GameObject[_areaTriggers.Length];
        for(int i = 0; i < _areaTriggers.Length; i++)
        {
            EverySingleAreaTrigger[i] = _areaTriggers[i];
        }
    }

    public void AddTriggers(GameObject originalTrigger)
    {
        _originalTriggers.Add(originalTrigger);
    }

    public void ResetAI()
    {
        for(int i = 0; i < _originalTriggers.Count; i++)
        {
            _originalTriggers[i].SetActive(false);
        }
    }

    public static void TurnOnAllAreaTriggers()
    {
        for(int i = 0; i < EverySingleAreaTrigger.Length; i++)
        {
            EverySingleAreaTrigger[i].SetActive(true);
        }
    }
}
