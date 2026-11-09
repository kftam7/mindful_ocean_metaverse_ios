using System.Collections.Generic;
using UnityEngine;
using BezierSolution;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class PathGenerator
{
    private Volume volume;
    private Transform container;

    public PathGenerator(Volume volume, Transform container)
    {
        this.volume = volume;
        this.container = container;
    }

    public BezierSpline GetBezierPath(string name, int pointCount, bool isLoop, Vector3 scale, Vector3 offset, Vector3? startPosition)
    {
        GameObject go = new GameObject(name);

        if (container != null)
        {
            go.transform.SetParent(container);
            go.transform.localScale = Vector3.one;
        }

        BezierSpline spline = go.AddComponent<BezierSpline>();
        spline.Initialize(pointCount);

        int startIndex = 0;

        if (startPosition != null)
        {
            startIndex = 1;
            spline[0].position = startPosition.Value;
        }

        for (int i = startIndex; i < pointCount; i++)
        {
            spline[i].position = Vector3.Scale(volume.GetRandomPosition(), scale) + offset;
        }

        spline.AutoConstructSpline();
        spline.loop = isLoop;

        return spline;
    }

    public BezierSpline GetCircularBezierPath(string name, int pointCount,
        int direction, bool isLoop, Vector3 scale, Vector3 offset, float startAngle, Vector3? startPosition, float? distance)
    {
        GameObject go = new GameObject(name);

        if (container != null)
        {
            go.transform.SetParent(container);
            go.transform.localScale = Vector3.one;
        }

        BezierSpline spline = go.AddComponent<BezierSpline>();
        spline.Initialize(pointCount);

        float increase = 360f / pointCount * direction;

        int startIndex = 0;
        if (startPosition != null)
        {
            startIndex = 1;
            spline[0].position = startPosition.Value;
            startAngle = Mathf.Atan(startPosition.Value.y / startPosition.Value.x) * Mathf.Rad2Deg;
        }

        for (int i = startIndex; i < pointCount; i++)
        {
            if (distance != null)
            {
                spline[i].position = Vector3.Scale(volume.GetRandomPosition(startAngle + increase * i, distance.Value), scale) + offset;
            }
            else
            {
                spline[i].position = Vector3.Scale(volume.GetRandomPosition(startAngle + increase * i), scale) + offset;
            }
        }

        spline.AutoConstructSpline();
        spline.loop = isLoop;

        return spline;
    }
    
    public BezierSpline GetBezierPathForNavMesh(string name, Vector3 startPosition, AukiSdkWrapper aukiSdkWrapper)
    {
        GameObject go = new GameObject(name);

        if (container != null)
        {
            go.transform.SetParent(container);
            go.transform.localScale = Vector3.one;
        }

        BezierSpline spline = go.AddComponent<BezierSpline>();
        List<Vector3> path = new List<Vector3>();

        // First random position
        Vector3 randomPosition = startPosition + Random.insideUnitSphere * 100f;
        NavMeshHit randomPositionOnNavMesh;
        NavMesh.SamplePosition(randomPosition, out randomPositionOnNavMesh, 100f, NavMesh.AllAreas);
        if (randomPositionOnNavMesh.hit) randomPosition = randomPositionOnNavMesh.position;
        path.AddRange(aukiSdkWrapper.CalculatePath(startPosition, randomPosition));
        
        // Second random position
        Vector3 secondRandomPosition = randomPosition + Random.insideUnitSphere * 100f;
        NavMesh.SamplePosition(secondRandomPosition, out randomPositionOnNavMesh, 100f, NavMesh.AllAreas);
        if (randomPositionOnNavMesh.hit) secondRandomPosition = randomPositionOnNavMesh.position;
        path.AddRange(aukiSdkWrapper.CalculatePath(randomPosition, secondRandomPosition));
        
        // Return from second position to start position
        path.AddRange(aukiSdkWrapper.CalculatePath(secondRandomPosition, startPosition));
        
        spline.Initialize(path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            var position = path[i];
            position.y = startPosition.y;
            spline[i].position = position;
        }
        
        spline.AutoConstructSpline();
        spline.loop = true;
        
        for (int i = 0; i < spline.Count; i++)
        {
            // spline[i].followingControlPointLocalPosition = Vector3.zero;
            // spline[i].precedingControlPointLocalPosition = Vector3.zero;
            spline[i].followingControlPointLocalPosition = new Vector3(0.02f, 0, 0.02f);
            spline[i].precedingControlPointLocalPosition = new Vector3(-0.02f, 0, -0.02f);
        }
        spline.Refresh();

        return spline;
    }
}