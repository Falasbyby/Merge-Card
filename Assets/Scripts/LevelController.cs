using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Убираем наследование от Singleton<T>, если он был специфичен для DontDestroyOnLoad
public class LevelController : Singleton<LevelController> // Можно сделать Singleton, если нужен глобальный доступ
{
    [Tooltip("Список корневых GameObject'ов для каждого уровня. Перетащите сюда объекты уровней со сцены.")]
    public List<GameObject> levelRoots = new List<GameObject>();

    [Tooltip("Индекс уровня, который должен быть активен при старте сцены.")]
    public int startingLevelIndex = 0;

    private GameObject currentActiveLevel = null;
    private int currentLevelIndex = -1;

    void Start()
    {
        // При старте деактивируем все уровни, кроме стартового

    }

    public void Init()
    {
        StartCoroutine(InitCoroutine());

    }
    private IEnumerator InitCoroutine()
    {
        yield return new WaitForSeconds(1f);
        DeactivateAllLevels();
        ActivateLevel(startingLevelIndex);
        SocketSystem.Instance.SendBoostGameMessage();
        CountdownTimer.Instance.Init();
    }

    // Активирует уровень по индексу в списке levelRoots
    public void ActivateLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelRoots.Count)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        if (levelRoots[levelIndex] == currentActiveLevel)
        {
            // Debug.LogWarning($"Level {levelIndex} is already active."); // Можно раскомментировать для отладки
            return; // Уровень уже активен
        }

        // Деактивируем текущий активный уровень (если он есть)
        if (currentActiveLevel != null)
        {
            currentActiveLevel.SetActive(false);
        }

        // Активируем новый уровень
        currentActiveLevel = levelRoots[levelIndex];
        currentLevelIndex = levelIndex;
        if (currentActiveLevel != null) // Доп. проверка на случай null в списке
        {
            currentActiveLevel.SetActive(true); // Активация вызовет OnEnable() у Level.cs

        }
        else
        {
            currentLevelIndex = -1;
        }
    }

    // Деактивирует все уровни из списка
    private void DeactivateAllLevels()
    {
        foreach (GameObject levelRoot in levelRoots)
        {
            if (levelRoot != null)
            {
                levelRoot.SetActive(false);
            }
        }
        currentActiveLevel = null;
        currentLevelIndex = -1;
    }

    // Метод для загрузки следующего уровня по порядку в списке
    public void ActivateNextLevel()
    {
        if (levelRoots.Count == 0)
        {
            Debug.LogError("Cannot activate next level: levelRoots list is empty.");
            return;
        }

        int nextIndex = currentLevelIndex + 1;
        // Если дошли до конца списка, выбираем случайный из последних 10
        if (nextIndex >= levelRoots.Count)
        {
            int startIndex = Mathf.Max(0, levelRoots.Count - 10); // Начальный индекс для выбора (не раньше 0)
            int randomIndex = Random.Range(startIndex, levelRoots.Count); // Выбираем случайный индекс от startIndex до последнего

            ActivateLevel(randomIndex);
        }
        else
        {
            ActivateLevel(nextIndex); // Активируем следующий по порядку
        }
    }

    // Метод для перезапуска текущего уровня
    public void ReloadCurrentLevel()
    {
        if (currentLevelIndex != -1)
        {
            // Деактивируем, а затем снова активируем
            int indexToReload = currentLevelIndex;
            if (currentActiveLevel != null)
            {
                currentActiveLevel.SetActive(false);
            }
            // Короткая задержка может быть полезна, чтобы скрипты успели среагировать на OnDisable
            // Invoke(nameof(ReactivateLevel), 0.1f); // или использовать корутину
            // Но для простоты пока просто активируем сразу
            ActivateLevel(indexToReload);
        }
        else
        {
            Debug.LogWarning("No level currently active to reload. Activating starting level.");
            ActivateLevel(startingLevelIndex);
        }
    }

}