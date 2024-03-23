using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineUpdater : MonoBehaviour
{
    [SerializeField] private LineRenderer lr;
    [SerializeField] private Transform camera;
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = camera.position;
        transform.rotation = camera.rotation;

        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 oldPos = lr.GetPosition(i);
            oldPos += transform.position;
        }
    }
}
