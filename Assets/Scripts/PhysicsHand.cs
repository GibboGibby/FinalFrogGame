using UnityEngine;

public class PhysicsHand : MonoBehaviour
{
    [Header("TongueController")]
    [SerializeField] private FrogTonueController controller;
    [SerializeField] private bool isRightHand = false;
    [Header("PID")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private float frequency = 50f;
    [SerializeField] private float damping = 1f;
    [SerializeField] private float rotFrequency = 100f;
    [SerializeField] private float rotDamping = 0.9f;
    [SerializeField] private Transform target;
    [Space]
    [Header("Springs")]
    [SerializeField] private float climbForce = 1000f;
    [SerializeField] private float climbDrag = 500f;
    private Vector3 _previousPosition;
    private Rigidbody _rigidbody;

    private Vector3 collisionPosition;
    private Quaternion collisionRotation;

    bool _isColliding = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = target.position;
        // I store rotation this way however I could just freeze the objects rotation and unfreeze later but this should work
        transform.rotation = target.rotation;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = float.PositiveInfinity;
        _previousPosition = transform.position;
    }

    // Update is called once per frame
    bool hasGrippedBefore = false;
    void FixedUpdate()
    {
        bool shouldHookesLaw = true;
        if (controller.isHandGrabbing(!isRightHand) && !controller.isHandGrabbing(isRightHand))
        {
            shouldHookesLaw = false;
        }
        
        if (controller.isGripping(isRightHand) && _isColliding)
        {
            transform.position = collisionPosition;
            transform.rotation = collisionRotation;
        }
        else
        {
            PIDMovement();
            PIDRotation();
            
        }

       
        /*
        if (controller.isGripping(isRightHand) && _isColliding && !hasGrippedBefore)
        {
            hasGrippedBefore = true;
            controller.GrabbedHand(isRightHand);
        }

        if (!controller.isGripping(isRightHand) && hasGrippedBefore)
        {
            hasGrippedBefore = false;
        }
         */
        
        if (_isColliding && shouldHookesLaw) HookesLaw();
    }

    private void LateUpdate()
    {
        if (controller.isGripping(isRightHand) && _isColliding)
        {
            transform.position = collisionPosition;
            transform.rotation = collisionRotation;
        }
    }

    void PIDMovement()
    {
        float kp = (6f * frequency) * (6f * frequency) * 0.25f;
        float kd = 4.5f * frequency * damping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Vector3 force = (target.position - transform.position) * ksg + (playerRigidbody.velocity - _rigidbody.velocity) * kdg;
        _rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    void PIDRotation()
    {
        float kp = (6f * rotFrequency) * (6f * rotFrequency) * 0.25f;
        float kd = 4.5f * rotFrequency * rotDamping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Quaternion q = target.rotation * Quaternion.Inverse(transform.rotation);

        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        q.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();
        axis *= Mathf.Deg2Rad;
        Vector3 torque = ksg * axis * angle + -_rigidbody.angularVelocity * kdg;
        _rigidbody.AddTorque(torque, ForceMode.Acceleration);
    }

    public bool IsColliding()
    {
        return _isColliding;
    }

    public void ForciblyStopCollisions()
    {
        _isColliding = false;
    }

    void HookesLaw(bool isGripping = false)
    {
        float newClimbForce = (isGripping) ? 200 : climbForce;
        float newClimbDrag = (isGripping) ? 350 : climbDrag;

        Vector3 displacementFromResting = transform.position - target.position;
        Vector3 force = displacementFromResting * newClimbForce;
        float drag = GetDrag();
        
        playerRigidbody.AddForce(force, ForceMode.Acceleration);
        playerRigidbody.AddForce(drag * -playerRigidbody.velocity * newClimbDrag, ForceMode.Acceleration);
    }

    float GetDrag()
    {
        Vector3 handVelocity = (target.localPosition - _previousPosition) / Time.fixedDeltaTime;
        float drag = 1 / handVelocity.magnitude + 0.01f;
        drag = drag > 1 ? 1 : drag;
        drag = drag < 0.03f ? 0.03f : drag;
        _previousPosition = transform.position;
        return drag;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
        collisionPosition = transform.position;
       
        collisionRotation = transform.rotation;
        if (controller.isGripping(isRightHand))
        {
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            _rigidbody.velocity = Vector3.zero;
            playerRigidbody.velocity = Vector3.zero;
            //_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
    private void OnCollisionExit(Collision collision) {  
        _isColliding = false;
        //_rigidbody.constraints = RigidbodyConstraints.None;
        
        collisionPosition = Vector3.zero;
        collisionRotation = Quaternion.identity;
    }
}
