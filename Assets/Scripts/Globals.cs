using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public static class Globals
{
    public static bool Movement = true;
    public static bool GameIsActive;
    public static bool Paused;
    public static bool AnimationOver;
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

    public static void FinishedAnimation()
    {
        AnimationOver = true;
    }

    public static void ResetAnimation()
    {
        AnimationOver = false;
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

    public static bool CheckMonsterState(string Mode)
    {
        if(Mode == "Chase")
        {
            return ChaseMode;
        }
        else if(Mode == "Idle")
        {
            return IdleMode;
        }
        else if(Mode == "Patrol")
        {
            return PatrolMode;
        }
        else
        {
            Debug.Log("ERROR WHEN CHECKING MONSTER STATE, RETURNING FALSE");
            return false;
        }
    }

    public static void SpatialSounds(FMOD.Studio.EventInstance eventInstance, GameObject pos)
    {
        if(eventInstance.isValid())
        {
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(pos.transform));
        }
    }

    public static void CheckLowpass(FMOD.Studio.EventInstance eventInstance, Oxygen subOxy)
    {
        if(Oxygen.NoSprint)
        {
            eventInstance.setParameterByName("Lowpass",(subOxy._oxygenMeter * 220));
        }
        else
        {
            eventInstance.setParameterByName("Lowpass",22000);
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
