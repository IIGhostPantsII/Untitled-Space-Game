using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAndPlace : MonoBehaviour
{
    [SerializeField] private Transform[] _placedTransforms;
    [SerializeField] private GameObject[] _items;

    private List<GameObject> inactiveItems = new List<GameObject>();

    public void Pickup(int value)
    {
        _items[value].SetActive(false);
    }

    public void Place(int value)
    {
        gameObject.transform.position = _placedTransforms[value].position;
        gameObject.transform.rotation = _placedTransforms[value].rotation;
        _items[value].SetActive(true);
    }

    public List<GameObject> GetInactiveItems()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (!_items[i].activeSelf)
            {
                _inactiveItems.Add(_items[i]);
            }
        }
        return _inactiveItems;
    }
}
