using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnerCard : Singleton<SpawnerCard>
{
    [SerializeField] private Sprite[] spritesCard;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private PointsCars[] spawnPoint;

    [SerializeField] private Color[] colors;

    public int NumberOfCardsPerSprite;
    public int NumberOfSpritesToUse;

    void Start()
    {
       
    }
    public void SetInfo(int numberOfCardsPerSprite, int numberOfSpritesToUse,PointsCars[] spawnPoint)
    {
        this.NumberOfCardsPerSprite = numberOfCardsPerSprite;
        this.NumberOfSpritesToUse = numberOfSpritesToUse;
        this.spawnPoint = spawnPoint;
        SpawnCard();
    }
    public void SpawnCard()
    {
        // Перемешиваем массив спрайтов и берем первые 4
        spritesCard = spritesCard.OrderBy(x => Random.value).ToArray();
        
        // Создаем список для всех карт
        List<Card> allCards = new List<Card>();
        
        // Создаем карты для каждого из первых 4 спрайтов
        for (int spriteIndex = 0; spriteIndex < NumberOfSpritesToUse; spriteIndex++)
        {
            // Создаем 5 карт для текущего спрайта
            for (int cardCount = 0; cardCount < NumberOfCardsPerSprite; cardCount++)
            {
                Card newCard = Instantiate(cardPrefab);
                newCard.SetSprite(spritesCard[spriteIndex]);
                newCard.SetSpriteIndex(spriteIndex);
                newCard.SetColor(colors[spriteIndex]);
                allCards.Add(newCard);
            }
        }
        
        // Перемешиваем все созданные карты
        allCards = allCards.OrderBy(x => Random.value).ToList();
        
        // Вычисляем, сколько карт должно быть в каждой точке спавна
        int totalCards = allCards.Count;
        int cardsPerPoint = totalCards / spawnPoint.Length;
        
        // Распределяем карты по точкам спавна
        for (int i = 0; i < spawnPoint.Length; i++)
        {
            // Добавляем несколько карт в каждую точку спавна
            for (int j = 0; j < cardsPerPoint; j++)
            {
                int cardIndex = i * cardsPerPoint + j;
                if (cardIndex < allCards.Count)
                {
                    spawnPoint[i].AddCard(allCards[cardIndex]);
                }
            }
        }
        
        // Если остались лишние карты, распределяем их по одной в каждую точку
        int remainingCards = totalCards % spawnPoint.Length;
        for (int i = 0; i < remainingCards; i++)
        {
            int cardIndex = totalCards - remainingCards + i;
            spawnPoint[i].AddCard(allCards[cardIndex]);
        }
    }
}
