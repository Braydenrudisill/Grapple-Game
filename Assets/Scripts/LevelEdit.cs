using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEdit : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 placePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    private float maxDistance = 500f;
    public float grappleSpeed;
    private SpringJoint joint;
    public bool Stuck = false;
    public bool onWall = false;
    public GameObject cubePrefab;
    Rigidbody rb;
    private Block selectedBlock;

    private GameObject ghostBlock;
    public Material gbmat;



    // Start is called before the first frame update
    void Start()
    {
        selectedBlock = new Block(new Vector3(0, 0, 0), "cube", Quaternion.identity);
        placePoint = new Vector3(0, 0, 0);
        ghostBlock = Instantiate(cubePrefab, placePoint, Quaternion.identity);
        ghostBlock.GetComponent<Renderer>().material = gbmat;
        ghostBlock.GetComponent<BoxCollider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        DrawGhostBlock();
        if (Input.GetMouseButtonDown(0))
        {
            PlaceBlock(selectedBlock);
        }

    }

    void PlaceBlock(Block b)
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            placePoint = hit.point;
            placePoint += hit.normal * selectedBlock.scale.x / 2;
            GameObject block = Instantiate(cubePrefab,placePoint,Quaternion.identity);
            Block blockb = new Block(placePoint, "block", Quaternion.identity);
        }
    }

    void DrawGhostBlock()
    {


        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            placePoint = hit.point;
            placePoint += hit.normal*selectedBlock.scale.x/2;
            //placePoint += hit.normal * 2;

            ghostBlock.transform.position = placePoint;
            ghostBlock.layer = 2;

        }
    }

}

[System.Serializable]
public class Block
{
    public Vector3 position;
    public string type;
    public Quaternion rotation;
    public Vector3 scale;

    public Block(Vector3 p, string t, Quaternion r)
    {
        this.position = p;
        this.type = t;
        this.rotation = r;
        if (this.type == "cube")
        {
            this.scale = new Vector3(4, 4, 4);
        }
    }

}



