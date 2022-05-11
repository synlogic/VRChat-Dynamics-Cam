#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Dynamics;
using UnityEditor;
using UnityEngine.SceneManagement;

public class DynamicsCam : MonoBehaviour
{
    // Help box uses https://github.com/johnearnshaw/unity-inspector-help/
    [SerializeField]
    [Help(" * Right click objects in the Hierchy tab to select a focus (Set DynamicsCam Focus)\n\n * Edit the sender prefab to change collision tags\n\n * Turn on gizmos in game view for a visual representation of collider and contact sizes.")]
    [Tooltip("The focusable object, right click objects in Hierachy tab to set the focus easily (Set DynamicsCam Focus)")]
    public Transform focus;
    [Tooltip("Offset of the camera to the focus object when pressing R")]
    public Vector3 focusOffset = new Vector3(0,0,0.5f);
    [Tooltip("Enabled to focus on the head instead of root of objects when pressing R")]
    public bool focusHead = true;
    
    [Header("Contact Sender Settings")]
    [Tooltip("Customizable sender prefab.  Edit this to change contact collision tags.")]
    public GameObject senderPrefab;
    [Tooltip("The minimum size the contact receiver will be able to shrink to.")]
    public float minRadiusSize = 0.003f;
    
    [Header("Movement Settings")]
    [Tooltip("Speed of the horizontal camera movement (WASD)")]
    public float moveSpeed = 0.6f;

    [Tooltip("Speed of camera acceleration in any given direction.")]
    public float accelerationSpeed = 0.075f;
    [Tooltip("Multiplayer of movespeed when pressing (Left Shift)")]
    public float shiftSpeedMultiplier = 3f;
    [Tooltip("Speed of camera rotation when holding down right click.")]
    public float rotateSpeed = 3;

    Vector3 acceleration = new Vector3(0,0,0);
    float defaultAccelerationSpeed;
    float defaultMoveSpeed;
    float pitch;
    float yaw;
    float radiusSize;
    float maxRadiusSize = 0f;
    Transform root;
    Camera cam;
    bool collidersFixed = false;
    GameObject sender;

    Transform GetRoot(Transform p)
    {
        if (p.parent) while(p.parent) p = p.parent;
        return p;
    }

