using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using VRC.Dynamics;
    using UnityEditor;
#endif

public class DynamicsCam : MonoBehaviour
{

    public Transform focus;
    public bool focusHead = true;
    public Vector3 focusOffset = new Vector3(0,0,0.5f);
    public GameObject senderPrefab;
    public float minRadiusSize = 0.003f;
    public float moveSpeed = 0.75f;
    public float shiftSpeedMultiplier = 3f;
    
    public float verticalSpeed = 1f;
    public float rotateSpeed = 3;


    float defaultMoveSpeed;
    float pitch;
    float yaw;
    float radiusSize;
    float maxRadiusSize = 0f;
    Transform root;
    Camera cam;

    GameObject sender;
    //ContactReceiver[] receivers;

    public void SetFocus(Transform f)
    {
        Debug.Log($"Set new focus to {f.name}");
        this.focus = f;
        try{
            Transform parent = focus.parent;
            while (parent.parent) parent = parent.parent;
            this.root = parent;
        } catch(System.Exception e) {}
    }
    void Start()
    {
        
        #if UNITY_EDITOR

        if (!focus) Debug.LogAssertion("DynamicsCam: No focus was set, the script will not work properly.");

        if (focusHead) {
            root = focus;
            while (root.parent) root = root.parent;
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
            Transform parent = receiver.transform;
            if (receiver.transform.parent)
            {
                while (parent.parent)
                {
                    parent = parent.parent;
                }
            }
            if (parent.gameObject.activeSelf)
            {
                SphereCollider col = receiver.gameObject.AddComponent<SphereCollider>();
                if (col.radius > maxRadiusSize) maxRadiusSize = col.radius;
                col.radius = radiusSize = receiver.radius * (0.95f);
            }
        }
        #endif
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
                
                if (senderPrefab)
                {
                    Debug.Log("Generated Sender Object");
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
            PlayerPrefs.SetFloat("radiusSize", radiusSize);
            foreach (ContactReceiver rec in receivers)
            {
                SphereCollider col = rec.GetComponent<SphereCollider>();
                col.radius = Mathf.Clamp(radiusSize, minRadiusSize, rec.radius);
            }
        }
        
        #if UNITY_EDITOR
        if (Input.GetKeyDown("f"))
        {
            Selection.SetActiveObjectWithContext(root, root);
        }
        #endif
    }
}
