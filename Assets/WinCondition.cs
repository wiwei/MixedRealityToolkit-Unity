using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    private GameObject victoryBanana;

    private void Start()
    {
        victoryBanana = GameObject.Find("VictoryBanana");    
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WinCondition")
        {
            Debug.Log("Win condition triggered");
            gameObject.SetActive(false);
            victoryBanana.SetActive(true);
        }
    }
}
