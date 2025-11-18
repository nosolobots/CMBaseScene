using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    public Transform waypointTransform;
    [Min(0f)]
    public float waitTime = 2f;
    public bool orientAgent = false;
}
public class PathWaypoints : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [SerializeField] List<Waypoint> waypoints = new List<Waypoint>();
    public List<Waypoint> Waypoints => waypoints;
}
