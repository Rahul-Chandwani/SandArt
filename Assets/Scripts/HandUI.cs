using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    private RectTransform handTransform;
    private Image hand;
    public Vector2 offset;
    public Sprite idle;
    public Sprite click;
    private Canvas canvas;

    void Start()
    {
        handTransform = transform.GetChild(0).GetComponent<RectTransform>();
        hand = handTransform.GetComponent<Image>();

        // Get the parent canvas
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.worldCamera,
            out canvasPos
        );

        handTransform.anchoredPosition = canvasPos + offset;

        if (Input.GetMouseButtonDown(0))
        {
            hand.sprite = click;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            hand.sprite = idle;
        }
    }
}