using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenController : MonoBehaviour
{
    public string SceneName;
    [SerializeField] private Image _fadeOut;
    [SerializeField] private float _fadeOutSpeed;
    [SerializeField] private FMOD.Studio.EventInstance fmodInstance;
    [SerializeField] public FMODUnity.EventReference fmodEvent;
    
    void Start() 
    {
        fmodInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        fmodInstance.start();
        fmodInstance.setParameterByName("Title Screen Volume", 1, true);
    }
    
    public void StartGame()
    {
        StartCoroutine(NewGame());
    }

    private IEnumerator NewGame()
    {
        _fadeOut.color = Color.clear;
        _fadeOut.gameObject.SetActive(true);
        fmodInstance.setParameterByName("Title Screen Volume", 0);

        try
        {
            GetComponent<Animator>().enabled = false;
        }
        catch
        {
            // Ignored
        }

        float time = 0;
        float duration = _fadeOutSpeed;

        while (time < duration)
        {
            time += Time.deltaTime;
            _fadeOut.color = Color.Lerp(Color.clear, Color.black, time / duration);
            yield return null;
        }
        
        _fadeOut.color = Color.black;

        yield return new WaitForSeconds(1);

        fmodInstance.stop(STOP_MODE.ALLOWFADEOUT);
        SceneManager.LoadScene(SceneName);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
