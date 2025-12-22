using UnityEngine;
using System.Collections.Generic;

namespace NexonGame.Utilities
{
    /// <summary>
    /// 유용한 확장 메서드 모음
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Transform의 모든 자식을 제거합니다
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Transform의 모든 자식을 즉시 제거합니다 (에디터 전용)
        /// </summary>
        public static void DestroyAllChildrenImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Vector3의 특정 축만 변경합니다
        /// </summary>
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        /// <summary>
        /// Vector2의 특정 축만 변경합니다
        /// </summary>
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }

        /// <summary>
        /// List에서 랜덤 요소를 가져옵니다
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Array에서 랜덤 요소를 가져옵니다
        /// </summary>
        public static T GetRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return default(T);
            }
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// List를 섞습니다 (Fisher-Yates 알고리즘)
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// GameObject의 레이어를 자식까지 포함하여 설정합니다
        /// </summary>
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
    }
}
