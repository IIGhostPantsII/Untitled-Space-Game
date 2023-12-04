using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TwoDimensionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _2DCam;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _2DCam.SetActive(true);
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.Is2D = true;
            playerController.Astronaut.SetActive(true);
            playerController.transform.rotation = transform.rotation;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _2DCam.SetActive(false);
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.Is2D = false;
            playerController.Astronaut.SetActive(false);
        }
    }
}
