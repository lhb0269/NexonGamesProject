using UnityEngine;

namespace NexonGame.Utilities
{
    /// <summary>
    /// ScriptableObject 기반 싱글톤 (설정 데이터 등에 사용)
    /// 런타임 서비스가 아닌 설정 데이터용으로만 사용하세요
    /// </summary>
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] assets = Resources.LoadAll<T>("");
                    if (assets == null || assets.Length == 0)
                    {
                        Debug.LogError($"{typeof(T).Name} 에셋을 Resources 폴더에서 찾을 수 없습니다.");
                        return null;
                    }
                    else if (assets.Length > 1)
                    {
                        Debug.LogWarning($"여러 개의 {typeof(T).Name} 에셋이 발견되었습니다. 첫 번째 에셋을 사용합니다.");
                    }
                    _instance = assets[0];
                }
                return _instance;
            }
        }
    }
}
