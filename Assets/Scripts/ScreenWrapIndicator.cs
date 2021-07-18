using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Camera;

public class ScreenWrapIndicator : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Player player;

    [SerializeField] private GroupFader top;
    [SerializeField] private GroupFader bottom;
    [SerializeField] private GroupFader left;
    [SerializeField] private GroupFader right;

    private GroupFader activeFader;

    public float edgeRange;

    private void Update()
    {
        Vector3 playerScreenPos = cam.WorldToScreenPoint(player.transform.position, MonoOrStereoscopicEye.Mono);
        GroupFader newFader = null;

        if(playerScreenPos.y <= edgeRange && player.canScreenWrap)
        {
            if(activeFader != bottom)
                newFader = bottom;
        }
        else if((Screen.height - playerScreenPos.y) <= edgeRange && player.canScreenWrap)
        {
            if(activeFader != top)
                newFader = top;
        }
        else if(playerScreenPos.x <= edgeRange && player.canScreenWrap)
        {
            if(activeFader != left)
                newFader = left;
        }
        else if((Screen.width - playerScreenPos.x) <= edgeRange && player.canScreenWrap)
        {
            if(activeFader != right)
                newFader = right;
        }
        else
        {
            activeFader?.FadeOut();
            activeFader = null;
            newFader = null;
        }

        if(newFader != null)
        {
            activeFader?.FadeOut();
            newFader.FadeIn();
            activeFader = newFader;
        }
    }
}
