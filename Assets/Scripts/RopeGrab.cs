using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grappling: MonoBehaviour
{
    [Header("References")]

    private Player pm;
    public Transform cam;
    public Transform ThrowingHand;
    public LayerMask whatIsGrabable;


    [Header("Grappling")]

    public float maxGrabDistance;
    public float grabDelayTime;

    private Vector3 GrapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling; 

    private void Start()
    {
        pm = GetComponent<Player>();
       
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey));

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }
    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = true;

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrabDistance, whatIsGrabable))
        {
            GrapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grabDelayTime);
        }
        else
        {
            GrapplePoint = cam.position + cam.forward * maxGrabDistance;

            Invoke(nameof(StopGrapple), grabDelayTime);
        }
    }

    private void ExecuteGrapple()
    {

    }
    private void StopGrapple()
    {
        grappling = false;

        grapplingCdTimer = grapplingCd;
    }

}