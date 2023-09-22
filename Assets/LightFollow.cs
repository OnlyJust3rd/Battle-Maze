using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFollow : MonoBehaviour
{
    public Transform follow;
    public float smoothTime;
    private Vector3 velocity;

    private void LateUpdate()
    {
        // Smoothly move the camera towards that target position
        transform.position = Vector3.SmoothDamp(transform.position, follow.position, ref velocity, smoothTime);
    }
}
