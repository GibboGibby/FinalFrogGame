using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TongueAimer : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Transform camera;
    [SerializeField] private FrogTonueController ftc;
    [SerializeField] private Image img;
    [SerializeField] private float minDist = 1.5f;
    private float maxDistance = 100.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        bool hitGood = false;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance))
        {
            if (hit.transform.gameObject.CompareTag("GrapplePoint"))
            {
                hitGood = true;
                float dist = Mathf.Abs(Vector3.Distance(hit.transform.position, camera.position));
                //Debug.Log("This is the dist - " + dist);
                if (Mathf.Abs(Vector3.Distance(hit.transform.position, camera.transform.position)) < minDist) hitGood = false;
            }
            
        }
        else
        {
            hitGood = false;
        }

        if (hitGood)
        {
            if (img.enabled == false)
                img.enabled = true;
            transform.position = hit.point;
            transform.LookAt(camera);
        }
        else
        {
            if (img.enabled == true)
                img.enabled = false;
        }
    }
}
