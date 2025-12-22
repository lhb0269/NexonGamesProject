using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NexonGame.BlueArchive.Stage;
using NexonGame.BlueArchive.Data;

namespace NexonGame.Tests.PlayMode
{
    /// <summary>
    /// 스테이지 시스템 PlayMode 테스트
    /// - GameObject 생성 및 비주얼 검증
    /// - 플랫폼 이동 테스트
    /// </summary>
    public class StagePlayModeTests
    {
        private GameObject _testSceneRoot;
        private StageManager _stageManager;
        private StageData _testStageData;
        private Camera _testCamera;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // 테스트용 씬 루트 생성
            _testSceneRoot = new GameObject("TestSceneRoot");

            // 테스트용 카메라 생성 (시각화용)
            CreateTestCamera();

            // StageManager 생성
            var managerObj = new GameObject("StageManager");
            managerObj.transform.SetParent(_testSceneRoot.transform);
            _stageManager = managerObj.AddComponent<StageManager>();

            // GridVisualizer 추가
            var visualizerObj = new GameObject("GridVisualizer");
            visualizerObj.transform.SetParent(managerObj.transform);
            visualizerObj.AddComponent<GridVisualizer>();

            // 테스트 데이터 생성
            _testStageData = StagePresets.CreateNormal1_4();

            yield return null;
        }

        /// <summary>
        /// 테스트용 카메라 생성
        /// </summary>
        private void CreateTestCamera()
        {
            var cameraObj = new GameObject("TestCamera");
            cameraObj.transform.SetParent(_testSceneRoot.transform);

            _testCamera = cameraObj.AddComponent<Camera>();
            _testCamera.transform.position = new Vector3(0, 10, -5);
            _testCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
            _testCamera.orthographic = true;
            _testCamera.orthographicSize = 8;
            _testCamera.clearFlags = CameraClearFlags.SolidColor;
            _testCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            // 조명 추가
            var lightObj = new GameObject("TestLight");
            lightObj.transform.SetParent(_testSceneRoot.transform);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.intensity = 1f;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // 테스트 데이터 정리
            if (_testStageData != null)
            {
                StagePresets.DestroyStageData(_testStageData);
            }

            // 테스트 오브젝트 제거
            if (_testSceneRoot != null)
            {
                Object.Destroy(_testSceneRoot);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator StageManager_InitializeStage_ShouldCreatePlatforms()
        {
            Debug.Log("=== 테스트 시작: 플랫폼 생성 검증 ===");

            // Act
            _stageManager.InitializeStage(_testStageData);
            yield return new WaitForSeconds(0.5f); // 시각화 대기

            // Assert - 이름으로 플랫폼 찾기
            var allObjects = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);

            // 시작 위치 1개 + 일반 플랫폼 6개 + 전투 위치 1개 = 8개
            int expectedPlatformCount = 1 + _testStageData.platformPositions.Count + 1;
            Assert.AreEqual(expectedPlatformCount, allObjects.Length, $"플랫폼 개수가 {expectedPlatformCount}개여야 함");

            // 플랫폼 오브젝트 검증
            foreach (var platform in allObjects)
            {
                Assert.IsNotNull(platform, "PlatformObject 컴포넌트가 있어야 함");
            }

            Debug.Log($"✅ [PlayMode Test] {allObjects.Length}개의 플랫폼이 생성됨");
            yield return new WaitForSeconds(0.5f); // 결과 확인 대기
        }

        [UnityTest]
        public IEnumerator StageManager_InitializeStage_ShouldSetCorrectState()
        {
            // Act
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // Assert
            Assert.AreEqual(StageState.MovingToBattle, _stageManager.CurrentState, "초기 상태는 MovingToBattle");
            Assert.AreEqual(_testStageData.startPosition, _stageManager.PlayerPosition, "시작 위치 확인");

            Debug.Log($"✅ [PlayMode Test] 스테이지 상태: {_stageManager.CurrentState}");
        }

        [UnityTest]
        public IEnumerator StageManager_MovePlayer_ShouldUpdatePosition()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            Vector2Int startPos = _stageManager.PlayerPosition;
            Vector2Int targetPos = _testStageData.platformPositions[0]; // 첫 번째 플랫폼

            // Act
            bool moved = _stageManager.MovePlayer(targetPos);
            yield return new WaitForSeconds(0.1f); // 비주얼 업데이트 대기

            // Assert
            Assert.IsTrue(moved, "이동 성공해야 함");
            Assert.AreEqual(targetPos, _stageManager.PlayerPosition, "플레이어 위치 업데이트 확인");
            Assert.AreNotEqual(startPos, _stageManager.PlayerPosition, "위치가 변경되어야 함");

            Debug.Log($"✅ [PlayMode Test] 플레이어 이동: {startPos} → {targetPos}");
        }