    public void SetFocus(Transform f)
    {
        Debug.Log($"Set new DynamicsCam focus to {f.name}");
        this.focus = f;
        this.root = GetRoot(focus);
    }
    void Start()
    {
        if (GameObject.Find("VRCSDK"))
        {
            this.enabled = false;
            return;
        }

        if (!focus) 
        {
            Debug.LogError("DynamicsCam: Focus object is required for this script to work.  Right click your avatar in the hierarchy tab and select Set DynamicsCam Focus.");
            this.enabled = false;
            return;
        }

        if (gameObject.tag != "MainCamera")
        {
            Debug.LogError("DynamicsCam: This script need to be on a camera with the tag MainCamera.");
            this.enabled = false;
            return;
        }

        if (focusHead) {
            root = GetRoot(focus);
            foreach (Transform t in root.gameObject.GetComponentsInChildren<Transform>())
            {
                if (t.name.ToLower() == "head") focus = t;
            }
        }

        //Disable other cameras to keep physbones happy
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam != this.gameObject.GetComponent<Camera>()) cam.gameObject.SetActive(false);
        }

        defaultMoveSpeed = moveSpeed;
        defaultAccelerationSpeed = accelerationSpeed;
        cam = SceneView.GetAllSceneCameras()[0];
        this.transform.position = cam.transform.position;
        this.transform.rotation = cam.transform.rotation;
        pitch = this.transform.rotation.eulerAngles.x;
        yaw = this.transform.rotation.eulerAngles.y;
    
        ContactReceiver[] receivers = GameObject.FindObjectsOfType<ContactReceiver>();
        foreach (ContactReceiver receiver in receivers)
        {
            Transform parent = GetRoot(receiver.transform);
            if (parent.gameObject.activeSelf)
            {
                GameObject colObj = new GameObject("ReceiverCollider");
                colObj.transform.SetParent(receiver.transform, false);
                SphereCollider col = colObj.AddComponent<SphereCollider>();
                if (col.radius > maxRadiusSize) maxRadiusSize = col.radius;
                col.radius = radiusSize = receiver.radius * (0.1f);
            }
        }
    }

    void setupColliders()
    {
        ContactReceiver[] recs = GameObject.FindObjectsOfType<ContactReceiver>();
        foreach (ContactReceiver rec in recs)
        {
            if (rec.GetComponentInChildren<SphereCollider>())
            {
                SphereCollider col = rec.GetComponentInChildren<SphereCollider>();
                col.radius = 0.9f * rec.radius;
                Debug.Log("Updated Sphere Collider sizes");
            }
        }
    }

    float Direction(float x)
    {
        if (x > 0f) return 1;
        else if (x < 0f) return -1;
        else return 0;
    }

    Vector3 Round0(Vector3 v)
    {
        for (int i = 0; i < 3; i++)
        {
            if (v[i] < accelerationSpeed && v[i] > -accelerationSpeed) v[i] = 0;
        }
        return v;
    }
    // Update is called once per frame
    void Update()
    {
        // Workaround for a unity bug
        if (!collidersFixed) {setupColliders(); collidersFixed = true;}

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed =  shiftSpeedMultiplier * defaultMoveSpeed;
            accelerationSpeed = shiftSpeedMultiplier * defaultAccelerationSpeed;
        }
        else 
        {
            moveSpeed = defaultMoveSpeed;
            accelerationSpeed = defaultAccelerationSpeed;
        }

        float hori = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");
        
        float slowdownSpeed = accelerationSpeed / 2;

        for (int i = 0; i < 3; i++)
        {
            acceleration[i] += slowdownSpeed * -Direction(acceleration[i]);
        }
        
        // Probably a better way to do this but this works for now.
        if (acceleration.x < moveSpeed && acceleration.x > -moveSpeed) acceleration.x += accelerationSpeed * hori;
        if (acceleration.z < moveSpeed && acceleration.z > -moveSpeed) acceleration.z += accelerationSpeed * vert;
        if (Input.GetKey("q")) if (acceleration.y > -moveSpeed) acceleration.y -= accelerationSpeed;
        if (Input.GetKey("e")) if (acceleration.y < moveSpeed) acceleration.y += accelerationSpeed;

        acceleration = Round0(acceleration);
        transform.Translate(acceleration * Time.deltaTime);
        
        if (Input.GetMouseButton(1))
        {
            yaw += rotateSpeed * Input.GetAxis("Mouse X");
            pitch -= rotateSpeed * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        if (Input.GetMouseButtonDown(2))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {   
                if (senderPrefab && !hit.transform.GetComponent<ContactReceiver>())
                {
                    sender = Instantiate(senderPrefab, hit.point, Quaternion.identity);
                }
            }
        }

        if (Input.GetMouseButtonUp(2))
        {
            GameObject.DestroyImmediate(sender);
        }

        if (Input.GetKeyDown("r"))
        {
            transform.position = focus.position + focusOffset;
            transform.LookAt(focus);
            pitch = transform.eulerAngles.x;
            yaw = transform.eulerAngles.y;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            ContactReceiver[] receivers = GameObject.FindObjectsOfType<ContactReceiver>();
            foreach (ContactReceiver rec in receivers)
            {
                SphereCollider col = rec.GetComponentInChildren<SphereCollider>();        
                radiusSize = col.radius;
                radiusSize += 0.01f * Input.GetAxis("Mouse ScrollWheel");
                radiusSize = Mathf.Clamp(radiusSize, 0f, maxRadiusSize);
                col.radius = Mathf.Clamp(radiusSize, minRadiusSize, rec.radius);
            }
        }

        if (Input.GetKeyDown("f"))
        {
            Selection.SetActiveObjectWithContext(root, root);
        }
    }
}
#endif