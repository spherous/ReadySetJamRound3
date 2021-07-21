using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRadius : MonoBehaviour
{
    public Player player;
    [SerializeField] private Collider2D col;
    [SerializeField] private MeshRenderer indicator;

    private void Awake()
    {
        ToggleOff();
        player.onActivatePowerup += PowerupActivate;
        player.onLosePowerup += LostPowerup;
    }

    private void ToggleOff()
    {
        col.enabled = false;
        indicator.enabled = false;
    }

    private void LostPowerup(PowerupType lost)
    {
        if(lost == PowerupType.PickupRadius && col.enabled)
            ToggleOff();
    }

    private void PowerupActivate(PowerupType activated)
    {
        if(activated == PowerupType.PickupRadius)
        {
            indicator.enabled = true;
            col.enabled = true;
        }
    }

    private void Update() => transform.position = player.transform.position;
}