        [UnityTest]
        public IEnumerator StageManager_GetPathToBattle_ShouldReturnValidPath()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // Act
            var path = _stageManager.GetPathToBattle();

            // Assert
            Assert.IsNotNull(path, "경로가 null이 아니어야 함");
            Assert.IsNotEmpty(path, "경로가 비어있지 않아야 함");
            Assert.AreEqual(_testStageData.battlePosition, path[path.Count - 1], "경로의 마지막은 전투 위치");

            Debug.Log($"✅ [PlayMode Test] 전투까지 경로: {path.Count}칸");
        }

        [UnityTest]
        public IEnumerator StageManager_MovePlayerToBattle_ShouldReachBattlePosition()
        {
            Debug.Log("=== 테스트 시작: 전투 위치까지 이동 ===");

            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return new WaitForSeconds(1f); // 초기화 확인

            var path = _stageManager.GetPathToBattle();
            Debug.Log($"경로: {path.Count}칸, 목표: {_testStageData.battlePosition}");

            // Act - 경로를 따라 이동 (천천히)
            foreach (var pos in path)
            {
                Debug.Log($"이동 중: {_stageManager.PlayerPosition} → {pos}");
                bool moved = _stageManager.MovePlayer(pos);
                Assert.IsTrue(moved, $"위치 {pos}로 이동 실패");
                yield return new WaitForSeconds(0.3f); // 이동 애니메이션 관찰
            }

            // Assert
            Assert.AreEqual(_testStageData.battlePosition, _stageManager.PlayerPosition, "전투 위치 도착 확인");
            Assert.AreEqual(StageState.ReadyForBattle, _stageManager.CurrentState, "전투 준비 상태 확인");
            Assert.IsTrue(_stageManager.CanEnterBattle(), "전투 진입 가능해야 함");

            Debug.Log($"✅ [PlayMode Test] 전투 위치 도착! 총 {_stageManager.TotalMovesInStage}회 이동");
            yield return new WaitForSeconds(1f); // 결과 확인
        }

        [UnityTest]
        public IEnumerator StageManager_StartBattle_ShouldChangeState()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // 전투 위치까지 이동
            var path = _stageManager.GetPathToBattle();
            foreach (var pos in path)
            {
                _stageManager.MovePlayer(pos);
                yield return null;
            }

            // Act
            _stageManager.StartBattle();
            yield return null;

            // Assert
            Assert.AreEqual(StageState.InBattle, _stageManager.CurrentState, "전투 중 상태 확인");

            Debug.Log("✅ [PlayMode Test] 전투 시작!");
        }

        [UnityTest]
        public IEnumerator StageManager_CompleteBattle_Victory_ShouldUpdateState()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            var path = _stageManager.GetPathToBattle();
            foreach (var pos in path)
            {
                _stageManager.MovePlayer(pos);
                yield return null;
            }

            _stageManager.StartBattle();
            yield return null;

            // Act
            _stageManager.CompleteBattle(victory: true);
            yield return null;

            // Assert
            Assert.AreEqual(StageState.BattleComplete, _stageManager.CurrentState, "전투 완료 상태");

            Debug.Log("✅ [PlayMode Test] 전투 승리!");
        }

        [UnityTest]
        public IEnumerator StageManager_ClearStage_ShouldCompleteStage()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            var path = _stageManager.GetPathToBattle();
            foreach (var pos in path)
            {
                _stageManager.MovePlayer(pos);
                yield return null;
            }

            _stageManager.StartBattle();
            yield return null;

            _stageManager.CompleteBattle(victory: true);
            yield return null;

            // Act
            _stageManager.ClearStage();
            yield return null;

            // Assert
            Assert.AreEqual(StageState.StageCleared, _stageManager.CurrentState, "스테이지 클리어 상태");

            Debug.Log("✅ [PlayMode Test] 스테이지 클리어!");
        }

        [UnityTest]
        public IEnumerator StageManager_PlayerMarker_ShouldExist()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // Act
            var playerMarker = GameObject.Find("PlayerMarker");

            // Assert
            Assert.IsNotNull(playerMarker, "플레이어 마커가 존재해야 함");

            Debug.Log("✅ [PlayMode Test] 플레이어 마커 존재 확인");
        }

        [UnityTest]
        public IEnumerator StageManager_GridVisualizer_ShouldCreateGridLines()
        {
            // Arrange
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // Act
            var visualizer = _stageManager.GetComponentInChildren<GridVisualizer>();

            // Assert - GridVisualizer 존재 확인
            Assert.IsNotNull(visualizer, "GridVisualizer가 있어야 함");

            Debug.Log("✅ [PlayMode Test] GridVisualizer 존재 확인");
        }
    }
}
