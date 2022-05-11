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
    [Help(" * Right click objects in the Hierchy tab to select a focus (Set DynamicsCam Focus)\n\n * Edit the senderPrefab to change collision tags\n\n * Turn on gizmos in game view for a visual representation of collider and contact sizes.")]
    [Tooltip("The focusable object, right click objects in Hierachy tab to set the focus easily (Set DynamicsCam Focus)")]
    public Transform focus;
    [Tooltip("Offset of the camera to the focus object when pressing R")]
    public Vector3 focusOffset = new Vector3(0,0,0.5f);
    [Tooltip("Enabled to focus on the head instead of root of objects when pressing R")]
    public bool focusHead = true;
    
    [Header("ContactSender Settings")]
    [Tooltip("Customizable sender prefab.  Edit this to change contact collision tags.")]
    public GameObject senderPrefab;
    [Tooltip("The minimum size the contact receiver will be able to shrink to.")]
    public float minRadiusSize = 0.003f;
    
    [Header("Movement Settings")]
    [Tooltip("Speed of the vertical camera movement (Q & E)")]
    public float verticalSpeed = 1f;
    [Tooltip("Speed of the horizontal camera movement (WASD)")]
    public float moveSpeed = 0.75f;
    [Tooltip("Multiplayer of movespeed when pressing (Left Shift)")]
    public float shiftSpeedMultiplier = 3f;
    [Tooltip("Speed of camera rotation when holding down right click.")]
    public float rotateSpeed = 3;


    float defaultMoveSpeed;
    float pitch;
    float yaw;
    float radiusSize;
    float maxRadiusSize = 0f;
    Transform root;
    Camera cam;

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
        // Probably a better way to handle this, will look into it.
        if (GameObject.Find("VRCSDK"))
        {
            this.enabled = false;
            return;
        }

        if (!focus) Debug.LogAssertion("DynamicsCam: No focus was set, the script will not work properly.");

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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed =  shiftSpeedMultiplier * defaultMoveSpeed;
        }
        else moveSpeed = defaultMoveSpeed;

        float hori = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");

        if (vert != 0)
        {
            //transform.localPosition += moveSpeed * new Vector3(-Input.GetAxisRaw("Horizontal"), 0, -Input.GetAxisRaw("Vertical")) * Time.deltaTime;
            if (vert < 0) transform.Translate(-Vector3.forward * Time.deltaTime * moveSpeed);
            if (vert > 0) transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        }

        if (hori != 0)
        {
            if (hori < 0) transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
            if (hori > 0) transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        }
        
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
                Debug.DrawLine(ray.origin, hit.point, Color.red);
                
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

        if (Input.GetKey("q"))
        {
            transform.Translate(verticalSpeed * -Vector3.up * Time.deltaTime);
            //transform.position = new Vector3(transform.position.x, transform.position.y + -verticalSpeed, transform.position.z);
        }

        if (Input.GetKey("e"))
        {
            transform.Translate(verticalSpeed * Vector3.up * Time.deltaTime);
            //transform.position = new Vector3(transform.position.x, transform.position.y + verticalSpeed, transform.position.z);
        }

        if (Input.GetKeyDown("r"))
        {
            transform.position = focus.position + focusOffset;
            transform.LookAt(focus);
            pitch = transform.eulerAngles.x;
            yaw = transform.eulerAngles.y;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0 && Input.GetKey(KeyCode.LeftControl))
        {
            ContactReceiver[] receivers = GameObject.FindObjectsOfType<ContactReceiver>();
            radiusSize += 0.01f * Input.GetAxis("Mouse ScrollWheel");
            radiusSize = Mathf.Clamp(radiusSize, 0f, maxRadiusSize);
            foreach (ContactReceiver rec in receivers)
            {   
                SphereCollider col = rec.GetComponentInChildren<SphereCollider>();
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