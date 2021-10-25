using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
//using UnityEngine.VR;

//public Camera childCamera;

public class DisablePoseTracking : MonoBehaviour
{
    // Camera Left_eye;

    void Awake()
    {
        //Camera cam = gameObject.GetComponent<Left_eye>();
        //Left_eye = Camera.main;
        //Left_eye.enabled = true;
        // XRDevice.DisableAutoXRCameraTracking(Left_eye, true);

    }
    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        //VRDevice.DisableAutoVRCameraTracking(childCamera, True)
        // UnityEngine.XR.InputTracking.disableRotationalTracking = true;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = -UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
        transform.rotation = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye));
        //transform.position = -UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
        //transform.rotation = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye));
    }
}
