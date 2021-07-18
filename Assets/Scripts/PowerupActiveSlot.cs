using UnityEngine;
using UnityEngine.UI;

public class PowerupActiveSlot : MonoBehaviour
{
    [SerializeField] private Image slot;
    [SerializeField] public Image ticker;

    public float powerupDuration;
    float ellapsedDuration = 0;

    private void Update() {
        ticker.fillAmount = Mathf.Clamp01((powerupDuration - ellapsedDuration)/powerupDuration);
        ellapsedDuration += Time.deltaTime;
    }

    public void SetPowerup(Sprite toSet)
    {
        slot.sprite = toSet;
        ticker.fillAmount = 1;
    }
    public void Refresh()
    {
        ellapsedDuration = 0;
        ticker.fillAmount = 1;
    }
}