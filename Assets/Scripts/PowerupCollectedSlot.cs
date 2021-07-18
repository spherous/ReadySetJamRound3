using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class PowerupCollectedSlot : SerializedMonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Image slot;
    [SerializeField] private CanvasGroup group;
    [OdinSerialize] public Dictionary<PowerupType, Sprite> spriteDict = new Dictionary<PowerupType, Sprite>();

    private void Awake() {
        group.alpha = 0;
        player.onPickedUpPowerup += PickedUp;
        player.onActivatePowerup += Activated;
    }

    private void Activated(PowerupType activated)
    {
        slot.sprite = null;
        group.alpha = 0;
    }

    private void PickedUp(PowerupType pickedUp)
    {
        slot.sprite = spriteDict[pickedUp];
        group.alpha = 1;
    }
}