using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DialogueCamera : MonoBehaviour
{
    public Transform fTarget; 
    public Transform bTarget;
    public float positionY;
    [Range(0, 1)]
    public float compositionX;
    [Range(0.5f, 1)]
    public float interval = 1;
    public bool update;

    Camera c;
    private float ec;
    private float ae;
    private float ab;
    private float be;
    private float abc;
    private Vector3 lookCenter;
    private Vector3 ft;
    private Vector3 bt;
    private float tanHf;
    private float tanR;

    private void OnEnable()
    {
        c = GetComponent<Camera>();
        SceneView.onSceneGUIDelegate += OnSceneView;
    }

    private void Update()
    {
        if (fTarget && bTarget)
        {
            ft = fTarget.position;
            bt = Vector3.Lerp(ft, bTarget.position, interval);
            if (update)
            {
                Calculate();
            }
        }
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneView;
    }

    [ContextMenu("Calc")]
    void Calculate()
    {
        var hf = c.fieldOfView / 2;
        tanHf = Mathf.Tan(hf * Mathf.Deg2Rad);
        tanR = tanHf * c.aspect * (compositionX * 2 - 1);
        ec = positionY / tanHf;
        ae = ec * tanR;
        ab = Vector3.Distance(ft, bt);
        be = Mathf.Sqrt(ab * ab - ae * ae);
        abc = Mathf.Atan(ae / be) * Mathf.Rad2Deg;
        lookCenter = bt + Quaternion.Euler(0, abc, 0) * (ft - bt).normalized * (be + ec);

        c.transform.position = lookCenter;
        c.transform.rotation = Quaternion.LookRotation(bt - lookCenter);
    }

    private void OnSceneView(SceneView sceneView)
    {

        var cp = c.transform.position;
        var cr = c.transform.rotation;
        Handles.DrawLine(ft, bt);
        Handles.DrawLine(ft, cp);
        Handles.DrawLine(cp, bt);

        Handles.color = Color.red;
        var ep = ft - cr * Vector3.right * ae;
        var dp = ep -Vector3.up * positionY;
        Handles.DrawLine(ft, ft - Vector3.up * positionY);
        Handles.DrawLine(ep, dp);
        Handles.DrawLine(ft, ep);
        Handles.color = Color.white;

        Handles.BeginGUI();
        Button(ft, "A");
        Button(bt, "B");
        Button(cp, "C");
        Button(ep, "E");
        Button(dp, "D");

        //DisButton(ft, bt);
        //DisButton(ep, ft);
        //DisButton(ep, bt);
        //DisButton(ep, cp);
        //DisButton(ep, dp);

        //AngleButton(ft, bt, ep, "ABC:");
        //AngleButton(ft, cp, bt, "HFov/2:");
        //AngleButton(hp, cp, ep, "VFov/2:");
        Handles.EndGUI();
    }

    void Button(Vector3 position, string s)
    {
        GUI.TextField(HandleUtility.WorldPointToSizedRect(
            position, new GUIContent(s), GUI.skin.textField),
            s);
    }

    void DisButton(Vector3 p1, Vector3 p2)
    {
        var s = Vector3.Distance(p1, p2).ToString("F2");
        GUI.Button(HandleUtility.WorldPointToSizedRect(
            (p1 + p2) / 2, new GUIContent(s), GUI.skin.button),
            s);
    }

    void AngleButton(Vector3 p1, Vector3 p2, Vector3 p3, string info = "")
    {
        var v1 = p1 - p2;
        var v3 = p3 - p2;
        var s = info + Vector3.Angle(v1, v3).ToString("F2");
        GUI.TextField(HandleUtility.WorldPointToSizedRect(
            p2 + (v1 + v3) / 16, new GUIContent(s), GUI.skin.textField),
            s);
    }

}
