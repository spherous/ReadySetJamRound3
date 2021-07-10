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
}
