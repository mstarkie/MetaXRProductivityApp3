using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using LearnXR.Core.Utilities;
using System.Linq;
using TMPro;
using LearnXR.Core;

public class MeasureTapeFeature : Singleton<MeasureTapeFeature>
{
    [Range(0.005f, 0.05f)]
    [SerializeField] private float tapeWidth = 0.01f;
    /* Press to start drawing tape.  Press again to top drawing tape. */
    [SerializeField] private OVRInput.Button tapeActionButton;
    [SerializeField] private Material tapeMaterial;
    /* Holds the measurement info */
    [SerializeField] private GameObject measurementInfoPrefab;
    /* so the info text doesn't visually overlap the controller */
    [SerializeField] private Vector3 measurementInfoControllerOffset = new Vector3(0, 0.045f, 0);
    /* format of the measurement info display */
    [SerializeField] private string measurementInfoFormat = "<mark=#0000005A padding=\"20, 20, 10, 10\"><color=white>{0}</color></mark>";
    [SerializeField] private float measurementInfoLength = 0.01f;
    /* position of controllers so we know where to draw points, etc. */
    [SerializeField] private Transform leftControllerTapeArea;
    [SerializeField] private Transform rightControllerTapeArea;

    /* save the lines that we draw */
    private List<MeasuringTape> savedTapeLines = new();
    /* point to the last line rendered so we know which one to adjust */
    private LineRenderer lastTapeLineRenderer;
    /* pint to the last measured info of the last line rendered */
    private TextMeshPro lastMeasurementInfo;
    /* only allow 1 controller at a time */
    private OVRInput.Controller? currentController;
    /* ensure the displayed info is always pointing at the camera (added Billboard Alignment prefab to MeasurementInfo */
    private OVRCameraRig cameraRig;

    private void Awake()
    {
        cameraRig = FindObjectOfType<OVRCameraRig>();
    }


    // Update is called once per frame
    void Update()
    {
        HandleControllerActions(OVRInput.Controller.LTouch, leftControllerTapeArea);
        HandleControllerActions(OVRInput.Controller.RTouch, rightControllerTapeArea);
    }

    private void HandleControllerActions(OVRInput.Controller controller, Transform tapeArea)
    {
        if (currentController != controller && currentController != null) return;

        /* push the button */
        if (OVRInput.GetDown(tapeActionButton, controller))
        {
            currentController = controller;
            HandleDownAction(tapeArea);
        }

        /* hold the button */
        if (OVRInput.Get(tapeActionButton, controller))
        {
            HandleHoldAction(tapeArea);
        }

        /* release the button */
        if (OVRInput.GetUp(tapeActionButton, controller))
        {
            currentController = null;
            HandleUpAction(tapeArea);
        }
    }
    /* New tape line is created when button pushed */
    private void HandleDownAction(Transform tapeArea)
    {
        CreateNewTapeLine(tapeArea.position);
        AttachAndDetachMeasurementInfo(tapeArea);
    }
    /* ending position of line set when held */
    private void HandleHoldAction(Transform tapeArea)
    {
        lastTapeLineRenderer.SetPosition(index: 1, position: tapeArea.position);
        CalculateMeasurements();
        AttachAndDetachMeasurementInfo(tapeArea);
    }
    private void HandleUpAction(Transform tapeArea)
    {
        AttachAndDetachMeasurementInfo(tapeArea, attachToController: false);
    }

    /* creates a line renderer */
    private void CreateNewTapeLine(Vector3 initialPosition)
    {
        // create some tape lines and give them names
        var newTapeLine = new GameObject(name: $"TapeLine_{savedTapeLines.Count}", components: typeof(LineRenderer));

        lastTapeLineRenderer = newTapeLine.GetComponent<LineRenderer>();
        lastTapeLineRenderer.positionCount = 2; // num of vertices
        lastTapeLineRenderer.startWidth = tapeWidth;
        lastTapeLineRenderer.endWidth = tapeWidth;
        lastTapeLineRenderer.material = tapeMaterial;
        lastTapeLineRenderer.SetPosition(index: 0, position: initialPosition);

        // the button is down so a new measurement will contain only defaults
        lastMeasurementInfo = Instantiate(measurementInfoPrefab, Vector3.zero, Quaternion.identity)
            .GetComponent<TextMeshPro>();
        lastMeasurementInfo.gameObject.SetActive(false); // not active while a button is pushed.
        lastMeasurementInfo.GetComponent<BillboardAlignment>().AttachTo(cameraRig.centerEyeAnchor); // attach to center line of camera so it faces user

        // store the new tape line
        savedTapeLines.Add(new MeasuringTape
        {
            TapeLine = newTapeLine,
            TapeInfo = lastMeasurementInfo
        });
    }

    /** Keeps track of label locations */
    private void AttachAndDetachMeasurementInfo(Transform tapeArea, bool attachToController = true)
    {
        // down and hold: attach to controller while we're doing a measurement
        if (attachToController)
        {
            lastMeasurementInfo.gameObject.SetActive(true);
            // info and tape area in lock-step 
            lastMeasurementInfo.transform.SetParent(tapeArea.transform.parent);
            // set position of the text display to the pre-defined offset
            lastMeasurementInfo.transform.localPosition = measurementInfoControllerOffset;
        }
        // up: otherwise place the info between both points
        else
        {
            // associates the measurement info position with the last line rendered position
            lastMeasurementInfo.transform.SetParent(lastTapeLineRenderer.transform);
            var lineDirection = lastTapeLineRenderer.GetPosition(index: 0) - lastTapeLineRenderer.GetPosition(index: 1);
            Vector3 lineCrossProduct = Vector3.Cross(lhs:lineDirection, rhs:Vector3.up);

            // mid point calculation
            Vector3 lineMidPoint = (lastTapeLineRenderer.GetPosition(index: 0) +
                                    lastTapeLineRenderer.GetPosition(index: 1)) / 2.0f;

            // keeps the measurement info from being obstructed by the tape
            lastMeasurementInfo.transform.position = lineMidPoint + (lineCrossProduct.normalized * measurementInfoLength);
        }
    }

    private void CalculateMeasurements()
    {
        var distance = Vector3.Distance(lastTapeLineRenderer.GetPosition(index: 0), lastTapeLineRenderer.GetPosition(index: 1));
        var inches = MeasuringTape.MetersToInches(distance);
        var centimeters = MeasuringTape.MetersToCentimeters(distance);
        var lastLine = savedTapeLines.Last();
        //lastLine.TapeInfo.text = string.Format(measurementInfoFormat, $"{inches:F2} <i>{centimeters:F2}cm</i>");
        lastLine.TapeInfo.text = string.Format(measurementInfoFormat, $"{inches:F2} IN");

    }

    private void OnDestroy() => ClearTapeLines();
 
    public void ClearTapeLines()
    {
        foreach (var tapeLine in savedTapeLines)
        {
            Destroy(tapeLine.TapeLine);
        }
        savedTapeLines.Clear();
    }
}