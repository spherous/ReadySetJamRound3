using UnityEngine;
using UnityEngine.UI;

public class FuelBar : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private SlicedFilledImage bar;
    [SerializeField] private Image background;
    private void OnEnable() => player.onFuelChanged += UpdateFuelBar;
    private void OnDisable() => player.onFuelChanged -= UpdateFuelBar;

    private void Awake() 
    {
        background.material = new Material(background.material);
        background.material.SetFloat("_TimeOffset", 1f);
    }

    private void UpdateFuelBar(float newAmount)
    {
        if(player.maxFuel == 0)
            return;

        bar.fillAmount = Mathf.Clamp01(newAmount / player.maxFuel);
        background.material.SetFloat("_BarPercent", bar.fillAmount);
    }
}