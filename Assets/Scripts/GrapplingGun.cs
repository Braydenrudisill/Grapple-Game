using UnityEngine;

public class GrapplingGun : MonoBehaviour {

    private LineRenderer lr;
    public Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    private float maxDistance = 100f;
    public float grappleSpeed;
    private SpringJoint joint;
    public bool Stuck = false;
    public CapsuleCollider capCollider;

    private bool isColliding;

    public float constantPullAmount;

    private PlayerMovement pm;
    Rigidbody rb;

    void Awake() {
        lr = GetComponent<LineRenderer>();
        rb = player.GetComponent<Rigidbody>();
        pm = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Stuck && pm.isColliding)
        {
            Debug.Log("Reseting jump", this);
            pm.ResetJump();
            pm.Ground();

        }

        HandleInputs();

        if (joint == null) { return; }

        var scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        joint.maxDistance -= scrollAmount * 5;
        joint.spring += scrollAmount * 5;




        // To get rid of pushback, set the maxdistance to a little less than
        // the distance between player and point if the maxdistance > current_dist
        if (Vector3.Distance(transform.position, grapplePoint) < joint.maxDistance)
        {
            joint.maxDistance = Vector3.Distance(transform.position, grapplePoint) - constantPullAmount;
        }


    }


    private void HandleInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();


        }
        if (Input.GetMouseButton(0))
        {
            if (Stuck)
            {
                StickRope();
            }
        }
        else
            UnstickRope();
        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        // Joint related inputs require joint

        if (joint == null) { return; }

        if (Input.GetMouseButtonDown(1))
        {
            StickRope();
        }
        var scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        joint.maxDistance -= scrollAmount * 5;
        joint.spring += scrollAmount * 5;
    }

    //Called after Update
    void LateUpdate() {
        DrawRope();

    }

   
    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable)) {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.001f;

            //Adjust these values to fit your game.
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }



    void StopGrapple() {
        lr.positionCount = 0;
        Destroy(joint);
        UnstickRope();
    }

    private Vector3 currentGrapplePosition;
    
    void DrawRope() {
        //If not grappling, don't draw rope
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling() {
        return joint != null;
    }

    void ReelRope()
    {
        if (joint == null) { return; }
        joint.maxDistance = 0.03f;
        joint.minDistance = 0.001f;
        joint.spring = 18f;

    }
    void StickRope()
    {

        StopGrapple();

       
        rb.velocity = new Vector3(0,0,0);
        rb.useGravity = false;
        Vector3 dirNormalized = (grapplePoint - player.transform.position);
        rb.AddForce(dirNormalized*200);
        Stuck = true;

    }
    public void UnstickRope()
    {
        if (!Stuck) { return; }

        rb.velocity = new Vector3(0, 0, 0);
        rb.useGravity = true;
        Stuck = false;

    }
}
