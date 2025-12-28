using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NexonGame.BlueArchive.Stage;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Reward;
using NexonGame.BlueArchive.UI;

namespace NexonGame.Tests.PlayMode
{
    /// <summary>
    /// 블루 아카이브 전체 통합 테스트
    /// - 6개 체크포인트 전체 검증
    /// - Normal 1-4 스테이지 완전 자동화
    /// - UI 진행 상황 표시
    /// </summary>
    public class BlueArchiveIntegrationTests
    {
        private GameObject _testSceneRoot;
        private StageManager _stageManager;
        private CombatManager _combatManager;
        private RewardSystem _rewardSystem;
        private TestProgressPanel _testProgressPanel;

        private StageData _testStageData;
        private List<StudentData> _testStudents;
        private List<EnemyData> _testEnemies;

        private Camera _testCamera;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log("====================================");
            Debug.Log("블루 아카이브 통합 테스트 시작");
            Debug.Log("====================================");

            // 테스트용 씬 루트 생성
            _testSceneRoot = new GameObject("TestSceneRoot");

            // 테스트용 카메라 생성
            CreateTestCamera();

            // StageManager 생성
            var stageManagerObj = new GameObject("StageManager");
            stageManagerObj.transform.SetParent(_testSceneRoot.transform);
            _stageManager = stageManagerObj.AddComponent<StageManager>();

            // GridVisualizer 추가
            var visualizerObj = new GameObject("GridVisualizer");
            visualizerObj.transform.SetParent(stageManagerObj.transform);
            visualizerObj.AddComponent<GridVisualizer>();

            // CombatManager 생성
            var combatManagerObj = new GameObject("CombatManager");
            combatManagerObj.transform.SetParent(_testSceneRoot.transform);
            _combatManager = combatManagerObj.AddComponent<CombatManager>();

            // TestProgressPanel 생성
            var testPanelObj = new GameObject("TestProgressPanel");
            testPanelObj.transform.SetParent(_testSceneRoot.transform);
            _testProgressPanel = testPanelObj.AddComponent<TestProgressPanel>();

            // RewardSystem 생성
            _rewardSystem = new RewardSystem();

            // 테스트 데이터 생성
            _testStageData = StagePresets.CreateNormal1_4();
            _testStudents = CreateTestStudents();
            _testEnemies = CreateTestEnemies();

            yield return null;

            Debug.Log("[SetUp] 테스트 환경 준비 완료");
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

        /// <summary>
        /// 테스트용 학생 데이터 생성 (StudentPresets 사용)
        /// </summary>
        private List<StudentData> CreateTestStudents()
        {
            // StudentPresets에서 정의된 학생 데이터 사용
            return StudentPresets.CreateAllStudents();
        }

        /// <summary>
        /// 테스트용 적 데이터 생성 (StudentPresets 사용)
        /// </summary>
        private List<EnemyData> CreateTestEnemies()
        {
            // StudentPresets에서 정의된 Normal 1-4 적 데이터 사용
            return StudentPresets.CreateNormal1_4Enemies();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // StudentData 정리 (StudentPresets 사용)
            if (_testStudents != null)
            {
                StudentPresets.DestroyAllStudents(_testStudents);
            }

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

            _testStudents?.Clear();
            _testEnemies?.Clear();

            yield return null;
        }

