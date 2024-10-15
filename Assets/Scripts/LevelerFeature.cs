using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelerFeature : MonoBehaviour
{
    /* At what point the outer area of the level tool is going to be considered level (i.e., green). 
     * If the leveler angle value is between -5 and 5 then set it to green else use the original color. */
    [Range(1.0f, 10.0f)]
    [SerializeField] private float levelerTolerance = 5.0f;
    [SerializeField] TextMeshPro levelerReadingText;
    [SerializeField] Renderer levelerOuterRenderer;

    private Color levelerDefaultColor;
    void Awake() => levelerDefaultColor = levelerOuterRenderer.material.color;

    void Update()
    {
        // rotation on Z and X to require leveling
        Vector3 objectUp = transform.up;
        Vector3 worldUp = Vector3.up;
        //Vector3 objectRight = transform.right;
        //Vector3 worldRight = Vector3.right;
        //Vector3 objectForward = transform.forward;
        //Vector3 worldForward = Vector3.forward;

        int upAngle = Mathf.RoundToInt(f: Vector3.Angle(objectUp, worldUp));
        //int xAngle = Mathf.RoundToInt(f: Vector3.Angle(objectRight, worldRight));
        //int zAngle = Mathf.RoundToInt(f: Vector3.Angle(objectForward, worldForward));

        // a vector that is perpendicular to world and object.  Used to determine the sign.
        Vector3 crossProductUp = Vector3.Cross(lhs:worldUp, rhs:objectUp);
        if (crossProductUp.z < 0.0f) upAngle = -upAngle;

        //Vector3 crossProductRight = Vector3.Cross(lhs: worldRight, rhs: objectRight);
        //if (crossProductRight.z < 0.0f) xAngle = -xAngle;

        //Vector3 crossProductForward = Vector3.Cross(lhs: worldForward, rhs: objectForward);
        //if (crossProductRight.z < 0.0f) xAngle = -xAngle;


        levelerReadingText.text = $"{upAngle:F0}\u00B0";
        levelerOuterRenderer.material.color =
            upAngle <= levelerTolerance &&
            upAngle >= (levelerTolerance * -1) 
            ? Color.green 
            : levelerDefaultColor;
    }
}
