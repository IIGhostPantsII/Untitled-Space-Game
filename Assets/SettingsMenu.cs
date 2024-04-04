using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
   private PlayerInput _playerInput;
   private string _prevInputMap;

   [SerializeField] private TMP_Dropdown _resolutionDropdown;
   [SerializeField] private Toggle _fullscreenToggle;
   [SerializeField] private Toggle _subtitleToggle;
   [SerializeField] private Toggle _altColorToggle;
   [SerializeField] private Toggle _crosshairToggle;
   [SerializeField] private Slider _mouseSensitivitySlider;
   [SerializeField] private Slider _brightnessSlider;
   
   [Space]
   [SerializeField] private Volume _lowBrightnessVolume;
   [SerializeField] private Volume _highBrightnessVolume;
   [SerializeField] private GameObject _crossHair;
   [SerializeField] private GameObject _subtitles;

   private SettingsDataFormatter _settings;

   private void OnEnable()
   {
       LoadSettingsFromJson();
       
      _resolutionDropdown.SetValueWithoutNotify(Globals.CurrentResolution);

      _fullscreenToggle.SetIsOnWithoutNotify(Globals.IsFullscreen);
      _subtitleToggle.SetIsOnWithoutNotify(Globals.SubtitlesOn);
      _altColorToggle.SetIsOnWithoutNotify(Globals.AltDoorColors);
      _crosshairToggle.SetIsOnWithoutNotify(Globals.CrosshairOn);

      _mouseSensitivitySlider.value = Globals.MouseSensitivity;
      _brightnessSlider.value = Globals.Gamma;
   }

   private void OnDisable()
   {
      Globals.CurrentResolution = _resolutionDropdown.value;
      Globals.IsFullscreen = _fullscreenToggle.isOn;
      Globals.SubtitlesOn = _subtitleToggle.isOn;
      Globals.AltDoorColors = _altColorToggle.isOn;
      Globals.CrosshairOn = _crosshairToggle.isOn;
      Globals.MouseSensitivity = _mouseSensitivitySlider.value;
      Globals.Gamma = _brightnessSlider.value;
      
      SaveSettingsIntoJson();
      LoadSettingsFromJson();
   }
   
   public void SaveSettingsIntoJson()
   {
        _settings = new SettingsDataFormatter();

        _settings.CurrentResolution = Globals.CurrentResolution;
        _settings.IsFullscreen = Globals.IsFullscreen;
        _settings.SubtitlesOn = Globals.SubtitlesOn;
        _settings.AltDoorColors = Globals.AltDoorColors;
        _settings.CrosshairOn = Globals.CrosshairOn;
        _settings.MouseSensitivity = Globals.MouseSensitivity;
        _settings.Gamma = Globals.Gamma;

        string jason = JsonUtility.ToJson(_settings, true);
        
        if (!Directory.Exists(Globals.SaveDataPath))
        {
            Directory.CreateDirectory(Globals.SaveDataPath);
        }
        
        File.WriteAllText(Globals.SaveDataPath + "/OPTIONS.oae", jason);
   }

    public void LoadSettingsFromJson(bool updateWindow = true)
    {
        if (!File.Exists(Globals.SaveDataPath + "/OPTIONS.oae"))
        {
            SaveSettingsIntoJson();
        }
        
        string jason = File.ReadAllText(Globals.SaveDataPath + $"/OPTIONS.oae");
        _settings = JsonUtility.FromJson<SettingsDataFormatter>(jason);

        Globals.CurrentResolution = _settings.CurrentResolution;
        Globals.IsFullscreen = _settings.IsFullscreen;
        Globals.SubtitlesOn = _settings.SubtitlesOn;
        Globals.AltDoorColors = _settings.AltDoorColors;
        Globals.CrosshairOn = _settings.CrosshairOn;
        Globals.MouseSensitivity = _settings.MouseSensitivity;
        Globals.Gamma = _settings.Gamma;

        List<int> resolution = Globals.Resolutions[_settings.CurrentResolution];
        if (updateWindow) Screen.SetResolution(resolution[0], resolution[1], _settings.IsFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        if (Globals.Gamma > 0)
        {
            _highBrightnessVolume.weight = Mathf.Abs(Globals.Gamma);
            _lowBrightnessVolume.weight = Mathf.Abs(0);
        }
        else if (Globals.Gamma < 0)
        {
            _highBrightnessVolume.weight = Mathf.Abs(0);
            _lowBrightnessVolume.weight = Mathf.Abs(Globals.Gamma);
        }
        else
        {
            _highBrightnessVolume.weight = Mathf.Abs(0);
            _lowBrightnessVolume.weight = Mathf.Abs(0);
        }
        
        if (_crossHair != null) _crossHair.SetActive(Globals.CrosshairOn);
        if (_subtitles != null) _subtitles.SetActive(Globals.SubtitlesOn);

        foreach (Door door in FindObjectsOfType<Door>(true))
        {
            door.UpdateLockedLights(true);
        }
    }
}

[Serializable]
public class SettingsDataFormatter
{
    public int CurrentResolution;
    public bool IsFullscreen;
    public bool SubtitlesOn;
    public bool CrosshairOn;
    public bool AltDoorColors;
    public float Volume;
    public float MouseSensitivity;
    public float Gamma;
}
