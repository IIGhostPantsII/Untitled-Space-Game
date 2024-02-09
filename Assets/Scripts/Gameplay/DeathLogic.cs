using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathLogic : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private GameObject _moleRat;
    [SerializeField] private GameObject _playerModel;

    private bool dead;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(MonsterAI.MoleRatDead && !dead)
        {
            dead = true;
            _playerModel.SetActive(false);
            _moleRat.SetActive(true);
            animator = _moleRat.GetComponent<Animator>();
            animator.Play("biteDeath");
        }
    }
}
