using UnityEngine;
using System.Collections; // Для корутин

public class GameManager : Singleton<GameManager> // Используем ваш базовый класс Singleton<T>
{
    [Tooltip("Задержка перед проверкой условия победы после удаления набора карт (в секундах)")]
    public float winCheckDelay = 0.5f; // Даем время на анимацию удаления и другие события

    private bool isCheckingWinCondition = false; // Флаг, чтобы не запускать проверку много раз одновременно
    public int currentScore = 0; // Текущий счет

    void Start()
    {
        // При старте игры сбрасываем счет (если GameManager не уничтожается)
        ResetScore();
    }

    // Этот метод будет вызываться из PointsCars, когда набор карт удален
    public void NotifyCardSetRemoved()
    {
        // Увеличиваем счет
        currentScore++;
        Debug.Log($"Set removed! Current score: {currentScore}");

        // Обновляем UI и запускаем анимацию
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.TriggerScoreBounce();
        }
        else
        {
            Debug.LogWarning("UIManager instance not found when updating score.");
        }

        // Запускаем проверку на победу с задержкой, если она еще не запущена
        if (!isCheckingWinCondition)
        {            
            StartCoroutine(CheckWinConditionDelayed());
        }
    }

    // Сбрасывает счет (например, при старте нового уровня)
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0.");
        // Обновляем UI, чтобы показать 0
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
        }
        else
        {
            // UIManager может быть еще не готов при самом первом запуске
            // Можно подождать кадр или вызвать позже из Start UIManager
            // Debug.LogWarning("UIManager instance not found when resetting score.");
        }
    }

    private IEnumerator CheckWinConditionDelayed()
    {
        isCheckingWinCondition = true;
        yield return new WaitForSeconds(winCheckDelay); 

        // Ищем оставшиеся карты
        PointsCars[] remainingPointsCars = FindObjectsOfType<PointsCars>(); // Ищем активные точки спавна
        int totalRemainingCards = 0;
        foreach (var points in remainingPointsCars)
        {
            totalRemainingCards += points.CountCards; // Используем свойство CountCards
        }

        Debug.Log($"Checking win condition. Total remaining cards across all PointsCars: {totalRemainingCards}");

        // Если карт не осталось
        if (totalRemainingCards == 0)
        {            
            Debug.Log("No cards left! Activating next level.");
            if (LevelController.Instance != null)
            {
                // Дополнительная задержка перед переходом, чтобы игрок успел увидеть счет
                yield return new WaitForSeconds(0.5f);    
                LevelController.Instance.ActivateNextLevel();
            }
            else
            {                
                Debug.LogError("LevelController instance not found!");
            }
        }
        isCheckingWinCondition = false;
    }
    
    // Можно добавить другие методы управления игрой (пауза, рестарт и т.д.)
} 