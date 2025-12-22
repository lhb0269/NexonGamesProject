using UnityEngine;
using NexonGame.Core;

namespace NexonGame.Utilities
{
    /// <summary>
    /// MonoBehaviour에서 서비스 의존성을 자동으로 주입하는 유틸리티
    /// </summary>
    public abstract class ServiceInjector : MonoBehaviour
    {
        protected virtual void Awake()
        {
            InjectServices();
        }

        /// <summary>
        /// 자식 클래스에서 오버라이드하여 서비스를 주입합니다
        /// </summary>
        protected abstract void InjectServices();

        /// <summary>
        /// 서비스 가져오기 헬퍼 메서드
        /// </summary>
        protected T GetService<T>() where T : class
        {
            return ServiceLocator.Instance.Get<T>();
        }
    }
}
