using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasControl : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            transform.position += (new Vector3(0f, 0.01f, 0f));
            if (transform.position.y > 2f)
                transform.position -= (new Vector3(0f, 0.01f, 0f));
        }
        if (!Input.GetKeyDown(KeyCode.K)) return;
        transform.position -= (new Vector3(0f, 0.01f, 0f));
        if (transform.position.y < -1.2f)
            transform.position += (new Vector3(0f, 0.01f, 0f));
    }
}
