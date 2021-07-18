using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRadius : MonoBehaviour
{
    public Player player;
    [SerializeField] private Collider2D col;

    private void Awake() {
        col.enabled = false;
        player.onActivatePowerup += PowerupActivate;
        player.onLosePowerup += LostPowerup;
    }

    private void LostPowerup(PowerupType lost)
    {
        if(lost == PowerupType.PickupRadius && col.enabled)
            col.enabled = false;
    }

    private void PowerupActivate(PowerupType activated)
    {
        if(activated == PowerupType.PickupRadius)
            col.enabled = true;
    }

    private void Update() => transform.position = player.transform.position;
}