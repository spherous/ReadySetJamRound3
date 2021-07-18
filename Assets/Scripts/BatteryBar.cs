using UnityEngine;
using UnityEngine.UI;

public class BatteryBar : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private SlicedFilledImage bar;
    [SerializeField] private Image background;
    private void OnEnable() => player.onBatteryChanged += UpdateBatteryBar;
    private void OnDisable() => player.onBatteryChanged -= UpdateBatteryBar;

    private void Awake() => background.material = new Material(background.material);

    private void UpdateBatteryBar(float newAmount)
    {
        if(player.maxCharge == 0)
            return;
        
        bar.fillAmount = Mathf.Clamp01(newAmount / player.maxCharge);
        background.material.SetFloat("_BarPercent", bar.fillAmount);
    }
}