using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleLookatBehavior : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        if (target)
        {
            this.transform.LookAt(target);
        }
    }
}
