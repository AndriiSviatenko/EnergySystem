using System;
using System.Collections;
using UnityEngine;

public class Timer
{
    public event Action<float, float> HasBeenUpdated;
    public event Action TimeIsOver;
    private float _time;
    private float _remainigTime;
    private Coroutine _countdown;

    public void Set(float time)
    {
        _time = time;
        _remainigTime = _time;
        HasBeenUpdated?.Invoke(_remainigTime, _time);
    }

    public void StartCountingTime(Func<IEnumerator, Coroutine> startCoroutine)
    {
        if (_countdown != null) return;
        _countdown = startCoroutine.Invoke(Countdown());
    }

    public void StopCountingTime(Action<Coroutine> stopCoroutine)
    {
        if (_countdown != null)
        {
            stopCoroutine.Invoke(_countdown);
            _countdown = null;
        }
    }

    private IEnumerator Countdown()
    {
        while (_remainigTime >= 0)
        {
            yield return new WaitForSeconds(1f);
            _remainigTime--;
            HasBeenUpdated?.Invoke(_remainigTime, _time);
        }

        TimeIsOver?.Invoke();
    }
}
