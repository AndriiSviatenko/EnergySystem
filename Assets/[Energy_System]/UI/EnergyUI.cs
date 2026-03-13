using DG.Tweening;
using TMPro;
using UnityEngine;

public class EnergyUI : MonoBehaviour
{
    [SerializeField] private CustomSlider energySlider;

    [SerializeField] private TextMeshProUGUI energyLabel;
    [SerializeField] private TextMeshProUGUI regenTimerLabel;

    [SerializeField] private float sliderAnimDuration = 0.4f;
    [SerializeField] private Ease sliderEase = Ease.OutCubic;

    private Tween _sliderTween;
    private float _displayedValue;

    private void Start()
    {
        if (EnergySystem.Instance == null)
            return;

        Subscribe();
        SyncAll();
    }

    private void OnEnable()
    {
        if (EnergySystem.Instance == null)
            return;

        Subscribe();
        SyncAll();
    }

    private void OnDisable()
    {
        if (EnergySystem.Instance == null)
            return;

        Unsubscribe();
    }

    private void Subscribe()
    {
        EnergySystem.Instance.OnEnergyChanged += HandleEnergyChanged;
        EnergySystem.Instance.OnEnergyFull += HandleEnergyFull;
        EnergySystem.Instance.OnRegenTimerTick += HandleRegenTick;
    }

    private void Unsubscribe()
    {
        EnergySystem.Instance.OnEnergyChanged -= HandleEnergyChanged;
        EnergySystem.Instance.OnEnergyFull -= HandleEnergyFull;
        EnergySystem.Instance.OnRegenTimerTick -= HandleRegenTick;
    }

    private void HandleEnergyChanged(int current, int max)
    {
        AnimateSliderTo(current);

        if (energyLabel != null)
            energyLabel.text = $"{current} / {max}";
    }

    private void HandleEnergyFull()
    {
        if (regenTimerLabel != null)
            regenTimerLabel.text = "Full";
    }

    private void HandleRegenTick(float secondsRemaining)
    {
        if (regenTimerLabel == null || EnergySystem.Instance.IsFull)
            return;

        secondsRemaining = Mathf.Max(0f, secondsRemaining);

        int mins = Mathf.FloorToInt(secondsRemaining / 60f);
        int secs = Mathf.FloorToInt(secondsRemaining % 60f);
        regenTimerLabel.text = $"Next in {mins}:{secs:D2}";
    }

    private void AnimateSliderTo(float target)
    {
        _sliderTween?.Kill();

        float from = _displayedValue;

        _sliderTween = DOTween
            .To(() => from,
                v => { from = v; _displayedValue = v; energySlider.SetValue(v); },
                target,
                sliderAnimDuration)
            .SetEase(sliderEase)
            .SetUpdate(true);
    }

    private void SyncAll()
    {
        int cur = EnergySystem.Instance.CurrentEnergy;
        int max = EnergySystem.Instance.MaxEnergy;

        _displayedValue = cur;
        energySlider.SetRange(0, max);
        energySlider.SetValue(cur);

        if (energyLabel != null)
            energyLabel.text = $"{cur} / {max}";

        if (regenTimerLabel != null)
            HandleRegenTick(EnergySystem.Instance.SecondsToNextRegen);
    }
}