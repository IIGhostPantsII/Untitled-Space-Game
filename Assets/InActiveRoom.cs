using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class InActiveRoom : MonoBehaviour
{
    private Material _mat;

    private bool _inRoom;
    
    void Start()
    {
        _mat = new Material(GetComponent<Image>().material);
        GetComponent<Image>().material = _mat;
    }

    [Button]
    public void ToggleEnterRoom()
    {
        StartCoroutine(ToggleRoom(!_inRoom));
    }
    
    public void ToggleEnterRoom(bool inRoom)
    {
        StartCoroutine(ToggleRoom(inRoom));
    }

    private IEnumerator ToggleRoom(bool inRoom)
    {
        float time = 0;
        float duration = 0.1f;

        _inRoom = inRoom;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            
            if (inRoom) _mat.SetFloat("_OutlineThickness", Mathf.Lerp(50, 72, time / duration));
            else _mat.SetFloat("_OutlineThickness", Mathf.Lerp(72, 50, time / duration));

            yield return null;
        }
        
        if (inRoom) _mat.SetFloat("_OutlineThickness", 72);
        else _mat.SetFloat("_OutlineThickness", 50);
    }
}
