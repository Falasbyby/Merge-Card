using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;

public class PointsCars : MonoBehaviour
{
    [SerializeField] private List<Card> cards = new List<Card>();
    [SerializeField] private float cardOffset = 0.05f; // Смещение по высоте между картами

    private Queue<Card> cardMoveQueue = new Queue<Card>();
    private bool isProcessingQueue = false;
    private Action currentAnimationCompleteCallback; // Callback для сообщения о завершении
    public ParticleSystem effectRemove;
    public int CountCards
    {
        get { return cards.Count; }
    }
    public void AddCard(Card card)
    {
        // Добавляем карту в список
        cards.Add(card);

        // Вычисляем конечную позицию
        Vector3 targetPosition = transform.position;
        targetPosition.y += cards.Count * cardOffset;

        // Устанавливаем карту как дочерний объект для точки спавна
        card.transform.parent = transform;

        // Добавляем карту в очередь для анимации
        cardMoveQueue.Enqueue(card);

        // Запускаем обработку очереди, если она еще не запущена
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessCardQueue());
        }
    }

    private IEnumerator ProcessCardQueue()
    {
        isProcessingQueue = true;

        while (cardMoveQueue.Count > 0)
        {
            Card currentCard = cardMoveQueue.Dequeue();
            // Защита от случая, если карта была удалена пока была в очереди
            if (currentCard == null || !cards.Contains(currentCard)) continue;

            Vector3 targetPosition = transform.position;
            int cardIndexInStack = cards.IndexOf(currentCard);
            if (cardIndexInStack != -1) // Убедимся, что карта есть в списке
            {
                targetPosition.y += cardIndexInStack * cardOffset;
            }
            SoundManager.Instance.PlayCardTransition();
            // Анимируем перемещение карты с помощью DOTween
            currentCard.transform
                .DOJump(targetPosition, 2, 1, 0.5f)
                .SetEase(Ease.OutQuad);
           
            yield return new WaitForSeconds(0.1f); // Задержка между картами
        }
        yield return new WaitForSeconds(0.5f);
        isProcessingQueue = false;
        // Вызываем callback после завершения всех анимаций в очереди
        currentAnimationCompleteCallback?.Invoke();
        currentAnimationCompleteCallback = null; // Сбрасываем callback

        CheckForCompleteSets();
    }

    public void TryAddCards(List<Card> newCards, Action onComplete)
    {
        // Сохраняем callback
        currentAnimationCompleteCallback = onComplete;
        // Добавляем карты в обратном порядке, чтобы сохранить стопку
        for (int i = newCards.Count - 1; i >= 0; i--)
        {
            AddCard(newCards[i]);
        }
        // Возвращаем true, т.к. всегда разрешаем добавлять
    }

    public void ReturnCards(List<Card> returningCards, Action onComplete)
    {
        // Сохраняем callback
        currentAnimationCompleteCallback = onComplete;
        // Возвращаем карты по одной в обратном порядке
        for (int i = returningCards.Count - 1; i >= 0; i--)
        {
            AddCard(returningCards[i]);
        }
    }

    public List<Card> GetMatchingCards()
    {
        List<Card> matchingCards = new List<Card>();

        if (cards.Count == 0)
            return matchingCards;

        // Get the top card's sprite index
        int topCardIndex = cards[cards.Count - 1].GetSpriteIndex();

        // Start from the top and go down through the stack
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i].GetSpriteIndex() == topCardIndex)
            {
                matchingCards.Add(cards[i]);
                cards.RemoveAt(i);
            }
            else
            {
                // Stop when we find a card that doesn't match
                break;
            }
        }

        // Возвращаем карты в обратном порядке, чтобы сохранить исходный порядок в стопке
        matchingCards.Reverse();
        return matchingCards;
    }

    public int GetCardCount()
    {
        return cards.Count;
    }

    private void CheckForCompleteSets()
    {
        // Проверяем, достаточно ли карт для полного набора
        if (cards.Count < SpawnerCard.Instance.NumberOfCardsPerSprite) return;

        // Берем индекс верхней карты
        int topCardIndex = cards[cards.Count - 1].GetSpriteIndex();
        int consecutiveCount = 0;
        List<Card> cardsToRemoveCandidate = new List<Card>();

        // Идем сверху вниз по стопке
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i].GetSpriteIndex() == topCardIndex)
            {
                consecutiveCount++;
                cardsToRemoveCandidate.Add(cards[i]);
                // Если набрали нужное количество, выходим из цикла
                if (consecutiveCount == SpawnerCard.Instance.NumberOfCardsPerSprite)
                {
                    break;
                }
            }
            else
            {
                // Последовательность прервалась
                break;
            }
        }

        // Если нашли полный набор подряд сверху
        if (consecutiveCount == SpawnerCard.Instance.NumberOfCardsPerSprite)
        {
            // Удаляем найденные карты
            RemoveCompleteSet(cardsToRemoveCandidate);
            // Перераспределяем оставшиеся карты
            RearrangeCards();

            // Сообщаем GameManager, что карты были удалены
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyCardSetRemoved();
            }
            else
            {
                Debug.LogWarning("GameManager instance not found when trying to notify about card removal.");
            }
        }
    }

    private void RemoveCompleteSet(List<Card> cardsToRemove)
    {
        // Находим центральную позицию набора карт
        float centerPosition = 0;
        foreach (Card card in cardsToRemove)
        {
            centerPosition += card.transform.position.y;
        }
        centerPosition /= cardsToRemove.Count;

        foreach (Card card in cardsToRemove)
        {
            // Удаляем из основного списка
            if (cards.Contains(card)) // Доп. проверка на всякий случай
            {
                cards.Remove(card);
            }
            // Анимация исчезновения и уничтожение объекта
            card.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);

        }

        // Запускаем эффект в центре набора карт
        StartCoroutine(TimerEffectRemove(centerPosition));
    }

    private IEnumerator TimerEffectRemove(float position)
    {
        yield return new WaitForSeconds(0.3f);
        SoundManager.Instance.PlayVictory();
        effectRemove.transform.localPosition = new Vector3(0, position, 0);
        effectRemove.Play();
    }

    private void RearrangeCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card currentCard = cards[i];
            Vector3 targetPosition = transform.position;
            targetPosition.y += i * cardOffset;
            // Плавно перемещаем карту на новую позицию, если она не там
            if (Vector3.Distance(currentCard.transform.position, targetPosition) > 0.01f)
            {
                currentCard.transform.DOMove(targetPosition, 0.2f).SetEase(Ease.OutQuad);
            }
        }
    }
}

