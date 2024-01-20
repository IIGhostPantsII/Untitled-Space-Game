using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static bool Movement = true;
    public static bool GameIsActive;
    public static bool Paused;
    public static bool GetOriginalTriggers;
    public static GameState GameState = GameState.Main;

    //Monster Modes
    public static bool ChaseMode;
    public static bool IdleMode;
    public static bool PatrolMode;

    public static void LockMovement()
    {
        Movement = false;
    }

    public static void UnlockMovement()
    {
        Movement = true;
    }

    public static void ResetGame()
    {
        //Turn off all Bools that get turned on mid-game
    }

    public static void OriginalTriggersCompleted()
    {
        GetOriginalTriggers = true;
    }

    public static void ChangeMonsterState(string Mode)
    {
        if(Mode == "Chase")
        {
            ChaseMode = true;
            IdleMode = false;
            PatrolMode = false;
        }
        else if(Mode == "Idle")
        {
            ChaseMode = false;
            IdleMode = true;
            PatrolMode = false;
        }
        else if(Mode == "Patrol")
        {
            ChaseMode = false;
            IdleMode = false;
            PatrolMode = true;
        }
    }
}

public enum GameState
{
    Main,
    Paused,
    Lost,
    Victory
}
