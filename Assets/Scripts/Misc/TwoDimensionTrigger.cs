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
            playerController.is2D = true;
            playerController._astronaut.SetActive(true);
            playerController._firstPersonCam.SetActive(false);
            playerController.transform.rotation = transform.rotation;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _2DCam.SetActive(false);
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.is2D = false;
            playerController._astronaut.SetActive(false);
            playerController._firstPersonCam.SetActive(true);
        }
    }
}
