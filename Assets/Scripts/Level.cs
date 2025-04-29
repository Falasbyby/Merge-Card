using UnityEngine;
using System.Collections.Generic; // Добавим на всякий случай, если понадобятся списки

public class Level : MonoBehaviour
{
    [Tooltip("Количество точек спавна карт (PointsCars) на уровне")]
    public GameObject PointsCars;

    private PointsCars[] pointsCars;

    [Tooltip("Количество карт, создаваемых для каждого уникального спрайта")]
    public int NumberOfCardsPerSprite = 5;

    [Tooltip("Количество уникальных спрайтов, используемых на уровне")]
    public int NumberOfSpritesToUse = 4;

    void Start()
    {
       
    }
    private void OnEnable()
    {
        pointsCars = PointsCars.GetComponentsInChildren<PointsCars>();
        SpawnerCard.Instance.SetInfo(NumberOfCardsPerSprite, NumberOfSpritesToUse, pointsCars);
    }

}