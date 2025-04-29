using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Image spriteCard;
    private int spriteIndex; // Индекс спрайта для определения пар
    public Color color;
    public void SetSprite(Sprite sprite)
    {
        spriteCard.sprite = sprite;
    }

    public void SetSpriteIndex(int index)
    {
        spriteIndex = index;
    }
    public void SetColor(Color color)
    {
        this.color = color;
        meshRenderer.material.color = color;
    }

    public int GetSpriteIndex()
    {
        return spriteIndex;
    }
}
