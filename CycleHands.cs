using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

public class CycleHands : MonoBehaviour
{
	public int currentHandIndex;
	public GameObject[] hands;
	public GameObject currentHand;

	// Use this for initialization
	void Start ()
	{
		currentHand = Instantiate (hands [currentHandIndex]);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			DestroyImmediate(currentHand);
			currentHandIndex++;
			if (currentHandIndex == hands.Length)
				currentHandIndex = 0;
			currentHand = Instantiate(hands[currentHandIndex]);
		}

		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			DestroyImmediate(currentHand);
			currentHandIndex--;
			if (currentHandIndex < 0)
				currentHandIndex = hands.Length - 1;
			currentHand = Instantiate(hands[currentHandIndex]);
		}

		// Move hand around
		if(Input.GetKeyDown(KeyCode.I))
		{
			currentHand.transform.position += new Vector3 (0f, 0f, 0.01f);
		}
		if(Input.GetKeyDown(KeyCode.K))
		{
			currentHand.transform.position -= new Vector3 (0f, 0f, 0.01f);
		}
		if(Input.GetKeyDown(KeyCode.L))
		{
			currentHand.transform.position += new Vector3 (0.01f, 0f, 0f);
		}
		if(Input.GetKeyDown(KeyCode.J))
		{
			currentHand.transform.position -= new Vector3 (0.01f, 0f, 0f);
		}
		if(Input.GetKeyDown(KeyCode.N))
		{
			currentHand.transform.position += new Vector3 (0f, 0.01f, 0f);
		}
		if(Input.GetKeyDown(KeyCode.M))
		{
			currentHand.transform.position -= new Vector3 (0f, 0.01f, 0f);
		}
	}
}
