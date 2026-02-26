using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    private Transform handTransform;
    private Image hand;
    public Vector2 offset;
    public Sprite idle;
    public Sprite click;
    public float dragSpeed = 10f; // Adjust this value for smoothness
    private Vector3 velocity = Vector3.zero; // Used for SmoothDamp

    void Start()
    {
        handTransform = transform.GetChild(0);
        hand = handTransform.GetComponent<Image>();
    }

    void Update()
    {
        Vector3 targetPosition = Input.mousePosition + new Vector3(offset.x, offset.y, 0);
        handTransform.position = Vector3.SmoothDamp(handTransform.position, targetPosition, ref velocity, 0.05f, dragSpeed);

        if (Input.GetMouseButtonDown(0))
        {
            hand.sprite = click;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            DOVirtual.DelayedCall(0.1f, () => hand.sprite = idle);
        }
    }
}
