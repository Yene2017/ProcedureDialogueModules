using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Baka : MonoBehaviour
{
    public Quaternion rotation;
    public float cos;
    public float sin;
    public Vector3 axis;
    public float angle;


    // Update is called once per frame
    void Update()
    {
        this.rotation = this.transform.rotation;
        this.sin = Mathf.Sin(this.transform.eulerAngles.y * Mathf.Deg2Rad);
        this.cos = Mathf.Cos(this.transform.eulerAngles.y * Mathf.Deg2Rad);
        this.transform.rotation.ToAngleAxis(out angle, out axis);
    }
}
