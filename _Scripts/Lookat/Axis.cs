using UnityEngine;

namespace IK.Lookat
{
    public enum Axis
    {
        Up,
        Down,
        Right,
        Left,
        Forward,
        Backward,
    }

    public static class AxisExtension
    {
        public static Vector3 GetDirection(this Axis axis, Transform transform)
        {
            switch(axis)
            {
                case Axis.Forward:
                    return transform.forward;
                case Axis.Backward:
                    return -transform.forward;
                case Axis.Up:
                    return transform.up;
                case Axis.Down:
                    return -transform.up;
                case Axis.Right:
                    return transform.right;
                case Axis.Left:
                    return -transform.right;
                default:
                    return transform.forward;
            }
        }
    }
}
