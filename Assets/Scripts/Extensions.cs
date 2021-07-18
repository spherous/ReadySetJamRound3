using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions 
{
    public static (float x, float y) GetRandomOffScreenLocation()
    {
        //  Choose a random position off the edge of the screen
        int offEdge = UnityEngine.Random.Range(0, 2);
        int posOrNeg = UnityEngine.Random.Range(0, 2);
        float offset = UnityEngine.Random.Range(1, 10);

        return offEdge switch{
            // off height
            0 => (UnityEngine.Random.Range(0f, Screen.width), posOrNeg == 0 ? Screen.height + offset : 0 - offset),
            // off width
            1 => (posOrNeg == 0 ? Screen.width + offset : 0 - offset, UnityEngine.Random.Range(0f, Screen.height)),
            _ => (0,0)
        };
    }

    public static Vector3 GetScreenWrapPosition(this Vector3 screenPos)
    {
        // Check horizontal wrapping
        if(screenPos.x < 0)
            screenPos = new Vector3(Screen.width + screenPos.x, screenPos.y, screenPos.z);
        else if(screenPos.x > Screen.width)
            screenPos = new Vector3(screenPos.x - Screen.width, screenPos.y, screenPos.z);

        // Check vertical wrapping
        if(screenPos.y < 0)
            screenPos = new Vector3(screenPos.x, Screen.height + screenPos.y, screenPos.z);
        else if(screenPos.y > Screen.height)
            screenPos = new Vector3(screenPos.x, screenPos.y - Screen.height, screenPos.z);
        
        return screenPos;
    }

    public static Vector3 ClampPositionToScreen(this Vector3 screenPos) => 
        new Vector3(Mathf.Clamp(screenPos.x, 0, Screen.width), Mathf.Clamp(screenPos.y, 0, Screen.height), screenPos.z);

    public static bool IsOnScreen(this Vector3 screenPos) => 
        screenPos.x < 0 || screenPos.y < 0 || screenPos.x > Screen.width || screenPos.y > Screen.height;
}
