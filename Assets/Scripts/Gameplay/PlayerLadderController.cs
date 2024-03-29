using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLadderController : MonoBehaviour
{
    private PlayerController _mainController;
    
    private void Start()
    {
        _mainController = GetComponent<PlayerController>();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Ladder")) return;
        
        enabled = false;
        _mainController.enabled = true;
    }
}
