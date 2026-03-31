using UnityEngine;

public class StartPhase : MonoBehaviour
{

    public bool startButtonClicked  = false; // Flag, um den Startzustand zu verfolgen
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartButtonClicked()
    {
        startButtonClicked = true;
    }
}
