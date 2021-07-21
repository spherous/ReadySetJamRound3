using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GroupFader fader;
    public bool paused = false;
    public void Enable()
    {
        paused = true;
        Time.timeScale = 0;
        fader.FadeIn();
    }
    public void Disable()
    {
        paused = false;
        Time.timeScale = 1;
        fader.FadeOut();
    }
    public void Toggle(CallbackContext context)
    {
        if(!context.performed)
            return;

        if(paused)
            Disable();
        else
            Enable();
    }
}
