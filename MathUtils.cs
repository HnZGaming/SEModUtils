using System;
using VRage.Library.Utils;
using VRageMath;

namespace HNZ.Utils
{
    public static class MathUtils
    {
        public static float PositiveMod(float a, float b)
        {
            return (a % b + b) % b;
        }

        // https://answers.unity.com/questions/24756
        public static double SmoothDamp(double current, double target, ref double currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Math.Max(0.0001f, smoothTime);
            var num = 2f / smoothTime;
            var num2 = num * deltaTime;
            var num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            var num4 = current - target;
            var num5 = target;
            var num6 = maxSpeed * smoothTime;
            num4 = MathHelper.Clamp(num4, -num6, num6);
            target = current - num4;
            var num7 = (currentVelocity + num * num4) * deltaTime;
            currentVelocity = (currentVelocity - num * num7) * num3;
            var num8 = target + (num4 + num7) * num3;
            if (num5 - current > 0f == num8 > num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }

            return num8;
        }

        public static Vector3D SmoothDamp(Vector3D current, Vector3D target, ref Vector3D currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            return new Vector3D(
                SmoothDamp(current.X, target.X, ref currentVelocity.X, smoothTime, maxSpeed, deltaTime),
                SmoothDamp(current.Y, target.Y, ref currentVelocity.Y, smoothTime, maxSpeed, deltaTime),
                SmoothDamp(current.Z, target.Z, ref currentVelocity.Z, smoothTime, maxSpeed, deltaTime));
        }

        public static Vector3D GetRandomUnitDirection()
        {
            var dir = new Vector3D(
                MyRandom.Instance.GetRandomFloat(-1f, 1f),
                MyRandom.Instance.GetRandomFloat(-1f, 1f),
                MyRandom.Instance.GetRandomFloat(-1f, 1f));
            dir.Normalize();

            return dir;
        }
    }
}