using DG.Tweening;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour
{
    public event Action ReachMaxValueEvent;
    public event Action ReachMinValueEvent;
    public event Action<int> ChangeEvent;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;

    [ReadOnly(true)]
    [SerializeField] private float currentValue;

    public float GetCurrentValue() => currentValue;
    public float GetMaxValue() => maxValue;

    private void Start()
    {
        UpdateFillAmount();
        SetRange(minValue, maxValue);
    }

    public void SetRange(float min, float max)
    {
        minValue = min;
        maxValue = max;
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        UpdateFillAmount();
    }

    public void SetValue(float value)
    {
        Debug.Log($"Set value {value} in {currentValue}");
        currentValue = value;
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        Debug.Log($"Seted value {value} in {currentValue}");
        UpdateFillAmount();
        ChangeEvent?.Invoke((int)currentValue);

        if (currentValue >= maxValue)
            ReachMaxValueEvent?.Invoke();

        if (currentValue <= minValue)
            ReachMinValueEvent?.Invoke();
    }

    public virtual void Show()
    {
        canvasGroup.DOFade(1, 0.2f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void Hide()
    {
        canvasGroup.DOFade(0, 0.2f);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    [ContextMenu("IncreaseTest")]

    public void Increase()
    {
        SetValue(currentValue++);
        ChangeEvent?.Invoke((int)currentValue);
    }

    public void IncreaseValue(float amount)
    {
        SetValue(currentValue + amount);
        ChangeEvent?.Invoke((int)currentValue);
    }

    public void Reduce()
    {
        SetValue(currentValue--);
        ChangeEvent?.Invoke((int)currentValue);
    }

    public void ReduceValue(float amount)
    {
        SetValue(currentValue - amount);
        ChangeEvent?.Invoke((int)currentValue);
    }

    private void UpdateFillAmount()
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.InverseLerp(minValue, maxValue, currentValue);
    }
}
