using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static bool Movement = true;

    public static bool GameIsActive;

    public static void LockMovement()
    {
        Movement = false;
    }

    public static void UnlockMovement()
    {
        Movement = true;
    }
}
