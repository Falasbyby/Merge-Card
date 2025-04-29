using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class CountdownTimer : Singleton<CountdownTimer>
{
    [Header("Timer Settings")]
    [Tooltip("Start time in seconds (e.g., 180 for 3 minutes)")]
    [SerializeField] private float startTimeInSeconds = 180f;

    [Header("UI Display")]
    [Tooltip("UI Text element to display the timer")]
    [SerializeField] private Text timerText; // Замените на TextMeshProUGUI, если используете TextMeshPro

    [Header("Events")]
    [Tooltip("Event invoked when the timer reaches zero")]
    public UnityEvent onTimerEnd; // Сюда можно добавить методы в инспекторе

    private float currentTime;
    private bool timerIsRunning = false;

    void Start()
    {
        // Инициализация таймера при старте
       
    }
    public void Init()
    {
        ResetTimer();
        StartTimer();
    }

    void Update()
    {
        if (timerIsRunning && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                TimerEnd();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Округляем до ближайшей секунды для отображения
            float displayTime = Mathf.Max(currentTime, 0f);
            TimeSpan timeSpan = TimeSpan.FromSeconds(displayTime);
            // Форматируем как ММ:СС
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    private void TimerEnd()
    {
        currentTime = 0;
        timerIsRunning = false;
        UpdateTimerDisplay(); // Обновить дисплей до 00:00
        Debug.Log("Timer finished!");
        UIManager.Instance.CompleteGame();
        // Вызываем событие
        onTimerEnd?.Invoke();
    }

    // --- Публичные методы для управления таймером ---

    /// <summary>
    /// Starts the countdown timer.
    /// </summary>
    public void StartTimer()
    {
        if (!timerIsRunning)
        {
            // Если таймер уже на нуле, сбрасываем его перед стартом
            if (currentTime <= 0)
            {
                ResetTimer();
            }
            timerIsRunning = true;
            Debug.Log("Timer started.");
        }
    }

    /// <summary>
    /// Stops the countdown timer.
    /// </summary>
    public void StopTimer()
    {
        if (timerIsRunning)
        {
            timerIsRunning = false;
            Debug.Log("Timer stopped at: " + currentTime);
        }
    }

    /// <summary>
    /// Resets the timer to the initial start time.
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTimeInSeconds;
        timerIsRunning = false; // По умолчанию сброшенный таймер не запущен
        UpdateTimerDisplay(); // Обновить отображение
        Debug.Log("Timer reset.");
    }

    /// <summary>
    /// Adds additional time to the current countdown.
    /// </summary>
    /// <param name="secondsToAdd">Seconds to add.</param>
    public void AddTime(float secondsToAdd)
    {
        currentTime += secondsToAdd;
        UpdateTimerDisplay(); // Обновить отображение
        Debug.Log("Added time: " + secondsToAdd);
    }
}
