using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [SerializeField] private List<Card> selectedCards = new List<Card>();
    private PointsCars sourceStack; // Откуда взяли карты
    private Vector3 originalPosition; // Исходная позиция карт
    [SerializeField] private float hoverHeight = 0.5f; // Высота подъема карт
    private bool isAnimating = false; // Флаг для блокировки управления

    void Update()
    {
        // Игнорируем клики, если идет анимация
        if (isAnimating)
            return;

        if (Input.GetMouseButtonDown(0)) // Левый клик мыши
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
            Debug.Log(hit.collider);

            if (hit.collider != null)
            {
                PointsCars hitStack = hit.collider.GetComponent<PointsCars>();
                if (hitStack != null)
                {
                    // Если у нас уже есть выбранные карты
                    if (selectedCards.Count > 0)
                    {
                        // Пробуем добавить карты в новую стопку
                        // Блокируем управление и передаем callback для разблокировки
                        isAnimating = true;
                        hitStack.TryAddCards(selectedCards, () => isAnimating = false);
                        
                        // Очищаем выбор независимо от результата (карты уже в полете)
                        selectedCards.Clear();
                        sourceStack = null;
                        // Убрали else, т.к. возврат невозможен во время анимации
                    }
                    else
                    {
                        // Выбираем карты из стопки
                        sourceStack = hitStack;
                        selectedCards = hitStack.GetMatchingCards();
                        if (selectedCards.Count > 0)
                        {
                            originalPosition = selectedCards[0].transform.position;
                            // Поднимаем карты (мгновенно, без анимации здесь)
                            foreach (Card card in selectedCards)
                            {
                                Vector3 newPos = card.transform.position;
                                newPos.y += hoverHeight;
                                card.transform.position = newPos;
                            }
                        }
                        else
                        {
                            // Если не удалось взять карты (стопка пуста), сбрасываем sourceStack
                            sourceStack = null;
                        }
                    }
                }
            }
            else if (selectedCards.Count > 0)
            {
                // Клик мимо стопок - возвращаем карты
                ReturnCardsToSource();
            }
        }
    }

    private void ReturnCardsToSource()
    {
        if (sourceStack != null && selectedCards.Count > 0)
        {
            // Блокируем управление и передаем callback для разблокировки
            isAnimating = true;
            List<Card> cardsToReturn = new List<Card>(selectedCards); // Копируем список
            selectedCards.Clear(); // Очищаем сразу
            sourceStack.ReturnCards(cardsToReturn, () => isAnimating = false);
            sourceStack = null; // Сбрасываем исходную стопку
        }
    }
}
