using UnityEngine;
using System.Reflection;

namespace NexonGame.Tests.Automation
{
    /// <summary>
    /// 테스트 부트스트랩 - TestVisualizationRunner를 런타임에 생성
    /// 직렬화 문제를 피하기 위해 씬에는 빈 스크립트만 배치
    /// </summary>
    public class TestBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("[TestBootstrap] 시작");

            // TestVisualizationRunner를 동적으로 생성
            var testRunnerObj = new GameObject("TestVisualizationRunner");
            var testRunner = testRunnerObj.AddComponent<TestVisualizationRunner>();

            // Reflection을 사용해서 _autoStart를 true로 설정
            var autoStartField = typeof(TestVisualizationRunner).GetField("_autoStart",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (autoStartField != null)
            {
                autoStartField.SetValue(testRunner, true);
                Debug.Log("[TestBootstrap] _autoStart를 true로 설정");
            }

            // 이 부트스트랩은 더 이상 필요 없으므로 제거
            Destroy(gameObject);

            Debug.Log("[TestBootstrap] TestVisualizationRunner 생성 완료");
        }
    }
}
