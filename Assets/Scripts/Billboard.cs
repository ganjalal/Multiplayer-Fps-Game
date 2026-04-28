using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    private Camera cam;
    
    void Start()
    {   
        // if(Camera.allCameras.Length > 0)
            // cam = Camera.allCameras[0];
            StartCoroutine(WaitAndFindCameras());
    }

    IEnumerator WaitAndFindCameras()
    {
        while (Camera.allCameras.Length == 0) 
        {
        yield return null; 
        }
        cam = Camera.allCameras[0];
    }
    
    void LateUpdate()
    {
        if (cam != null)
        {
            // Always face camera
            transform.LookAt(cam.transform);
            transform.Rotate(0, 180, 0); // Flip to face correctly
        }
    }
}
