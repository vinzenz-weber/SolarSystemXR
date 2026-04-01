using System.Collections;
using UnityEngine;

public class StartPhase : MonoBehaviour
{

    public bool startButtonClicked  = false; // Flag, um den Startzustand zu verfolgen
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(StartAfterTime());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartAfterTime()
    {
        yield return new WaitForSeconds(3);

        startButtonClicked = true;

    }
}


