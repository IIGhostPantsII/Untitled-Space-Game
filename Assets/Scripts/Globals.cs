using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;

public static class Globals
{
    public static bool Movement = true;
    public static bool MonsterMovement = true;
    public static bool GameIsActive;
    public static bool Paused;
    public static bool AnimationOver;
    public static bool GetOriginalTriggers;
    public static GameState GameState = GameState.Main;

    public static List<RoomTasks> RoomTasks = new List<RoomTasks>();
    public static List<string> StoryFlags = new List<string>();
    public static bool[] RoomPower = new bool[6];
    
    public static readonly string SaveDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Occupational Hazards ~ The Trials and Tribulations of the Stellar United Systems";
    //Monster Modes
    public static bool ChaseMode;
    public static bool IdleMode = true;
    public static bool PatrolMode;

    public static int Money;

    public static void LockMovement()
    {
        Movement = false;
    }

    public static void UnlockMovement()
    {
        Movement = true;
    }

    public static void LockMonsterMovement()
    {
        MonsterMovement = false;
    }

    public static void UnlockMonsterMovement()
    {
        MonsterMovement = true;
    }

    public static void ResetGame(bool isGameOver)
    {
        //Turn off all Bools that get turned on mid-game
        if(isGameOver)
        {
            Money = 0;
        }
        PatrolMode = false;
        IdleMode = true;
        ChaseMode = false;
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
    
    public static bool CheckStoryFlags(string[] flags)
    {
        if (flags == null) return true;
        if (flags.Length == 0) return true; 
        bool[] bools = new bool[flags.Length];
        for (int i = 0; i < flags.Length; i++)
        {
            bools[i] = false;
            string flag = flags[i];
            bool not = false;
                
            if (flag[0] == '!')
            {
                not = true;
                flag = flag.Remove(0, 1);
            }

            if (not)
            {
                if (!StoryFlags.Contains(flag))
                {
                    bools[i] = true;
                }  
            }
            else if (StoryFlags.Contains(flag))
            {
                bools[i] = true;
            }
        }

        foreach (bool bowl in bools)
        {
            if (!bowl) return false;
        }
        
        return true;
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
        if(eventInstance.isValid())
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

    public static void AddMoney(int value)
    {
        Money += value;
    }

    public static void LoseMoney(int value)
    {
        Money -= value;
    }
    
    public static Dictionary<string, ArrayList> LoadTSV(TextAsset file) {
        Dictionary<string, ArrayList> dictionary = new Dictionary<string, ArrayList>();
        ArrayList list = new ArrayList();
        
        var content = file.text;
        var lines = content.Split(System.Environment.NewLine);

        for (int i=0; i < lines.Length; i++)
        {
            list = new ArrayList();
            var line = lines[i];
            if (string.IsNullOrEmpty(line)) continue;
            string[] values = line.Split('	');
            for (int j=1; j < values.Length; j++) {
                list.Add(values[j]);
            }

            values[0] = new string(values[0].Where(c => !char.IsControl(c)).ToArray());
            if (values[0] != "") dictionary.Add(values[0], list);
        }

        return dictionary;
    }
    
    public static void Screenshot()
    {
        string folderPath = Globals.SaveDataPath + "/Screenshots/";
        string screenshotName =
            "Screenshot " + System.DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd-HHmmss") + ".png";
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName));
    }
}

public enum GameState
{
    Main,
    Paused,
    Lost,
    Victory
}

[Serializable]
public struct RoomTasks
{
    public string RoomName;
    public Task[] Tasks;
}

[Serializable]
public struct Task
{
    public string TaskName;
    public int TotalSteps;
    public int CompletedSteps;
}
