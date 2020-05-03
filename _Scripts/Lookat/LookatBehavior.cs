using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IK.Lookat
{
    public class LookatBehavior : MonoBehaviour
    {
        [Serializable]
        public class Bone
        {
            public Transform transform;
            public Quaternion lastRotation;
            public float smoothWeight;
        }

        public LookatData lookatData;
        public Transform lookatTarget;
        public Vector3 lookatPosition;

        public Bone[] boneArray;
        public Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

        private void OnEnable()
        {
            InitBoneMap(this.transform);
            Init(this.transform, lookatData);

#if UNITY_EDITOR
            //LookatDrawer.Begin();
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            //LookatDrawer.End();
#endif
        }

        private void Update()
        {
            if (!lookatTarget) return;
            lookatPosition = lookatTarget.position;
        }

        private void LateUpdate()
        {
            if (!lookatTarget) return;
            Smooth(Calculate());
        }

        public void InitBoneMap(Transform transform)
        {
            boneMap.Clear();
            foreach (var t in transform.GetComponentsInChildren<Transform>())
            {
                boneMap.Add(t.name, t);
            }
        }

        public void Init(Transform root, LookatData lookatData)
        {
            var length = lookatData.boneConfig.Length;
            boneArray = new Bone[length];
            for (var i = 0; i < length; i++)
            {
                var data = lookatData.boneConfig[i];
                Transform transform = null;
                if (!boneMap.TryGetValue(data.name, out transform))
                {
                    continue;
                }
                var bone = new Bone();
                bone.transform = transform;
                boneArray[i] = bone;
            }
        }

        private bool Validate()
        {
            var firstData = lookatData.boneConfig[0];
            var firstBone = boneArray[0];
            var forwardDirection = firstData.forward.GetDirection(firstBone.transform);
            var lookatDirection = lookatPosition - firstBone.transform.position;
            return Vector3.Angle(lookatDirection, forwardDirection) < lookatData.returnAngle;
        }

        private int Calculate()
        {
            var boneLimit = boneArray.Length;
            if (!Validate()) return boneLimit;
#if UNITY_EDITOR
            LookatDrawer.Clear(lookatPosition, boneLimit);
#endif

            var firstData = lookatData.boneConfig[0];
            var firstBone = boneArray[0];
            var forwardDirection = firstData.forward.GetDirection(firstBone.transform);
            var lookatDirection = lookatPosition - firstBone.transform.position;
#if UNITY_EDITOR
            LookatDrawer.Push(firstBone.transform.position, 
                forwardDirection + firstBone.transform.position,
                firstData);
#endif
            var i = 0;
            for (; i < boneLimit; i++)
            {
                var data = lookatData.boneConfig[i];
                var bone = boneArray[i];
                var isFinish = false;
                if (lookatData.lerpAxis)
                {
                    LerpAxisPerBone(i, forwardDirection, lookatDirection,
                        out isFinish);
                }
                if (lookatData.clampAngle)
                {
                    ClampAnglePerBone(i, forwardDirection, lookatDirection,
                        out isFinish);
                }
                if (isFinish) break;
                if (i == boneLimit - 1) break;
                //目标传递
                IterateTarget(i, firstData, firstBone,
                    out forwardDirection, out lookatDirection);
            }
            return boneLimit;
        }

        private void IterateTarget(int i, LookatData.BoneConfig firstBoneConfig, Bone firstBone,
            out Vector3 forwardDirection, out Vector3 lookatDirection)
        {
            //每次计算结束后直接赋值,初始位置坐标对应改变
            var firstBonePosition = firstBone.transform.position;
            var length = Vector3.Distance(lookatPosition, firstBonePosition);
            var vtPosition = firstBonePosition +
                firstBoneConfig.forward.GetDirection(firstBone.transform) * length;

            var nextBone = boneArray[i + 1];
            var nextData = lookatData.boneConfig[i + 1];
            var nextBonePosition = nextBone.transform.position;
            var vtDirection = vtPosition - nextBonePosition;
            var finalDirection = lookatPosition - nextBonePosition;
            forwardDirection = nextData.forward.GetDirection(nextBone.transform);
            lookatDirection = Quaternion.FromToRotation(vtDirection, finalDirection) 
                * forwardDirection;
#if UNITY_EDITOR
            LookatDrawer.Push(nextBonePosition, vtPosition, nextData);
#endif
        }

        private void LerpAxisPerBone(int i, Vector3 forwardDirection, Vector3 lookatDirection, 
            out bool finish)
        {
            var config = lookatData.boneConfig[i];
            var bone = boneArray[i];
            var rotation = Quaternion.FromToRotation(forwardDirection, lookatDirection);
            var position = bone.transform.position;
            if (config.limitWeight > 0)
            {
                Vector3 axis;
                float angle;
                rotation.ToAngleAxis(out angle, out axis);
                Vector3 up = Vector3.up;
                var newAxis = Vector3.Lerp(axis, up, config.limitWeight);
                var newAngle = Mathf.Min(angle - Vector3.Angle(axis, newAxis), config.limitAngle);
                var lookRotation = Quaternion.AngleAxis(newAngle, newAxis);
                bone.transform.rotation = lookRotation * bone.transform.rotation;
                finish = false;
            }
            else
            {
                var lookRotation = rotation;
                bone.transform.rotation = lookRotation * bone.transform.rotation;
                finish = true;
            }
        }


        private void ClampAnglePerBone(int index, Vector3 forwardDirection, Vector3 lookatDirection, 
            out bool finish)
        {
            var config = lookatData.boneConfig[index];
            var bone = boneArray[index];
            var position = bone.transform.position;
            finish = true;
            var limitHori = config.limitHori < 90;
            var limitVert = config.limitVert < 90;
            var limitSelf = config.limitSelf != 0;
            if (!limitHori && !limitVert && !limitSelf)
            {
                var lookRotation = Quaternion.FromToRotation(forwardDirection, lookatDirection);
                bone.transform.rotation = lookRotation * bone.transform.rotation;
                return;
            }
            if (lookatDirection.y == 1)
            {
                Debug.Assert(true,"LookDirectlyUp");
            }

            var forwardHoriDirection = forwardDirection;
            forwardHoriDirection.y = 0;
            Quaternion rotation = Quaternion.identity;
            if (limitHori)
            {
                //归一化后统一y轴分量
                var xzScale = Mathf.Sqrt(1 /
                    (lookatDirection.x * lookatDirection.x + lookatDirection.z * lookatDirection.z));
                var horiDirection = new Vector3(xzScale * lookatDirection.x,
                    0, xzScale * lookatDirection.z);
                var horiAngle = Vector3.Angle(forwardHoriDirection, horiDirection);
                var horiAxis = Vector3.Cross(forwardHoriDirection, horiDirection);
                if (horiAngle > config.limitHori)
                {
                    finish = false;
                    horiAngle = config.limitHori;
                }
                rotation = Quaternion.AngleAxis(horiAngle, horiAxis);
            }
            if (config.limitVert < 90)
            {
                var vertDirection = rotation * forwardHoriDirection;
                //归一化后统一y轴分量
                var xzScale2 = Mathf.Sqrt((1 - lookatDirection.y * lookatDirection.y) /
                    (vertDirection.x * vertDirection.x + vertDirection.z * vertDirection.z));
                var vertDirection2 = new Vector3(xzScale2 * vertDirection.x,
                    lookatDirection.y, xzScale2 * vertDirection.z);
                var vertAngle = Vector3.Angle(vertDirection, vertDirection2);
                if (vertAngle > config.limitVert)
                {
                    finish = false;
                    vertAngle = config.limitVert;
                }
                rotation = Quaternion.AngleAxis(vertAngle,
                    Vector3.Cross(vertDirection, vertDirection2)) * rotation;
            }
            var boneRotation = bone.transform.rotation;
            if (!finish && config.limitSelf != 0)
            {
                //TOTEST
                var inverseForwardRotation = Quaternion.FromToRotation(forwardDirection, Vector3.forward);
                var oriSelfRotation = inverseForwardRotation * boneRotation;
                float oriSelfAngle;
                Vector3 oriSelfAxis;
                oriSelfRotation.ToAngleAxis(out oriSelfAngle, out oriSelfAxis);
                var actualDirection = rotation * forwardDirection;
                var selfRotation = Quaternion.AngleAxis(oriSelfAngle * config.limitSelf, 
                    actualDirection);
                rotation = selfRotation * rotation;
            }
            bone.transform.rotation = rotation * boneRotation;
        }

        private void Smooth(int boneLimit)
        {
            for (var i = 0; i < boneLimit; i++)
            {
                var data = lookatData.boneConfig[i];
                var bone = boneArray[i];
                var result = bone.transform.localRotation;
                var angle = (int)Quaternion.Angle(result, bone.lastRotation);
                if (angle > 0)
                {
                    bone.smoothWeight += Mathf.Min(lookatData.rotateSpeed, data.angularSpeed * Time.deltaTime / 90f);
                    result = Quaternion.Lerp(
                        bone.lastRotation, result, bone.smoothWeight);
                    bone.transform.localRotation = result;
                }
                else
                {
                    bone.smoothWeight = 0;
                }
                bone.lastRotation = result;
            }
        }

    }
}