        /// <summary>
        /// 5개 체크포인트 전체 통합 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator FullIntegration_AllFiveCheckpoints_ShouldPass()
        {
            Debug.Log("=====================================");
            Debug.Log("5개 체크포인트 전체 통합 테스트 시작");
            Debug.Log("=====================================");

            _testProgressPanel.UpdateMessage("테스트 시작...");
            yield return new WaitForSeconds(1f);

            // ========================================
            // 체크포인트 #1: 플랫폼 이동 검증
            // ========================================
            yield return CheckpointOne_PlatformMovement();

            // ========================================
            // 체크포인트 #2: 전투 진입 검증
            // ========================================
            yield return CheckpointTwo_BattleEntry();

            // ========================================
            // 체크포인트 #3: EX 스킬 사용 로깅 (코스트 소모 포함)
            // ========================================
            yield return CheckpointThree_SkillUsage();

            // ========================================
            // 체크포인트 #4: 전투별 데미지 추적
            // ========================================
            yield return CheckpointFour_DamageTracking();

            // ========================================
            // 체크포인트 #5: 보상 획득 검증
            // ========================================
            yield return CheckpointFive_RewardVerification();

            // ========================================
            // 최종 결과
            // ========================================
            Debug.Log("=====================================");
            Debug.Log("✅ 모든 체크포인트 통과!");
            Debug.Log("=====================================");

            _testProgressPanel.UpdateMessage("✅ 전체 테스트 완료!");
            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// 체크포인트 #1: 플랫폼 이동 검증
        /// AAA 패턴 적용: Arrange - Act - Assert
        /// </summary>
        private IEnumerator CheckpointOne_PlatformMovement()
        {
            Debug.Log("\n[체크포인트 #1] 플랫폼 이동 검증 시작 (AAA 패턴)");
            _testProgressPanel.UpdateCheckpoint(1, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("플랫폼 이동 테스트 중...");

            // ========================================
            // Arrange: 스테이지 초기화 및 경로 준비
            // ========================================
            Debug.Log("  [Arrange] 스테이지 초기화");

            // 스테이지 초기화
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // 플랫폼 생성 확인
            var platforms = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);
            int expectedPlatformCount = 1 + _testStageData.platformPositions.Count + 1; // 시작 + 일반 + 전투
            Assert.AreEqual(expectedPlatformCount, platforms.Length,
                $"플랫폼 {expectedPlatformCount}개 생성 확인");

            Debug.Log($"    - 생성된 플랫폼: {platforms.Length}개");
            Debug.Log($"    - 시작 위치: {_stageManager.PlayerPosition}");
            Debug.Log($"    - 목표 위치: {_testStageData.battlePosition}");

            // 이동 경로 준비: (0,0) → (1,1) → (0,2) → (1,1) → (2,1) → (3,1)
            var movementPath = new List<Vector2Int>
            {
                new Vector2Int(1, 1), // (0,0)에서 (1,1)로
                new Vector2Int(0, 2), // (1,1)에서 (0,2)로
                new Vector2Int(1, 1), // (0,2)에서 (1,1)로 (되돌아옴)
                new Vector2Int(2, 1), // (1,1)에서 (2,1)로
                new Vector2Int(3, 1)  // (2,1)에서 (3,1)로 (전투)
            };

            Debug.Log($"    - 이동 경로: {movementPath.Count}칸");
            yield return new WaitForSeconds(0.5f);

            // ========================================
            // Act: 경로를 따라 플랫폼 클릭으로 플레이어 이동
            // ========================================
            Debug.Log("  [Act] 플랫폼 클릭을 통한 플레이어 이동 실행");

            int successfulMoves = 0;
            foreach (var targetPos in movementPath)
            {
                Vector2Int currentPos = _stageManager.PlayerPosition;
                Debug.Log($"    - 현재 위치: {currentPos}, 목표 플랫폼 클릭: {targetPos}");

                // Act: 플랫폼 클릭 시뮬레이션
                _stageManager.SimulatePlatformClick(targetPos);
                yield return null;

                // 이동 성공 여부 확인
                if (_stageManager.PlayerPosition == targetPos)
                {
                    successfulMoves++;
                    Debug.Log($"    - 이동 성공: {_stageManager.PlayerPosition}");
                }
                else
                {
                    Debug.LogWarning($"    - 이동 실패: 현재 위치 {_stageManager.PlayerPosition}");
                }

                yield return new WaitForSeconds(0.3f);
            }

            // ========================================
            // Assert: 이동 결과 검증
            // ========================================
            Debug.Log("  [Assert] 이동 결과 검증");

            // 성공한 이동 횟수 확인
            Assert.AreEqual(movementPath.Count, successfulMoves,
                $"모든 이동이 성공해야 함 ({movementPath.Count}회)");

            // 최종 위치가 전투 위치인지 확인
            Assert.AreEqual(_testStageData.battlePosition, _stageManager.PlayerPosition,
                "전투 위치에 도착해야 함");

            // 스테이지 상태가 전투 준비 상태인지 확인
            Assert.AreEqual(StageState.ReadyForBattle, _stageManager.CurrentState,
                "전투 준비 상태여야 함");

            // 총 이동 횟수 확인
            Assert.AreEqual(movementPath.Count, _stageManager.TotalMovesInStage,
                "총 이동 횟수가 일치해야 함");

            Debug.Log($"    ✓ 성공한 이동: {successfulMoves}/{movementPath.Count}");
            Debug.Log($"    ✓ 최종 위치: {_stageManager.PlayerPosition}");
            Debug.Log($"    ✓ 스테이지 상태: {_stageManager.CurrentState}");
            Debug.Log($"    ✓ 총 이동 횟수: {_stageManager.TotalMovesInStage}회");

            Debug.Log("[체크포인트 #1] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(1, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("플랫폼 이동 완료!");
            yield return new WaitForSeconds(0.5f);

            // 정리: 생성된 플랫폼 제거
            platforms = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);
            foreach (var platform in platforms)
            {
                Object.Destroy(platform.gameObject);
            }
            Debug.Log($"  🧹 플랫폼 {platforms.Length}개 정리 완료");
            yield return null;
        }

        /// <summary>
        /// 체크포인트 #2: 전투 진입 검증
        /// AAA 패턴 적용: Arrange - Act - Assert
        /// </summary>
        private IEnumerator CheckpointTwo_BattleEntry()
        {
            Debug.Log("\n[체크포인트 #2] 전투 진입 검증 시작 (AAA 패턴)");
            _testProgressPanel.UpdateCheckpoint(2, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("전투 진입 테스트 중...");

            // ========================================
            // Arrange: 전투 준비 (이미 체크포인트 #1에서 전투 위치 도착)
            // ========================================
            Debug.Log("  [Arrange] 전투 진입 준비");

            StageState initialStageState = _stageManager.CurrentState;
            Debug.Log($"    - 초기 스테이지 상태: {initialStageState}");
            Debug.Log($"    - 학생 데이터: {_testStudents.Count}명");
            Debug.Log($"    - 적 데이터: {_testEnemies.Count}명");

            // 전투 준비 상태여야 함 (체크포인트 #1에서 검증됨)
            Assert.AreEqual(StageState.ReadyForBattle, initialStageState,
                "전투 시작 전에는 ReadyForBattle 상태여야 함");

            // ========================================
            // Act: 전투 시작 및 초기화
            // ========================================
            Debug.Log("  [Act] 전투 시작 및 초기화 실행");

            // 스테이지에서 전투 시작
            _stageManager.StartBattle();
            yield return null;

            // 전투 매니저 초기화
            _combatManager.InitializeCombat(_testStudents, _testEnemies, "Normal 1-4");
            yield return null;

            // ========================================
            // Assert: 전투 진입 및 오브젝트 생성 검증
            // ========================================
            Debug.Log("  [Assert] 전투 진입 결과 검증");

            // 스테이지 상태가 전투 중으로 변경되었는지 확인
            Assert.AreEqual(StageState.InBattle, _stageManager.CurrentState,
                "스테이지 상태가 InBattle이어야 함");

            // 전투 매니저 상태가 진행 중인지 확인
            Assert.AreEqual(CombatState.InProgress, _combatManager.CurrentState,
                "전투 매니저 상태가 InProgress여야 함");

            Debug.Log($"    ✓ 스테이지 상태: {_stageManager.CurrentState}");
            Debug.Log($"    ✓ 전투 상태: {_combatManager.CurrentState}");

            // 학생 오브젝트 생성 검증
            var studentObjects = Object.FindObjectsByType<StudentObject>(FindObjectsSortMode.None);
            Assert.AreEqual(_testStudents.Count, studentObjects.Length,
                $"학생 오브젝트 {_testStudents.Count}명 생성되어야 함");

            // 적 오브젝트 생성 검증
            var enemyObjects = Object.FindObjectsByType<EnemyObject>(FindObjectsSortMode.None);
            Assert.AreEqual(_testEnemies.Count, enemyObjects.Length,
                $"적 오브젝트 {_testEnemies.Count}명 생성되어야 함");

            Debug.Log($"    ✓ 학생 오브젝트: {studentObjects.Length}명");
            Debug.Log($"    ✓ 적 오브젝트: {enemyObjects.Length}명");

            // UI 패널 생성 검증
            var costDisplay = Object.FindFirstObjectByType<CostDisplay>();
            var combatLogPanel = Object.FindFirstObjectByType<CombatLogPanel>();
            var combatStatusPanel = Object.FindFirstObjectByType<CombatStatusPanel>();
            var skillButtonPanel = Object.FindFirstObjectByType<SkillButtonPanel>();

            Assert.IsNotNull(costDisplay, "CostDisplay가 생성되어야 함");
            Assert.IsNotNull(combatLogPanel, "CombatLogPanel이 생성되어야 함");
            Assert.IsNotNull(combatStatusPanel, "CombatStatusPanel이 생성되어야 함");
            Assert.IsNotNull(skillButtonPanel, "SkillButtonPanel이 생성되어야 함");

            Debug.Log($"    ✓ UI 패널: CostDisplay, CombatLog, CombatStatus, SkillButton");

            // 코스트 시스템 초기화 검증
            Assert.Greater(_combatManager.MaxCost, 0, "최대 코스트가 설정되어야 함");
            Assert.GreaterOrEqual(_combatManager.CurrentCost, 0, "현재 코스트가 0 이상이어야 함");

            Debug.Log($"    ✓ 코스트 시스템: {_combatManager.CurrentCost}/{_combatManager.MaxCost}");

            Debug.Log("[체크포인트 #2] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(2, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("전투 진입 완료!");
            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// 체크포인트 #3: EX 스킬 사용 로깅 (버튼 클릭 시뮬레이션)
        /// AAA 패턴 적용: Arrange - Act - Assert
        /// </summary>
        private IEnumerator CheckpointThree_SkillUsage()
        {
            Debug.Log("\n[체크포인트 #3] EX 스킬 사용 로깅 시작 (AAA 패턴)");
            _testProgressPanel.UpdateCheckpoint(3, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("스킬 사용 테스트 중...");

            // ========================================
            // Arrange: 테스트 준비
            // ========================================
            Debug.Log("  [Arrange] 테스트 환경 준비");

            // 초기 상태 기록
            var combatLog = _combatManager.CombatSystem.CombatLog;
            int initialSkillCount = combatLog.TotalSkillsUsed;
            int initialDamage = combatLog.TotalDamageDealt;
            int initialCost = _combatManager.CurrentCost;

            Debug.Log($"    - 초기 스킬 사용 횟수: {initialSkillCount}");
            Debug.Log($"    - 초기 데미지: {initialDamage}");
            Debug.Log($"    - 초기 코스트: {initialCost}/{_combatManager.MaxCost}");

            // 코스트 충전 대기
            Debug.Log("    - 코스트 충전 대기...");
            yield return new WaitForSeconds(2f);

            // ========================================
            // Act: 모든 학생의 스킬 버튼 클릭
            // ========================================
            Debug.Log("  [Act] 학생 스킬 사용 실행");

            int skillsUsedCount = 0;
            for (int i = 0; i < _testStudents.Count; i++)
            {
                var student = _testStudents[i];

                // 코스트 충분할 때까지 대기
                while (_combatManager.CurrentCost < student.skillCost)
                {
                    yield return new WaitForSeconds(1f);
                }

                Debug.Log($"    - [{student.studentName}] 스킬 버튼 클릭 (코스트: {student.skillCost})");

                // Act: 스킬 버튼 클릭
                _combatManager.SimulateSkillButtonClick(i);
                yield return null;

                skillsUsedCount++;

                // 적이 모두 격파되면 종료
                if (_combatManager.GetAliveEnemyCount() == 0)
                {
                    Debug.Log("    - 모든 적 격파! 스킬 사용 종료");
                    break;
                }

                yield return new WaitForSeconds(0.5f);
            }

            // ========================================
            // Assert: 결과 검증
            // ========================================
            Debug.Log("  [Assert] 결과 검증");

            int finalSkillCount = combatLog.TotalSkillsUsed;
            int finalDamage = combatLog.TotalDamageDealt;
            int finalCost = _combatManager.CurrentCost;

            // 스킬이 최소 1회 이상 사용되었는지 검증
            Assert.Greater(finalSkillCount, initialSkillCount,
                "스킬이 최소 1회 이상 사용되어야 함");

            // 데미지가 발생했는지 검증
            Assert.Greater(finalDamage, initialDamage,
                "데미지가 발생했어야 함");

            // 코스트가 소모되었는지 검증 (코스트 회복 고려)
            int totalCostSpent = combatLog.TotalCostSpent;
            Assert.Greater(totalCostSpent, 0,
                "코스트가 소모되었어야 함");

            Debug.Log($"    ✓ 스킬 사용: {initialSkillCount} → {finalSkillCount} (+{finalSkillCount - initialSkillCount})");
            Debug.Log($"    ✓ 총 데미지: {initialDamage} → {finalDamage} (+{finalDamage - initialDamage})");
            Debug.Log($"    ✓ 코스트 소모: {totalCostSpent} (현재: {finalCost}/{_combatManager.MaxCost})");
            Debug.Log($"    ✓ 실제 사용한 학생 수: {skillsUsedCount}명");

            Debug.Log("[체크포인트 #3] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(3, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("스킬 사용 완료!");
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 체크포인트 #4: 전투별 데미지 추적
        /// AAA 패턴 적용: Arrange - Act - Assert
        /// </summary>
        private IEnumerator CheckpointFour_DamageTracking()
        {
            Debug.Log("\n[체크포인트 #4] 전투별 데미지 추적 시작 (AAA 패턴)");
            _testProgressPanel.UpdateCheckpoint(4, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("데미지 추적 중...");

            // ========================================
            // Arrange: 테스트 준비
            // ========================================
            Debug.Log("  [Arrange] 데미지 추적 환경 준비");

            var combatLog = _combatManager.CombatSystem.CombatLog;
            int initialDamage = combatLog.TotalDamageDealt;
            int initialEnemiesDefeated = combatLog.TotalEnemiesDefeated;
            int aliveEnemyCount = _combatManager.GetAliveEnemyCount();

            Debug.Log($"    - 현재까지 총 데미지: {initialDamage}");
            Debug.Log($"    - 격파한 적: {initialEnemiesDefeated}명");
            Debug.Log($"    - 생존 중인 적: {aliveEnemyCount}명");

            // 기존 데미지가 있어야 함 (체크포인트 #3에서 스킬 사용)
            Assert.Greater(initialDamage, 0, "이전 체크포인트에서 데미지가 기록되어 있어야 함");

            // ========================================
            // Act: 추가 스킬 사용 (남은 적이 있을 경우)
            // ========================================
            Debug.Log("  [Act] 추가 데미지 발생");

            int additionalDamage = 0;
            if (aliveEnemyCount > 0)
            {
                Debug.Log("    - 코스트 충전 대기...");
                yield return new WaitForSeconds(2f);

                int damageBefore = combatLog.TotalDamageDealt;

                Debug.Log("    - 첫 번째 학생 스킬 사용");
                _combatManager.SimulateSkillButtonClick(0);
                yield return null;

                int damageAfter = combatLog.TotalDamageDealt;
                additionalDamage = damageAfter - damageBefore;

                Debug.Log($"    - 발생한 데미지: {additionalDamage}");
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.Log("    - 모든 적이 격파되어 추가 공격 스킵");
            }

            // ========================================
            // Assert: 데미지 기록 검증
            // ========================================
            Debug.Log("  [Assert] 데미지 추적 결과 검증");

            int finalDamage = combatLog.TotalDamageDealt;
            int finalEnemiesDefeated = combatLog.TotalEnemiesDefeated;
            int finalSkillsUsed = combatLog.TotalSkillsUsed;

            // 총 데미지가 증가했거나 유지되어야 함
            Assert.GreaterOrEqual(finalDamage, initialDamage,
                "총 데미지는 감소하지 않아야 함");

            // 학생별 데미지 통계가 존재해야 함
            var studentDamageStats = combatLog.StudentDamageStats;
            Assert.Greater(studentDamageStats.Count, 0,
                "학생별 데미지 통계가 기록되어야 함");

            Debug.Log($"    ✓ 최종 총 데미지: {finalDamage} (증가분: +{finalDamage - initialDamage})");
            Debug.Log($"    ✓ 총 스킬 사용: {finalSkillsUsed}회");
            Debug.Log($"    ✓ 격파한 적: {finalEnemiesDefeated}명");
            Debug.Log($"    ✓ 학생별 데미지 통계:");

            foreach (var kvp in studentDamageStats)
            {
                Debug.Log($"      - {kvp.Key}: {kvp.Value} 데미지");
            }

            Debug.Log("[체크포인트 #4] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(4, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("데미지 추적 완료!");
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 체크포인트 #5: 보상 획득 검증 (인벤토리 + 검증 통합)
        /// AAA 패턴 적용: Arrange - Act - Assert
        /// </summary>
        private IEnumerator CheckpointFive_RewardVerification()
        {
            Debug.Log("\n[체크포인트 #5] 보상 획득 검증 시작 (AAA 패턴 + 인벤토리)");
            _testProgressPanel.UpdateCheckpoint(5, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("보상 검증 중...");

            // ========================================
            // Arrange: 전투 완료 준비
            // ========================================
            Debug.Log("  [Arrange] 보상 계산 준비");

            var combatLog = _combatManager.CombatSystem.CombatLog;
            int totalMoves = _stageManager.TotalMovesInStage;
            int totalSkillsUsed = combatLog.TotalSkillsUsed;
            int totalDamage = combatLog.TotalDamageDealt;
            int enemiesDefeated = combatLog.TotalEnemiesDefeated;

            Debug.Log($"    - 스테이지: {_testStageData.stageName}");
            Debug.Log($"    - 총 이동 횟수: {totalMoves}회");
            Debug.Log($"    - 스킬 사용: {totalSkillsUsed}회");
            Debug.Log($"    - 총 데미지: {totalDamage}");
            Debug.Log($"    - 격파한 적: {enemiesDefeated}명");

            // ========================================
            // Act: 전투 완료 및 보상 계산
            // ========================================
            Debug.Log("  [Act] 전투 완료 및 보상 계산 실행");

            // 전투 완료
            _stageManager.CompleteBattle(victory: true);
            yield return null;

            // 보상 계산
            var rewardResult = _rewardSystem.CalculateRewards(
                _testStageData.stageName,
                totalMoves,
                combatLog
            );

            // 스테이지 클리어
            _stageManager.ClearStage();
            yield return null;

            // ========================================
            // Assert: 보상 및 상태 검증 (1단계)
            // ========================================
            Debug.Log("  [Assert - 1단계] 기본 보상 검증");

            // 스테이지 상태가 클리어로 변경되었는지 확인
            Assert.AreEqual(StageState.StageCleared, _stageManager.CurrentState,
                "스테이지 상태가 StageCleared여야 함");

            Debug.Log($"    ✓ 스테이지 상태: {_stageManager.CurrentState}");

            // 보상 결과가 존재하는지 확인
            Assert.IsNotNull(rewardResult, "보상 결과가 null이 아니어야 함");

            // 보상 항목이 있는지 확인
            Assert.IsNotEmpty(rewardResult.GrantedRewards,
                "보상 항목이 최소 1개 이상 있어야 함");

            Debug.Log($"    ✓ 획득한 보상: {rewardResult.GrantedRewards.Count}개");

            // 각 보상의 수량이 유효한지 확인
            foreach (var reward in rewardResult.GrantedRewards)
            {
                Assert.Greater(reward.quantity, 0,
                    $"{reward.itemName} 보상 수량이 0보다 커야 함");
                Debug.Log($"      - {reward.itemName} x{reward.quantity}");
            }

            // === 1단계: RewardResultPanel 생성 및 표시 ===
            Debug.Log("  [1/4] RewardResultPanel 생성 중...");
            var rewardPanelObj = new GameObject("RewardResultPanel");
            var rewardPanel = rewardPanelObj.AddComponent<RewardResultPanel>();
            yield return null;

            string statistics = $"총 이동 횟수: {totalMoves}회\n" +
                              $"스킬 사용: {totalSkillsUsed}회\n" +
                              $"총 데미지: {totalDamage}\n" +
                              $"격파한 적: {enemiesDefeated}명";

            rewardPanel.ShowRewards(_testStageData.stageName, rewardResult, statistics);

            // Assert: 보상 패널이 정상적으로 생성되었는지 확인
            Assert.IsNotNull(rewardPanel, "RewardResultPanel이 생성되어야 함");
            Debug.Log("  ✅ RewardResultPanel 표시 완료");

            yield return new WaitForSeconds(1.5f);

            // === 2단계: InventoryPanel 생성 및 초기화 ===
            Debug.Log("  [2/4] InventoryPanel 생성 중...");
            var inventoryPanelObj = new GameObject("InventoryPanel");
            var inventoryPanel = inventoryPanelObj.AddComponent<InventoryPanel>();
            inventoryPanel.Initialize(_rewardSystem);

            // Assert: 인벤토리 패널 생성 확인
            Assert.IsNotNull(inventoryPanel, "InventoryPanel이 생성되어야 함");
            Debug.Log("  ✅ InventoryPanel 생성 완료");

            yield return new WaitForSeconds(0.5f);

            // === 3단계: 보상을 하나씩 인벤토리에 추가 (애니메이션 포함) ===
            Debug.Log("  [3/4] 보상을 인벤토리에 추가 중...");
            _testProgressPanel.UpdateMessage("보상을 인벤토리에 추가 중...");

            int rewardsGranted = 0;
            foreach (var reward in rewardResult.GrantedRewards)
            {
                Debug.Log($"    인벤토리에 추가: {reward.itemName} x{reward.quantity}");
                _rewardSystem.GrantReward(reward); // 이벤트 발생 → InventoryPanel 업데이트
                rewardsGranted++;
                yield return new WaitForSeconds(0.4f);
            }

            // Assert: 모든 보상이 추가되었는지 확인
            Assert.AreEqual(rewardResult.GrantedRewards.Count, rewardsGranted,
                "모든 보상이 인벤토리에 추가되어야 함");
            Debug.Log($"  ✅ 모든 보상 인벤토리 추가 완료 ({rewardsGranted}개)");

            yield return new WaitForSeconds(1f);

            // === 4단계: 검증 수행 및 ValidationResultPanel 표시 ===
            Debug.Log("  [4/4] 보상 검증 수행 중...");
            _testProgressPanel.UpdateMessage("보상 검증 중...");

            var rewardValidator = new RewardValidator(_rewardSystem);
            var validationResult = rewardValidator.ValidateRewardGrant(_testStageData, rewardResult);

            // Assert: 검증 결과 확인
            Assert.IsNotNull(validationResult, "검증 결과가 null이 아니어야 함");
            Assert.IsTrue(validationResult.IsValid,
                $"검증이 성공해야 함 - 실패 이유: {validationResult.FailureReason}");

            Debug.Log($"  검증 결과: {(validationResult.IsValid ? "성공" : "실패")}");
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"  검증 실패 이유: {validationResult.FailureReason}");
                foreach (var error in validationResult.ValidationErrors)
                {
                    Debug.LogWarning($"    - {error}");
                }
            }

            // ValidationResultPanel 생성
            var validationPanelObj = new GameObject("ValidationResultPanel");
            var validationPanel = validationPanelObj.AddComponent<ValidationResultPanel>();

            var inventoryData = inventoryPanel.GetInventoryData();
            Assert.IsNotNull(inventoryData, "인벤토리 데이터가 null이 아니어야 함");

            validationPanel.ShowValidationResult(
                validationResult,
                rewardResult,
                inventoryData
            );

            // Assert: 검증 패널 생성 확인
            Assert.IsNotNull(validationPanel, "ValidationResultPanel이 생성되어야 함");
            Debug.Log("  ✅ ValidationResultPanel 표시 완료");

            yield return new WaitForSeconds(3f);

            // ========================================
            // Assert: 최종 검증 (2단계)
            // ========================================
            Debug.Log("  [Assert - 2단계] 최종 검증");

            // 인벤토리 데이터 존재 확인
            Assert.Greater(inventoryData.Count, 0, "인벤토리에 아이템이 있어야 함");

            // 모든 보상이 인벤토리에 정확히 추가되었는지 확인
            foreach (var reward in rewardResult.GrantedRewards)
            {
                Assert.IsTrue(inventoryData.ContainsKey(reward.itemType),
                    $"인벤토리에 {reward.itemType} 타입이 있어야 함");

                int inventoryQuantity = inventoryData[reward.itemType];
                Assert.AreEqual(reward.quantity, inventoryQuantity,
                    $"{reward.itemName}: 예상 {reward.quantity}, 실제 {inventoryQuantity}");

                Debug.Log($"    ✓ {reward.itemName}: {inventoryQuantity}/{reward.quantity} (일치)");
            }

            Debug.Log("[체크포인트 #5] ✅ 통과 - 보상, 인벤토리, 검증 모두 완료");

            _testProgressPanel.UpdateCheckpoint(5, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("보상 획득 및 검증 완료!");
            yield return new WaitForSeconds(1f);
            yield return null;
        }

        /// <summary>
        /// 인접하지 않은 플랫폼 클릭 시 이동 실패 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator PlatformClick_NonAdjacent_ShouldFail()
        {
            Debug.Log("\n[단위 테스트] 인접하지 않은 플랫폼 클릭 테스트 시작");

            // ========================================
            // Arrange: 스테이지 초기화
            // ========================================
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            Vector2Int initialPosition = _stageManager.PlayerPosition; // (0, 0)
            Vector2Int nonAdjacentPosition = new Vector2Int(3, 1); // 3칸 떨어진 위치

            Debug.Log($"  [Arrange] 초기 위치: {initialPosition}, 비인접 목표: {nonAdjacentPosition}");

            // ========================================
            // Act: 인접하지 않은 플랫폼 클릭
            // ========================================
            Debug.Log("  [Act] 비인접 플랫폼 클릭 시도");
            _stageManager.SimulatePlatformClick(nonAdjacentPosition);
            yield return null;

            // ========================================
            // Assert: 이동하지 않아야 함
            // ========================================
            Debug.Log("  [Assert] 이동 실패 검증");

            Assert.AreEqual(initialPosition, _stageManager.PlayerPosition,
                "인접하지 않은 플랫폼 클릭 시 이동하지 않아야 함");

            Assert.AreEqual(0, _stageManager.TotalMovesInStage,
                "이동 횟수가 증가하지 않아야 함");

            Debug.Log($"    ✓ 위치 유지: {_stageManager.PlayerPosition}");
            Debug.Log($"    ✓ 이동 횟수: {_stageManager.TotalMovesInStage}");
            Debug.Log("[단위 테스트] ✅ 통과 - 비인접 플랫폼 이동 실패 확인");

            // 정리
            var platforms = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);
            foreach (var platform in platforms)
            {
                Object.Destroy(platform.gameObject);
            }
            yield return null;
        }

        /// <summary>
        /// 인접한 플랫폼 클릭 시 이동 성공 테스트 (8방향)
        /// </summary>
        [UnityTest]
        public IEnumerator PlatformClick_Adjacent8Directions_ShouldSucceed()
        {
            Debug.Log("\n[단위 테스트] 8방향 인접 플랫폼 클릭 테스트 시작");

            // ========================================
            // Arrange: 스테이지 초기화
            // ========================================
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // 시작 위치: (0, 0)
            // 인접 8방향: N(0,1), S(0,-1), E(1,0), W(-1,0), NE(1,1), NW(-1,1), SE(1,-1), SW(-1,-1)
            // 실제 존재하는 플랫폼: (1, 1) - NE 방향
            Vector2Int startPos = new Vector2Int(0, 0);
            Vector2Int adjacentPos = new Vector2Int(1, 1);

            Debug.Log($"  [Arrange] 시작 위치: {startPos}, 인접 플랫폼 (NE): {adjacentPos}");

            // ========================================
            // Act: 인접한 플랫폼 클릭
            // ========================================
            Debug.Log("  [Act] 인접 플랫폼 클릭 (대각선 NE 방향)");
            _stageManager.SimulatePlatformClick(adjacentPos);
            yield return null;

            // ========================================
            // Assert: 이동 성공 확인
            // ========================================
            Debug.Log("  [Assert] 이동 성공 검증");

            Assert.AreEqual(adjacentPos, _stageManager.PlayerPosition,
                "인접한 플랫폼 클릭 시 이동해야 함");

            Assert.AreEqual(1, _stageManager.TotalMovesInStage,
                "이동 횟수가 1 증가해야 함");

            Debug.Log($"    ✓ 최종 위치: {_stageManager.PlayerPosition}");
            Debug.Log($"    ✓ 이동 횟수: {_stageManager.TotalMovesInStage}");
            Debug.Log("[단위 테스트] ✅ 통과 - 인접 플랫폼 이동 성공 확인");

            // 정리
            var platforms = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);
            foreach (var platform in platforms)
            {
                Object.Destroy(platform.gameObject);
            }
            yield return null;
        }

        /// <summary>
        /// 동일한 플랫폼 클릭 시 이동 실패 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator PlatformClick_SamePosition_ShouldFail()
        {
            Debug.Log("\n[단위 테스트] 동일 위치 플랫폼 클릭 테스트 시작");

            // ========================================
            // Arrange: 스테이지 초기화
            // ========================================
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            Vector2Int currentPosition = _stageManager.PlayerPosition; // (0, 0)

            Debug.Log($"  [Arrange] 현재 위치: {currentPosition}");

            // ========================================
            // Act: 동일한 위치의 플랫폼 클릭
            // ========================================
            Debug.Log("  [Act] 동일 위치 플랫폼 클릭 시도");
            _stageManager.SimulatePlatformClick(currentPosition);
            yield return null;

            // ========================================
            // Assert: 이동하지 않아야 함
            // ========================================
            Debug.Log("  [Assert] 이동 실패 검증");

            Assert.AreEqual(currentPosition, _stageManager.PlayerPosition,
                "동일 위치 클릭 시 이동하지 않아야 함");

            Assert.AreEqual(0, _stageManager.TotalMovesInStage,
                "이동 횟수가 증가하지 않아야 함");

            Debug.Log($"    ✓ 위치 유지: {_stageManager.PlayerPosition}");
            Debug.Log($"    ✓ 이동 횟수: {_stageManager.TotalMovesInStage}");
            Debug.Log("[단위 테스트] ✅ 통과 - 동일 위치 이동 실패 확인");

            // 정리
            var platforms = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);
            foreach (var platform in platforms)
            {
                Object.Destroy(platform.gameObject);
            }
            yield return null;
        }
    }
}
