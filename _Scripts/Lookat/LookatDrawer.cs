using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IK.Lookat
{
    public static class LookatDrawer
    {
        public class RotateInfo
        {
            internal Vector3 position;
            internal Vector3 vtPosition;
            internal Vector3 forward;
            internal Vector3 horiDirection;
            internal LookatData.BoneConfig data;
        }
        static List<RotateInfo> info = new List<RotateInfo>();
        static float sum = 8;
        static Vector3 target;

        public static void Begin()
        {
            SceneView.onSceneGUIDelegate -= Draw;
            SceneView.onSceneGUIDelegate += Draw;
        }

        private static void Draw(SceneView sceneView)
        {
            Handles.SphereHandleCap(0, target, Quaternion.identity, 
                HandleUtility.GetHandleSize(target) / 8, EventType.Repaint);
            var count = 0;
            foreach (var i in info)
            {
                var color1 = Color.HSVToRGB(count / sum, 0.8f, 0.9f);
                var color2 = Color.HSVToRGB(count / sum, 0.6f, 0.3f);
                var position = i.position;
                var lookat = target - position;
                var actual = lookat;
                if (count < info.Count - 1)
                {
                    actual = info[count + 1].vtPosition - i.position;
                }
                var length = lookat.magnitude;
                var forward = i.forward;
                var rotation = Quaternion.FromToRotation(Vector3.forward, forward);

                Handles.color = color1;
                Handles.DrawWireArc(position,
                    Vector3.Cross(forward, actual), forward,
                    Vector3.Angle(forward, actual), length);
                Handles.color = color2;
                Handles.DrawLine(position, position + i.horiDirection * length);
                Handles.DrawLine(position, position + forward);
                Handles.DrawLine(position, target);
                if (i.data != null)
                {
                    var vertLmt = Mathf.Sin(i.data.limitVert * Mathf.Deg2Rad);
                    var horiLmt = Mathf.Sin(i.data.limitHori * Mathf.Deg2Rad);
                    var l = forward.magnitude / (count + 1);
                    var p1 = position + rotation * new Vector3(-horiLmt, vertLmt, 1) * l;
                    var p2 = position + rotation * new Vector3(horiLmt, vertLmt, 1) * l;
                    var p3 = position + rotation * new Vector3(horiLmt, -vertLmt, 1) * l;
                    var p4 = position + rotation * new Vector3(-horiLmt, -vertLmt, 1) * l;
                    Handles.DrawLine(p1, p2);
                    Handles.DrawLine(p2, p3);
                    Handles.DrawLine(p3, p4);
                    Handles.DrawLine(p4, p1);
                }
                count++;
            }
            Handles.color = Color.white;
        }

        public static void End()
        {
            SceneView.onSceneGUIDelegate -= Draw;
        }

        public static void Clear(Vector3 position, int i = 8)
        {
            target = position;
            sum = i;
            info.Clear();
        }

        public static void Push(Vector3 position, Vector3 vtPosition, LookatData.BoneConfig data)
        {
            var forward = vtPosition - position;
            var ri = new RotateInfo()
            {
                position = position,
                vtPosition = vtPosition,
                forward = forward,
                data = data,
            };
            info.Add(ri);
        }

        public static RotateInfo Modify(int index)
        {
            return info.Count > index ? info[index] : default(RotateInfo);
        }

    }
}
