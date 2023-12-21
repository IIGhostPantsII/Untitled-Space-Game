using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class RandomChatter : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference[] _dialogue;
    [SerializeField] private FMODUnity.EventReference[] _PASoundPath;
    
    private Speakers[] _speakers;

    private float minDelay = 17.5f;
    private float maxDelay = 20f;

    int random;

    void Start()
    {
        GameObject[] objectsFound = FindObjectsByTag("Speakers");
        _speakers = new Speakers[objectsFound.Length];
        for(int i = 0; i < objectsFound.Length; i++)
        {
            _speakers[i] = objectsFound[i].GetComponent<Speakers>();
        }
        StartCoroutine(RandomDialogueRoutine());
    }

    public GameObject[] FindObjectsByTag(string tag)
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
        return foundObjects;
    }

    IEnumerator RandomDialogueRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            SetRandomPA();
            //yield return new WaitForSeconds(3f);
            //PlayRandomDialogue();
        }
    }

    void SetRandomPA()
    {
        random = Random.Range(0,300);
        if(random == 1)
        {
            RunAllSpeakers(_PASoundPath[2], _PASoundPath.Length, true);
        }
        else if(random > 1 && random < 16)
        {
            RunAllSpeakers(_PASoundPath[1], _PASoundPath.Length, true);
        }
        else
        {
            RunAllSpeakers(_PASoundPath[0], _PASoundPath.Length, true);
        }
    }

    void PlayRandomDialogue()
    {
        if(_dialogue.Length > 0)
        {
            int randomIndex = Random.Range(0, _dialogue.Length);
            RunAllSpeakers(_dialogue[randomIndex], _dialogue.Length, true);
        }
    }

    void RunAllSpeakers(FMODUnity.EventReference reference, int length, bool afterEffect)
    {
        for(int i = 0; i < length; i++)
        {
            _speakers[i].PlaySound(reference, afterEffect);
        }
    }

    public void GoAgain()
    {
        Debug.Log("Went Again");
        if(random == 1)
        {
            RunAllSpeakers(_PASoundPath[2], _PASoundPath.Length, false);
        }
        else if(random > 1 && random < 16)
        {
            RunAllSpeakers(_PASoundPath[1], _PASoundPath.Length, false);
        }
        else
        {
            RunAllSpeakers(_PASoundPath[0], _PASoundPath.Length, false);
        }
    }
}
