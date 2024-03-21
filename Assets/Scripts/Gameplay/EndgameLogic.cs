using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameLogic : MonoBehaviour
{
    public static bool Started;

    void Start()
    {
        StartEndgame();
    }

    public void StartEndgame()
    {
        Started = true;
    }
}
