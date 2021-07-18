using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtensionUtils
{
    public static class ExtensionUtils 
    {
        private static readonly AngleSmallCheck smallCheck = AngleSmallCheck.CreateFromAngle(0.005f);
        public static void ForEach<T>(this IEnumerable<T> items, System.Action<T> action)
        {
            foreach(T item in items)
                action(item);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> items) =>
            !items.Any();

        public static float Sqr(this float value) => value * value;
        public static float Cos(this float val) => Mathf.Cos(val);
        public static float Acos(this float val) => Mathf.Acos(val);
        public static float Sin(this float val) => Mathf.Sin(val);
        public static float Dot(this Vector3 v1, Vector3 v2) => Vector3.Dot(v1, v2);
        public static float Atan2(this Vector2 val) => Mathf.Atan2(val.y, val.x);
        public static float Saturate(this float val) => Mathf.Clamp01(val);

        public static bool CloseEnough(this Vector3 p1, Vector3 p2, float epsilon = 0.005f) => (p1 - p2).sqrMagnitude < epsilon.Sqr();
        public static bool AngleSmallEnough(this Vector3 v1, Vector3 v2) => smallCheck.AngleSmallEnough(v1, v2);

        public static float PercentInRange(this float from, float toLimit1, float toLimit2) => toLimit1 + ((toLimit2 - toLimit1) * Mathf.Clamp(from, 0, 1));
        public static float Remap(this float from, float fromLimit1, float fromLimit2, float toLimit1, float toLimit2) =>
            PercentInRange(
                (Mathf.Clamp(from, fromLimit1, fromLimit2) - fromLimit1)/(fromLimit2 - fromLimit1), 
                toLimit1, toLimit2
            );

        public static T RandomElement<T>(this T[] val) => val[UnityEngine.Random.Range(0, val.Length)];
        public static T RandomElement<T>(this IList<T> val) => val[UnityEngine.Random.Range(0, val.Count)];
        // public static T RandomElement<T, L>(this L list) where L : struct, IList<T> => list[UnityEngine.Random.Range(0, list.Count)];
    }
    
    public struct AngleSmallCheck
    {
        public float cosLimit;
        public static AngleSmallCheck CreateFromAngle(float angle) => new AngleSmallCheck{cosLimit = angle.Cos()};
        public bool AngleSmallEnough(Vector3 v1, Vector3 v2) => v1.Dot(v2) / Mathf.Sqrt(v1.sqrMagnitude * v2.sqrMagnitude) > cosLimit;
        public bool AngleSmallEnoughNormalized(Vector3 v1Normalized, Vector3 v2Normalized) => v1Normalized.Dot(v2Normalized) > cosLimit;
    }

    // 1100, 0000, 0100
    enum TriSign : sbyte {Negative = -1, Zero = 0, Positive = 1}


    public static class MaterialsInstanced {
        public static Dictionary<AlternateMat, Material> instancedMatsDict = new Dictionary<AlternateMat, Material>();

        public static Material GetInstancedMat(AlternateMat altMat){
            if(instancedMatsDict.ContainsKey(altMat))
                return instancedMatsDict[altMat];

            Material mat = new Material(altMat.mat);
            mat.SetColor("_BaseColor", altMat.color);
            instancedMatsDict.Add(altMat, mat);
            return mat;
        }
    }
    
    public struct AlternateMat {
        public Material mat;
        public Color color;
        public AlternateMat(Material _mat, Color _color)
        {
            mat = _mat;
            color = _color;
        }
    }
}
