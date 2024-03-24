using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameLogic : MonoBehaviour
{
    public static bool Started;
    public static bool CanEndGame;

    public static GameObject[] Monsters;
    public static GameObject EndgameCam;
    public static Animator DonutAni;

    [SerializeField] public GameObject[] _monsters;
    [SerializeField] private GameObject _cam;
    [SerializeField] private Animator _donutAni;

    void Start()
    {
        Monsters = new GameObject[_monsters.Length];
        EndgameCam = _cam;
        DonutAni = _donutAni;
        StartEndgame();
        for(int i = 0; i < _monsters.Length; i++)
        {
            Monsters[i] = _monsters[i];
        }
    }

    public static void StartEndgame()
    {
        Started = true;
        Globals.ChangeMonsterState("Chase");
    }

    public static void EndGame()
    {
        for(int i = 0; i < Monsters.Length; i++)
        {
            Monsters[i].SetActive(false);
        }
        EndgameCam.SetActive(true);
        DonutAni.Play("launch");
        Globals.LockMovement();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Monster"))
        {
            CanEndGame = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Monster"))
        {
            CanEndGame = false;
        }
    }
}
