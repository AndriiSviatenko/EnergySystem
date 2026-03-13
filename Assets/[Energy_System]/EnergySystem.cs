using System;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance { get; private set; }

    public event Action OnEnergyFull;
    public event Action OnEnergyEmpty;
    public event Action<float> OnRegenTimerTick;
    public event Action<int, int> OnEnergyChanged;
    public event Action<int, int> OnEnergySpent;
    public event Action<int, int> OnEnergyRestored;

    [SerializeField] private EnergyConfig config;

    private Timer _regenTimer;
    private int _currentEnergy;

    public int CurrentEnergy => _currentEnergy;
    public int MaxEnergy => config.MaxEnergy;
    public bool IsFull => _currentEnergy >= config.MaxEnergy;
    public bool IsEmpty => _currentEnergy <= 0;
    public float SecondsToNextRegen { get; private set; }

    public bool HasEnough(int amount) => _currentEnergy >= amount;

    public void SetEnergy(int value)
    {
        _currentEnergy = Mathf.Clamp(value, 0, config.MaxEnergy);
        Save();
        BroadcastChanged();
        StartRegenTimer();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Load();
        ApplyOfflineRegen();
        BroadcastChanged();
        StartRegenTimer();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            _regenTimer?.StopCountingTime(StopCoroutine);
            Save();
        }
        else
        {
            ApplyOfflineRegen();
            BroadcastChanged();
            StartRegenTimer();
        }
    }

    private void OnApplicationQuit() =>
        Save();

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || _currentEnergy < amount) 
            return false;

        int before = _currentEnergy;
        _currentEnergy = Mathf.Max(0, _currentEnergy - amount);

        Save();

        OnEnergySpent?.Invoke(before - _currentEnergy, _currentEnergy);
        BroadcastChanged();

        if (IsEmpty) 
            OnEnergyEmpty?.Invoke();

        StartRegenTimer();
        return true;
    }

    public void Restore(int amount)
    {
        if (amount <= 0)
            return;

        bool wasFull = IsFull;
        int before = _currentEnergy;
        _currentEnergy = Mathf.Min(config.MaxEnergy, _currentEnergy + amount);
        int restored = _currentEnergy - before;

        if (restored == 0) 
            return;

        Save();
        OnEnergyRestored?.Invoke(restored, _currentEnergy);
        BroadcastChanged();

        if (!wasFull && IsFull)
        {
            StopRegenTimer();
            OnEnergyFull?.Invoke();
        }
    }

    public void RefillFull()
    {
        _currentEnergy = config.MaxEnergy;
        StopRegenTimer();
        Save();
        BroadcastChanged();
        OnEnergyFull?.Invoke();
    }

    public void SpendFull()
    {
        _currentEnergy = 0;
        Save();
        BroadcastChanged();
        OnEnergyEmpty?.Invoke();
    }

    private void StartRegenTimer()
    {
        if (IsFull)
            return;

        StopRegenTimer();

        _regenTimer = new Timer();
        _regenTimer.Set(config.RegenIntervalSeconds);
        _regenTimer.HasBeenUpdated += OnTimerUpdated;
        _regenTimer.TimeIsOver += OnRegenTick;
        _regenTimer.StartCountingTime(StartCoroutine);
    }

    private void StopRegenTimer()
    {
        if (_regenTimer == null) 
            return;

        _regenTimer.HasBeenUpdated -= OnTimerUpdated;
        _regenTimer.TimeIsOver -= OnRegenTick;
        _regenTimer.StopCountingTime(StopCoroutine);
        _regenTimer = null;
    }

    private void OnTimerUpdated(float remaining, float _)
    {
        SecondsToNextRegen = remaining;
        OnRegenTimerTick?.Invoke(remaining);
    }

    private void OnRegenTick()
    {
        Restore(config.RegenAmountPerTick);

        if (!IsFull) 
            StartRegenTimer();
    }

    private void ApplyOfflineRegen()
    {
        if (IsFull)
            return;

        long savedTicks = long.Parse(
            PlayerPrefs.GetString(config.TimeStampSaveKey, DateTime.UtcNow.Ticks.ToString()));

        double offlineSec = (DateTime.UtcNow - new DateTime(savedTicks, DateTimeKind.Utc)).TotalSeconds;

        if (offlineSec <= 0)
            return;

        int ticks = Mathf.FloorToInt((float)offlineSec / config.RegenIntervalSeconds);
        if (ticks > 0)
            _currentEnergy = Mathf.Min(config.MaxEnergy, _currentEnergy + config.RegenAmountPerTick * ticks);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(config.EnergySaveKey, _currentEnergy);
        PlayerPrefs.SetString(config.TimeStampSaveKey, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(config.EnergySaveKey))
        {
            _currentEnergy = Mathf.Clamp(PlayerPrefs.GetInt(config.EnergySaveKey), 0, config.MaxEnergy);
        }
        else
        {
            _currentEnergy = config.InitialEnergy;
            Save();
        }
    }

    private void BroadcastChanged() => 
        OnEnergyChanged?.Invoke(_currentEnergy, config.MaxEnergy);
}
