﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{
    
    public enum Algorithm
    { 
        BreadthFirstSearch, 
        Dijkstra, 
        AStar
    };

    static PathRequestManager instance;
    Queue<PathResult> results = new Queue<PathResult>();
    WorldGrid grid;
    public Algorithm algorithm = Algorithm.BreadthFirstSearch;

    void Awake()
    {
        instance = this;
        grid = this.gameObject.GetComponent<WorldGrid>();
    }

    void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.exploredPoints, result.success);
                }
            }
        }
    }

    public static void RequestPath(Vector3 start, Vector3 end, Action<List<Node>, Dictionary<int, List<Node>>, bool> callback)
    {
        Debug.LogWarning("Path Requested");
        PathRequest request = new PathRequest(start, end, callback);
        ThreadStart threadStart = delegate {
            Debug.LogWarning("Active Algorithm: " + instance.algorithm);
            switch(instance.algorithm)
            {
                case Algorithm.AStar:
                    Pathfinding.AStar(instance.grid, request, instance.FinishedProcessingPath);
                    break;
                case Algorithm.Dijkstra:
                    Pathfinding.Dijkstra(instance.grid, request, instance.FinishedProcessingPath);
                    break;
                default:
                    Pathfinding.BreadthFirstSearch(instance.grid, request, instance.FinishedProcessingPath);
                    break;
            }
        };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);
        }
    }
}

public struct PathResult
{
    public List<Node> path;
    public Dictionary<int, List<Node>> exploredPoints;
    public bool success;
    public Action<List<Node>, Dictionary<int, List<Node>>, bool> callback;

    public PathResult(List<Node> path, Dictionary<int, List<Node>> exploredPoints, bool success, Action<List<Node>, Dictionary<int, List<Node>>, bool> callback)
    {
        this.path = path;
        this.exploredPoints = exploredPoints;
        this.success = success;
        this.callback = callback;
    }

}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<List<Node>, Dictionary<int, List<Node>>, bool> callback;

    public PathRequest(Vector3 _start, Vector3 _end, Action<List<Node>, Dictionary<int, List<Node>>, bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }

}
