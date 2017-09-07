using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using UnityEngine.VR;

public class VRChanges : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        //QualitySettings.antiAliasing = 8;
	}

	void Update ()
	{
        OVRInput.Update();

		// Recenter pose to default position
		if(Input.GetKeyDown(KeyCode.R))
		{
            InputTracking.Recenter();
		}
	}
}
