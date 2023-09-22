using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SuperBotAI : MonoBehaviour
{
    public Transform target;
    public bool isNeedLine = false;

    public float speed = 200f;
    public float nextWayPointDistance = .5f;
    public Vector2 desireDirection;
    public LineRenderer pathRenderer;

    private Path path;

    private int currentWayPoint = 0;
    private bool reachedEndOfPath = false;

    private Seeker seeker;
    private Rigidbody2D rb;
    private PlayerController playerController;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        InvokeRepeating("UpdatePath", 0, .5f);
    }

    private void UpdatePath()
    {
        if(seeker.IsDone() && target) seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    private void Update()
    {
        if (path == null || !GameManager.instance.playable) return;

        if (isNeedLine) DrawPath();
        else pathRenderer.positionCount = 0;

        if (!playerController.isControlledByBot && !playerController.isEater) return;

        if (currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else reachedEndOfPath = false;

        desireDirection = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized;

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);

        if (distance < nextWayPointDistance) currentWayPoint++;
    }

    private void DrawPath()
    {
        pathRenderer.positionCount = path.vectorPath.Count;
        for (int i = 0; i < pathRenderer.positionCount; i++)
        {
            pathRenderer.SetPosition(i, path.vectorPath[i]);
        }
    }
}
