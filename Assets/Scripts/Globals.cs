using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static bool Movement = true;
    public static bool GameIsActive;
    public static bool Paused;
    public static bool GetOriginalTriggers;

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
}
