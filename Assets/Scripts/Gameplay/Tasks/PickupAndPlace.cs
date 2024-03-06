using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAndPlace : MonoBehaviour
{
    [SerializeField] private Transform[] _placedTransforms;
    [SerializeField] private GameObject[] _items;

    private List<GameObject> inactiveItems = new List<GameObject>();
    private List<Transform> inactiveTransforms = new List<Transform>();

    [HideInInspector] public int counter = 0;

    public void Pickup(int value)
    {
        _items[value].SetActive(false);
    }

    public void Place()
    {
        GetInactiveItems();
        TurnOn();
    }

    public void GetInactiveItems()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (!_items[i].activeSelf)
            {
                inactiveItems.Add(_items[i]);
                inactiveTransforms.Add(_placedTransforms[i]);
            }
        }
    }

    public void TurnOn()
    {
        for(int i = 0; i < inactiveItems.Count; i++)
        {
            counter++;

            inactiveItems[i].transform.position = inactiveTransforms[i].position;
            inactiveItems[i].transform.rotation = inactiveTransforms[i].rotation;
            inactiveItems[i].SetActive(true);
        }
    }
}
