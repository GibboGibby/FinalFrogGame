using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
public class FrogTonueController : MonoBehaviour
{
    [SerializeField] private InputActionReference iar;
    [SerializeField] private InputActionReference bButton;
    [SerializeField] private LineRenderer lineRenderer;

    private Vector3 grapplePoint;
    [SerializeField] public LayerMask whatIsGrappleable;
    [SerializeField] private Transform tonguePos, camera, player;
    [SerializeField] private float maxDistance = 100.0f;
    [SerializeField] private float pullForce;
    private SpringJoint joint;

    UnityEngine.XR.InputDevice leftHandDevice;
    UnityEngine.XR.InputDevice rightHandDevice;


    bool rightGrip = false;
    bool leftGrip = false;

    List<UnityEngine.XR.InputDevice> leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
    UnityEngine.XR.InputDeviceCharacteristics leftCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
    List<UnityEngine.XR.InputDevice> rightHandedControllers = new List<UnityEngine.XR.InputDevice>();
    UnityEngine.XR.InputDeviceCharacteristics rightCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;

    [Header("Physics Hands")]
    [SerializeField] private BoxCollider leftHandCollider;
    [SerializeField] private BoxCollider rightHandCollider;

    [SerializeField] private PhysicsHand leftHandController;
    [SerializeField] private PhysicsHand rightHandController;



    // Start is called before the first frame update
    void Start()
    {
        UpdateHandsTracker();
    }

    public bool isGripping(bool isRightHand)
    {
        if (isRightHand) return rightGrip;
        else return leftGrip;
    }
    
    void UpdateHandsTracker()
    {
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(leftCharacteristics, leftHandedControllers);
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(rightCharacteristics, rightHandedControllers);
        if (leftHandedControllers.Count > 0)
            leftHandDevice = leftHandedControllers[0];
        if (rightHandedControllers.Count > 0)
            rightHandDevice = rightHandedControllers[0];
    }

    void UpdateGrabHands()
    {
        if (leftHandedControllers.Count == 0 || rightHandedControllers.Count == 0)
        {
            UpdateHandsTracker();
        }

        leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool lGripButtonValue);
        rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool rGripButtonValue);
        leftGrip = lGripButtonValue;
        rightGrip = rGripButtonValue;
    }

    public bool isHandGrabbing(bool isRightHand)
    {
        if (isRightHand) return rightGrip && rightHandController.IsColliding();
        else return leftGrip && leftHandController.IsColliding();
    }
    // Update is called once per frame
    void Update()
    {
        UpdateGrabHands();
        if (leftGrip && rightGrip)
        {
            Debug.Log("Grabbing with both hands");
        }
        /*
        if (leftGrip && leftHandCollider.enabled == false)
        {
            leftHandCollider.enabled = true;
        }
        if (rightGrip && rightHandCollider.enabled == false)
        {
            rightHandCollider.enabled = true;
        }

        if (!leftGrip && !rightGrip)
        {
            rightHandCollider.enabled = true;
            leftHandCollider.enabled = true;
        }
         */
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    public void GrabbedHand(bool isRightHand)
    {
        if (isRightHand)
        {
            leftHandCollider.enabled = false;
            leftHandController.ForciblyStopCollisions();
        }
        else
        {
            rightHandCollider.enabled = false;
            rightHandController.ForciblyStopCollisions();
        }
    }

    public void HandLetGo(bool isRightHand)
    {

    }

    private void OnEnable()
    {
        iar.action.performed += Tongue;
        bButton.action.performed += LaunchTowardsGrapplePoint;
    }
    private bool tongueOut = false;

    private void LaunchTowardsGrapplePoint(InputAction.CallbackContext context)
    {
        if (tongueOut)
        {
            Vector3 launchDir = grapplePoint - player.transform.position;
            player.gameObject.GetComponent<Rigidbody>().AddForce(launchDir.normalized * pullForce, ForceMode.Acceleration);
            StopGrapple();
            tongueOut = false;
        }
    }
    private void Tongue(InputAction.CallbackContext context)
    {
        Debug.Log("Tongued");
        if (tongueOut)
        {
            lineRenderer.enabled = false;
            StopGrapple();
            tongueOut = !tongueOut;
            //StartCoroutine(TongueMovement(10, 0, 1));
        }
        else
        {
            lineRenderer.enabled = true;
            StartGrapple();
            //tongueOut = !tongueOut;
            //StartCoroutine(TongueMovement(0, 10, 1));
        }

        
    }




    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            tongueOut = true;
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.35f;

            joint.spring = 6.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lineRenderer.positionCount = 2;
        }
    }

    void DrawRope()
    {
        if (!joint) return;
        lineRenderer.SetPosition(0, tonguePos.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void StopGrapple()
    {
        lineRenderer.positionCount = 0;
        Destroy(joint);
    }

    private IEnumerator TongueMovement(int start, int end, float time)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < time)
        {
            Vector3 oldSize = lineRenderer.GetPosition(1);
            oldSize.z += ((float)(end - start)) * elapsedTime / time;
            lineRenderer.SetPosition(1, oldSize);
            yield return null;
        }
    }

    void Tongue()
    {

    }
}
