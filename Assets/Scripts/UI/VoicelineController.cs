using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class VoicelineController : MonoBehaviour
{
    [SerializeField] private TMP_Text _subtitles;
    [SerializeField] private TextAsset _voicelineTSV;
    
    private bool _isPlaying;
    private GameObject _player;
    private Dictionary<string, ArrayList> _voicelines = new Dictionary<string, ArrayList>();
    private List<string> _usedVoiceLines = new List<string>();

    private bool _walkieTalkie = true;

    private void Start()
    {
        _voicelines = Globals.LoadTSV(_voicelineTSV);
        _player = GameObject.FindWithTag("MainCamera");
        _subtitles.SetText("");
        TogglePAMode(_walkieTalkie);
    }

    [Button]
    public void TogglePAMode()
    {
        TogglePAMode(!_walkieTalkie);
    }
    
    public void TogglePAMode(bool PAMode)
    {
        _walkieTalkie = PAMode;

        RuntimeManager.StudioSystem.setParameterByName("Walkie-Talkie", PAMode ? 1 : 0);
    }

    private void Update() {
        if (Keyboard.current[Key.T].wasPressedThisFrame) {
            PlaySound($"msg_descole_test{Random.Range(1, 9)}");
        }
    }

    public bool PlaySound(string sound)
    {
        System.Random rand = new System.Random();

        bool canPlay = !_usedVoiceLines.Contains(sound);

        StartCoroutine(PlayVoiceline(sound));

        return canPlay;
    }

    private IEnumerator PlayVoiceline(string sound)
    {
        if (_isPlaying) yield break;
        if (_usedVoiceLines.Contains(sound))
        {
            Debug.LogError($"Voice line \"{sound}\" already played");
            yield break;
        }

        bool fakeLine = false;
        string fakeSound = "The error detection is broken somehow :/";
        _usedVoiceLines.Add(sound);

        if (!_voicelines.ContainsKey(sound) && !_voicelines.ContainsKey(sound + "_1"))
        {
            fakeLine = true;
            fakeSound = sound;
            sound = "err_fake_voiceline";
        }
        
        _isPlaying = true;
        
        EventInstance instance;
        
        if (_walkieTalkie) instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/PA Jingles/PA-Portable-Opening");
        else instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/PA Jingles/PA-Normal-Opening");
        
        RuntimeManager.AttachInstanceToGameObject(instance, _player.transform, _player.GetComponent<Rigidbody>());
        instance.start();
        instance.release();

        instance.getPlaybackState(out var state);
        while (state != PLAYBACK_STATE.STOPPING && state != PLAYBACK_STATE.STOPPED)
        {
            yield return null;
            instance.getPlaybackState(out state);
        }

        List<string> voiceLineSplit = sound.Split("_").ToList();
        bool multipleVoiceClips = false;
        int currentLine = 1;

        if (_voicelines.ContainsKey(sound + "_1")) multipleVoiceClips = true;

        try
        {
            currentLine = Int32.Parse(voiceLineSplit[^1]);
            multipleVoiceClips = true;
            voiceLineSplit.RemoveAt(voiceLineSplit.Count - 1);

            sound = string.Join("_", voiceLineSplit);
            Debug.Log(sound);
        } catch {}

        if (multipleVoiceClips)
        {
            while (true)
            {
                int voicelineMilis = 0;
                string tempSound = sound + "_" + currentLine;
                
                try
                {
                    if (!string.IsNullOrEmpty(_voicelines[tempSound][2].ToString()))
                    {
                        _subtitles.SetText("[" + _voicelines[tempSound][2] + "] " + _voicelines[tempSound][1].ToString());
                    }
                    else
                    {
                        _subtitles.SetText(_voicelines[tempSound][1].ToString());
                    }

                    if (!Globals.StoryFlags.Contains(_voicelines[tempSound][3])) Globals.StoryFlags.Add((string) _voicelines[tempSound][3]);

                    RuntimeManager.StudioSystem.getEvent($"event:/Dialogue Sounds/Dialogue/{tempSound}", out var desc);
                    if (!desc.isValid())
                    {
                        tempSound = "err_not_recorded";
                        RuntimeManager.StudioSystem.getEvent($"event:/Dialogue Sounds/Dialogue/{tempSound}", out desc);
                    }
                    desc.getLength(out voicelineMilis);
                    
                    instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/Dialogue/{tempSound}");
                    RuntimeManager.AttachInstanceToGameObject(instance, _player.transform, _player.GetComponent<Rigidbody>());
                    instance.start();
                    instance.release();
                }
                catch
                {
                    goto DoneVoiceLine;
                }
                
                yield return new WaitForSeconds(0.075f);
        
                RuntimeManager.PlayOneShotAttached($"event:/Dialogue Sounds/Dialogue/{tempSound}",
                    _player);

                yield return new WaitForSeconds(Mathf.Max((voicelineMilis / 1000f) - 0.075f, 0));

                currentLine += 1;
            }
        }
        else
        {
            if (fakeLine)
            {
                if (!string.IsNullOrEmpty(_voicelines[sound][2].ToString()))
                {
                    _subtitles.SetText("[" + _voicelines[sound][2] + "] " + _voicelines[sound][1].ToString() +
                                       "\n(Failed to load voiceline <color=red>" + fakeSound + "</color>)");
                }
                else
                {
                    _subtitles.SetText(_voicelines[sound][1].ToString() +
                                       "\n(Failed to load voiceline <color=red>" + fakeSound + "</color>)");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_voicelines[sound][2].ToString()))
                {
                    _subtitles.SetText("[" + _voicelines[sound][2] + "] " + _voicelines[sound][1].ToString());
                }
                else
                {
                    _subtitles.SetText(_voicelines[sound][1].ToString());
                }
            }
            
            if (!Globals.StoryFlags.Contains(_voicelines[sound][3])) Globals.StoryFlags.Add((string) _voicelines[sound][3]);
        
            RuntimeManager.StudioSystem.getEvent($"event:/Dialogue Sounds/Dialogue/{sound}", out var desc);
            if (!desc.isValid())
                sound = "err_not_recorded";
            
            RuntimeManager.PlayOneShotAttached($"event:/Dialogue Sounds/Dialogue/{sound}", _player);
            yield return new WaitForSeconds(0.075f);
        
            instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/Dialogue/{sound}");
            RuntimeManager.AttachInstanceToGameObject(instance, _player.transform, _player.GetComponent<Rigidbody>());
            instance.start();
            instance.release();

            instance.getPlaybackState(out state);
            while (state != PLAYBACK_STATE.STOPPING && state != PLAYBACK_STATE.STOPPED)
            {
                yield return null;
                instance.getPlaybackState(out state);
            }
        }

        DoneVoiceLine:
        yield return new WaitForSeconds(0.75f);
        
        _subtitles.SetText("");
        
        if (_walkieTalkie) instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/PA Jingles/PA-Portable-Closing");
        else instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/PA Jingles/PA-Normal-Closing");
        
        RuntimeManager.AttachInstanceToGameObject(instance, _player.transform, _player.GetComponent<Rigidbody>());
        instance.start();
        instance.release();

        instance.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPING && state != PLAYBACK_STATE.STOPPED)
        {
            yield return null;
            instance.getPlaybackState(out state);
        }

        foreach (string flag in Globals.StoryFlags)
        {
            Debug.Log(flag);
        }
        _isPlaying = false;
    }
}
