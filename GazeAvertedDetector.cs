using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GazeAvertedDetector : MonoBehaviour
{
    public bool IsActive { get; private set; }

    public UnityEvent OnActivate;
    public UnityEvent OnDeactivate;
    public UnityEvent OnGazeAverted;
    public UnityEvent OnGazeGood;

    public float Period = .1f; //seconds

    private int _horizontalLimit;
    private int _verticalLimit;

    private IEnumerator GazeAvertedCoroutine;

    public GazeAvertedDetector()
    {
        IsActive = false;
    }

    private void Awake()
    {
        GazeAvertedCoroutine = GazeAversionWatcher();
    }
    private void Start()
    {
        _horizontalLimit =
            GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>().PermittedHorizontalDeviation;
        _verticalLimit =
            GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>().PermittedVerticalDeviation;
    }

    private IEnumerator GazeAversionWatcher()
    {
        var gazeAverted = false;
        while (true)
        {
            var xAngleDeviation = transform.eulerAngles.x;
            var yAngleDeviation = transform.eulerAngles.y;

            if (xAngleDeviation > 180)
            {
                xAngleDeviation = xAngleDeviation - 360;
            }
            xAngleDeviation = Math.Abs(xAngleDeviation);
            if (yAngleDeviation > 180)
            {
                yAngleDeviation = yAngleDeviation - 360;
            }
            yAngleDeviation = Math.Abs(yAngleDeviation);


            if (((xAngleDeviation > _verticalLimit) || (yAngleDeviation > _horizontalLimit)) && !gazeAverted)
            {
                gazeAverted = true;
                OnGazeAverted.Invoke();
            }
            else if ((xAngleDeviation < (_verticalLimit*0.9f)) && (yAngleDeviation < (_horizontalLimit*0.9f)) && gazeAverted)
            {
                gazeAverted = false;
                OnGazeGood.Invoke();
            }

            if (gazeAverted)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
            yield return new WaitForSeconds(Period);
        }
    }

    private void OnEnable()
    {
        StopCoroutine(GazeAvertedCoroutine);
        StartCoroutine(GazeAvertedCoroutine);
    }


    private void OnDisable()
    {
        StopCoroutine(GazeAvertedCoroutine);
        Deactivate();
    }

    
	public virtual void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        OnActivate.Invoke();
    }

    public virtual void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        OnDeactivate.Invoke();
    }
}
