using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinColliderTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "banana")
        {
            Debug.Log("Win condition triggered");
        }
    }
}
