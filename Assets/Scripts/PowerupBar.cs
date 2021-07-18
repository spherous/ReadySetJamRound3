using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class PowerupBar : SerializedMonoBehaviour
{
    [SerializeField] private PowerupActiveSlot slotPrefab;
    [SerializeField] private Player player;
    [OdinSerialize] public Dictionary<PowerupType, Sprite> spriteDict = new Dictionary<PowerupType, Sprite>();
    Dictionary<PowerupType, PowerupActiveSlot> powerupSlotDict = new Dictionary<PowerupType, PowerupActiveSlot>();

    private void Awake() {
        player.onActivatePowerup += Activated;
        player.onLosePowerup += Lost;
    }

    private void Lost(PowerupType lost)
    {
        if(powerupSlotDict.ContainsKey(lost))
        {
            PowerupActiveSlot slot = powerupSlotDict[lost];
            powerupSlotDict.Remove(lost);
            Destroy(slot.gameObject);
        }
    }

    private void Activated(PowerupType activated)
    {
        if(powerupSlotDict.ContainsKey(activated))
            powerupSlotDict[activated].Refresh();
        else
        {
            PowerupActiveSlot slot = Instantiate(slotPrefab, transform);
            powerupSlotDict.Add(activated, slot);
            slot.SetPowerup(spriteDict[activated]);
        }
    }
}
