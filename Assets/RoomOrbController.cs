using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomOrbController : MonoBehaviour
{
    [SerializeField] private Color _filledColor;
    [SerializeField] public Image[] _rooms;
    [SerializeField] private bool[] _roomsOn;

    private void Start()
    {
        _roomsOn = new bool[_rooms.Length];
    }
    
    public void UpdateRoomInfo()
    {
        for (int i = 0; i < _rooms.Length; i++)
        {
            if (Globals.RoomPower[i] && !_roomsOn[i])
            {
                _roomsOn[i] = true;
                StartCoroutine(ChangeRoomColor(_rooms[i]));
            }
        }
    }

    private IEnumerator ChangeRoomColor(Image room)
    {
        float time = 0f;
        float duration = 0.5f;

        Color startColor = room.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            room.color = Color.Lerp(startColor, _filledColor, time / duration);
            yield return null;
        }

        room.color = _filledColor;
    }
}
