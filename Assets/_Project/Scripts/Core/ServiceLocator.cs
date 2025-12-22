using System;
using System.Collections.Generic;
using UnityEngine;

namespace NexonGame.Core
{
    /// <summary>
    /// 의존성 주입을 위한 서비스 로케이터 패턴 구현
    /// 싱글톤 대신 사용하여 느슨한 결합을 유지합니다
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 서비스를 등록합니다
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"서비스 {type.Name}이(가) 이미 등록되어 있습니다. 교체합니다.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"서비스 등록됨: {type.Name}");
            }
        }

        /// <summary>
        /// 서비스를 가져옵니다
        /// </summary>
        public T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogError($"서비스 {type.Name}을(를) 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 서비스가 등록되어 있는지 확인합니다
        /// </summary>
        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 서비스 등록을 해제합니다
        /// </summary>
        public void Unregister<T>() where T : class
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"서비스 등록 해제됨: {type.Name}");
            }
        }

        /// <summary>
        /// 모든 서비스를 초기화합니다
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            Debug.Log("모든 서비스가 초기화되었습니다.");
        }

        /// <summary>
        /// 테스트용: 새 인스턴스를 생성합니다
        /// </summary>
        public static void ResetInstance()
        {
            _instance = null;
        }
    }
}
