using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IK.Lookat
{
    [CreateAssetMenu]
    public class LookatData : ScriptableObject
    {
        [Serializable]
        public class BoneConfig
        {
            public string name;
            public Axis forward;
            [Range(1,90)]
            public int angularSpeed = 1;

            [Header("Lerp Axis")]
            [Range(0,90)]
            public int limitAngle;
            [Range(0,1)]
            public float limitWeight;
            [Header("Clamp Angle")]
            [Range(0,90)]
            public int limitHori;
            [Range(0,90)]
            public int limitVert;
            [Range(-1,1)]
            public float limitSelf;
        }

        public bool lerpAxis;
        public bool clampAngle;
        [Range(0, 90)]
        public int returnAngle = 60;
        [Range(0, 1)]
        public float rotateSpeed = 0.1f;
        public BoneConfig[] boneConfig = new BoneConfig[0];
    }
}
