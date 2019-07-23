using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuScript : MonoBehaviour
{
    private GameObject banana;
    private GameObject victoryBanana;

    // Start is called before the first frame update
    void Start()
    {
        banana = GameObject.Find("banana");
        victoryBanana = GameObject.Find("VictoryBanana");
    }
    
    public void ResetBanana()
    {
        banana.transform.position = new Vector3(0, .5f, 1);
    }

    public void ResetStage()
    {
        banana.SetActive(true);
        victoryBanana.SetActive(false);
        ResetBanana();
    }
}
