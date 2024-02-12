using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoicelineController : MonoBehaviour
{
    [SerializeField] private TMP_Text _subtitles;
    [SerializeField] private TextAsset _voicelineTSV;
    
    private bool _isPlaying;
    private GameObject _player;
    private Dictionary<string, ArrayList> _voicelines = new Dictionary<string, ArrayList>();
    private List<string> _usedVoiceLines = new List<string>();
    
    private void Start()
    {
        _voicelines = Globals.LoadTSV(_voicelineTSV);
        _player = GameObject.FindWithTag("MainCamera");
        _subtitles.SetText("");
    }
    
    [Button]
    void PlaySound()
    {
        System.Random rand = new System.Random();
        StartCoroutine(PlayVoiceline($"msg_descole_test{Random.Range(1, 8)}"));
    }

    public IEnumerator PlayVoiceline(string sound)
    {
        if (_isPlaying) yield break;
        if (_usedVoiceLines.Contains(sound))
        {
            Debug.LogError($"Voice line \"{sound}\" already played");
            yield break;
        }

        _usedVoiceLines.Add(sound);
        if (!_voicelines.ContainsKey(sound) && !_voicelines.ContainsKey(sound + "_1")) sound = "err_fake_voiceline";
        
        _isPlaying = true;
        
        FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/PA Jingles/PA-Normal-Opening");
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
                    _subtitles.SetText(_voicelines[tempSound][1].ToString());
                    RuntimeManager.StudioSystem.getEvent($"event:/Dialogue Sounds/Dialogue/{tempSound}", out var desc);
                    if (!desc.isValid())
                        tempSound = "err_not_recorded";

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
            _subtitles.SetText(_voicelines[sound][1].ToString());
        
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
        instance = RuntimeManager.CreateInstance($"event:/Dialogue Sounds/PA Jingles/PA-Normal-Closing");
        RuntimeManager.AttachInstanceToGameObject(instance, _player.transform, _player.GetComponent<Rigidbody>());
        instance.start();
        instance.release();

        instance.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPING && state != PLAYBACK_STATE.STOPPED)
        {
            yield return null;
            instance.getPlaybackState(out state);
        }

        _isPlaying = false;
    }
}
