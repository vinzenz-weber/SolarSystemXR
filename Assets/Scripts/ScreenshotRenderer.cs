using UnityEngine;

public class ScreenshotRenderer : MonoBehaviour
{
    public string FileName = "Jupiter_Render.png";
    public int SuperSize = 2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Speichert den Screenshot im Projektordner, wenn du im Editor bist.
            ScreenCapture.CaptureScreenshot(FileName, SuperSize);
            Debug.Log("Screenshot gespeichert: " + FileName);
        }
    }
}
