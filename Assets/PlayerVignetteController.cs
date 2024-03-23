using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerVignetteController : MonoBehaviour
{

    [SerializeField] private TunnelingVignetteController tunnelingVignetteController;
    [SerializeField] private LocomotionVignetteProvider locomotionVignetteProvider;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float minVelocity;
    [SerializeField] private float maxVelocity;

    [SerializeField] private bool shouldVignette;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    bool vignetteStarted = false;
    // Update is called once per frame
    void Update()
    {
        if (!shouldVignette) return;
        if (rb.velocity.magnitude > minVelocity)
        {
            Debug.Log("vignette should start");
            if (!vignetteStarted)
            {
                locomotionVignetteProvider.locomotionProvider.locomotionPhase = LocomotionPhase.Moving;
                tunnelingVignetteController.BeginTunnelingVignette(locomotionVignetteProvider);
                vignetteStarted = true;
            }
        }
        else
        {
            if (vignetteStarted)
            {
                locomotionVignetteProvider.locomotionProvider.locomotionPhase = LocomotionPhase.Done;
                tunnelingVignetteController.EndTunnelingVignette(locomotionVignetteProvider);
                vignetteStarted = false;
            }
        }
    }
}
