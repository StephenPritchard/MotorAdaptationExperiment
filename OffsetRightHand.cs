using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetRightHand : MonoBehaviour
{
    public bool HandOffsetFlag { get; set; }
    public float HandOffsetInCm { get; set; }

    // Use this for initialization
    private void Start ()
	{
        HandOffsetFlag = false;
    }
	
	// Update is called once per frame
    private void Update ()
    {
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    HandOffsetFlag = !HandOffsetFlag;
        //}

        if (!HandOffsetFlag) return;
        HandOffset(HandOffsetInCm);
    }

    public void HandOffset(float offset)
    {
        transform.position += new Vector3((offset / 100), 0f, 0f);
    }
}
