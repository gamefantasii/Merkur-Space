using System.Collections.Generic;
using UnityEngine;

public class TubePath : MonoBehaviour
{
    public Transform pathRoot;                 
    public bool autoCollect = true;

    [HideInInspector] public List<Transform> points = new List<Transform>();
    public float TotalLength { get; private set; }

    private float[] cumLen;

    private void Awake() => Build();

    public void Build()
    {
        points.Clear();
        if (autoCollect && pathRoot != null)
        {
            for (int i = 0; i < pathRoot.childCount; i++)
                points.Add(pathRoot.GetChild(i));
        }

        if (points.Count < 2)
        {
            cumLen = new float[0];
            TotalLength = 0f;
            return;
        }

        cumLen = new float[points.Count];
        cumLen[0] = 0f;
        float acc = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            acc += Vector3.Distance(points[i - 1].position, points[i].position);
            cumLen[i] = acc;
        }
        TotalLength = acc;
    }

    public int PointCount => points.Count;
    public Vector3 GetPointByIndex(int i)
    {
        if (points.Count == 0) return transform.position;
        i = Mathf.Clamp(i, 0, points.Count - 1);
        return points[i].position;
    }

    public Vector3 GetFirstPoint() => GetPointByIndex(0);
    public Vector3 GetLastPoint() => GetPointByIndex(points.Count - 1);
}