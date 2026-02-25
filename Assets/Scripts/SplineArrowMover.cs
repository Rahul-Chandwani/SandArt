using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineArrowMover : MonoBehaviour
{
    [Header("Spline")]
    public SplineContainer splineContainer;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public int numberOfSprites = 10;
    public float heightOffset = 0f;
    public Vector3 rotationOffset;

    [Header("Movement")]
    public float speed = 0.2f;

    private List<Transform> arrows = new List<Transform>();
    private float[] tValues;

    void Start()
    {
        SpawnArrows();
    }

    void Update()
    {
        if (splineContainer == null || arrows.Count == 0)
            return;

        for (int i = 0; i < arrows.Count; i++)
        {
            tValues[i] += speed * Time.deltaTime;

            // Proper closed loop wrap
            if (tValues[i] > 1f)
                tValues[i] -= 1f;

            UpdateArrow(i);
        }
    }

    void SpawnArrows()
    {
        if (arrowPrefab == null || splineContainer == null || numberOfSprites <= 0)
            return;

        tValues = new float[numberOfSprites];

        for (int i = 0; i < numberOfSprites; i++)
        {
            // Even spacing across full closed spline
            float t = (float)i / numberOfSprites;
            tValues[i] = t;

            GameObject arrow = Instantiate(arrowPrefab);
            arrows.Add(arrow.transform);

            UpdateArrow(i);
        }
    }

    void UpdateArrow(int index)
    {
        float t = tValues[index];

        Vector3 position = splineContainer.EvaluatePosition(t);
        Vector3 tangent = splineContainer.EvaluateTangent(t);

        // Height offset
        position += Vector3.up * heightOffset;
        arrows[index].position = position;

        // Align with spline direction
        if (tangent != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);
            rotation *= Quaternion.Euler(rotationOffset);
            arrows[index].rotation = rotation;
        }
    }
}