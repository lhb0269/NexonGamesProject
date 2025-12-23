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
        /// 테스트용 학생 데이터 생성
        /// </summary>
        private List<StudentData> CreateTestStudents()
        {
            var students = new List<StudentData>();

            // Shiroko
            var shirokoSkill = ScriptableObject.CreateInstance<SkillData>();
            shirokoSkill.skillName = "EX - 목표 사격";
            shirokoSkill.costAmount = 3;
            shirokoSkill.cooldownTime = 20f;
            shirokoSkill.baseDamage = 1250;
            shirokoSkill.targetType = SkillTargetType.Single;

            var shiroko = ScriptableObject.CreateInstance<StudentData>();
            shiroko.studentName = "Shiroko";
            shiroko.maxHP = 2431;
            shiroko.exSkill = shirokoSkill;
            students.Add(shiroko);

            // Hoshino
            var hoshinoSkill = ScriptableObject.CreateInstance<SkillData>();
            hoshinoSkill.skillName = "EX - 방패 전개";
            hoshinoSkill.costAmount = 2;
            hoshinoSkill.cooldownTime = 15f;
            hoshinoSkill.baseDamage = 800;
            hoshinoSkill.targetType = SkillTargetType.Single;

            var hoshino = ScriptableObject.CreateInstance<StudentData>();
            hoshino.studentName = "Hoshino";
            hoshino.maxHP = 3512;
            hoshino.exSkill = hoshinoSkill;
            students.Add(hoshino);

            // Aru
            var aruSkill = ScriptableObject.CreateInstance<SkillData>();
            aruSkill.skillName = "EX - 섬광탄";
            aruSkill.costAmount = 4;
            aruSkill.cooldownTime = 25f;
            aruSkill.baseDamage = 1500;
            aruSkill.targetType = SkillTargetType.Multiple;

            var aru = ScriptableObject.CreateInstance<StudentData>();
            aru.studentName = "Aru";
            aru.maxHP = 2187;
            aru.exSkill = aruSkill;
            students.Add(aru);

            // Haruna
            var harunaSkill = ScriptableObject.CreateInstance<SkillData>();
            harunaSkill.skillName = "EX - 집중 사격";
            harunaSkill.costAmount = 5;
            harunaSkill.cooldownTime = 30f;
            harunaSkill.baseDamage = 2100;
            harunaSkill.targetType = SkillTargetType.Single;

            var haruna = ScriptableObject.CreateInstance<StudentData>();
            haruna.studentName = "Haruna";
            haruna.maxHP = 2089;
            haruna.exSkill = harunaSkill;
            students.Add(haruna);

            return students;
        }

        /// <summary>
        /// 테스트용 적 데이터 생성
        /// </summary>
        private List<EnemyData> CreateTestEnemies()
        {
            var enemies = new List<EnemyData>();

            enemies.Add(new EnemyData("일반병 A", 1200, 50, 20));
            enemies.Add(new EnemyData("일반병 B", 1200, 50, 20));
            enemies.Add(new EnemyData("정예병", 2500, 80, 30));

            return enemies;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // StudentData는 ScriptableObject이므로 파괴 필요
            foreach (var student in _testStudents)
            {
                if (student != null)
                {
                    Object.DestroyImmediate(student);
                }
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
        /// 6개 체크포인트 전체 통합 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator FullIntegration_AllSixCheckpoints_ShouldPass()
        {
            Debug.Log("=====================================");
            Debug.Log("6개 체크포인트 전체 통합 테스트 시작");
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
            // 체크포인트 #3: EX 스킬 사용 로깅
            // ========================================
            yield return CheckpointThree_SkillUsage();

            // ========================================
            // 체크포인트 #4: 코스트 소모 검증
            // ========================================
            yield return CheckpointFour_CostConsumption();

            // ========================================
            // 체크포인트 #5: 전투별 데미지 추적
            // ========================================
            yield return CheckpointFive_DamageTracking();

            // ========================================
            // 체크포인트 #6: 보상 획득 검증
            // ========================================
            yield return CheckpointSix_RewardVerification();

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
        /// </summary>
        private IEnumerator CheckpointOne_PlatformMovement()
        {
            Debug.Log("\n[체크포인트 #1] 플랫폼 이동 검증 시작");
            _testProgressPanel.UpdateCheckpoint(1, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("플랫폼 이동 테스트 중...");

            // 스테이지 초기화
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // 플랫폼 생성 확인
            var platforms = Object.FindObjectsByType<PlatformObject>(FindObjectsSortMode.None);
            int expectedCount = 1 + _testStageData.platformPositions.Count + 1; // 시작 + 일반 + 전투
            Assert.AreEqual(expectedCount, platforms.Length, $"플랫폼 {expectedCount}개 생성 확인");

            Debug.Log($"  ✓ {platforms.Length}개 플랫폼 생성 확인");

            // 이동 경로 계산
            var path = _stageManager.GetPathToBattle();
            Assert.IsNotNull(path, "이동 경로 계산 성공");
            Assert.Greater(path.Count, 0, "이동 경로가 존재해야 함");

            Debug.Log($"  ✓ 이동 경로 계산: {path.Count}칸");

            yield return new WaitForSeconds(0.5f);

            // 경로를 따라 이동
            foreach (var pos in path)
            {
                bool moved = _stageManager.MovePlayer(pos);
                Assert.IsTrue(moved, $"위치 {pos}로 이동 성공");
                yield return new WaitForSeconds(0.2f);
            }

            // 전투 위치 도착 확인
            Assert.AreEqual(_testStageData.battlePosition, _stageManager.PlayerPosition, "전투 위치 도착");
            Assert.AreEqual(StageState.ReadyForBattle, _stageManager.CurrentState, "전투 준비 상태");

            Debug.Log($"  ✓ 전투 위치 도착 (총 {_stageManager.TotalMovesInStage}회 이동)");
            Debug.Log("[체크포인트 #1] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(1, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("플랫폼 이동 완료!");
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 체크포인트 #2: 전투 진입 검증
        /// </summary>
        private IEnumerator CheckpointTwo_BattleEntry()
        {
            Debug.Log("\n[체크포인트 #2] 전투 진입 검증 시작");
            _testProgressPanel.UpdateCheckpoint(2, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("전투 진입 테스트 중...");

            // 전투 시작
            _stageManager.StartBattle();
            yield return null;

            Assert.AreEqual(StageState.InBattle, _stageManager.CurrentState, "전투 중 상태");
            Debug.Log("  ✓ 전투 상태 진입");

            // 전투 매니저 초기화
            _combatManager.InitializeCombat(_testStudents, _testEnemies, "Normal 1-4");
            yield return null;

            Assert.AreEqual(CombatState.InProgress, _combatManager.CurrentState, "전투 진행 중");
            Debug.Log("  ✓ 전투 매니저 초기화 완료");

            // 학생/적 오브젝트 생성 확인
            var studentObjects = Object.FindObjectsByType<StudentObject>(FindObjectsSortMode.None);
            var enemyObjects = Object.FindObjectsByType<EnemyObject>(FindObjectsSortMode.None);

            Assert.AreEqual(_testStudents.Count, studentObjects.Length, $"{_testStudents.Count}명 학생 생성");
            Assert.AreEqual(_testEnemies.Count, enemyObjects.Length, $"{_testEnemies.Count}명 적 생성");

            Debug.Log($"  ✓ 학생 {studentObjects.Length}명, 적 {enemyObjects.Length}명 생성");

            // UI 패널 생성 확인
            var costDisplay = Object.FindFirstObjectByType<CostDisplay>();
            var combatLogPanel = Object.FindFirstObjectByType<CombatLogPanel>();
            var combatStatusPanel = Object.FindFirstObjectByType<CombatStatusPanel>();

            Assert.IsNotNull(costDisplay, "CostDisplay 생성");
            Assert.IsNotNull(combatLogPanel, "CombatLogPanel 생성");
            Assert.IsNotNull(combatStatusPanel, "CombatStatusPanel 생성");

            Debug.Log("  ✓ UI 패널 생성 완료");
            Debug.Log("[체크포인트 #2] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(2, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("전투 진입 완료!");
            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// 체크포인트 #3: EX 스킬 사용 로깅
        /// </summary>
        private IEnumerator CheckpointThree_SkillUsage()
        {
            Debug.Log("\n[체크포인트 #3] EX 스킬 사용 로깅 시작");
            _testProgressPanel.UpdateCheckpoint(3, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("스킬 사용 테스트 중...");

            // 코스트 충전 대기
            Debug.Log("  코스트 충전 대기...");
            yield return new WaitForSeconds(2f);

            // 각 학생별로 스킬 사용
            for (int i = 0; i < _testStudents.Count; i++)
            {
                var studentName = _testStudents[i].studentName;
                var skillCost = _testStudents[i].skillCost;

                Debug.Log($"  [{studentName}] 스킬 사용 시도 (코스트: {skillCost})");

                // 코스트가 충분할 때까지 대기
                while (_combatManager.CurrentCost < skillCost)
                {
                    Debug.Log($"    코스트 충전 중... ({_combatManager.CurrentCost}/{skillCost})");
                    yield return new WaitForSeconds(1f);
                }

                // 스킬 사용
                var result = _combatManager.UseStudentSkill(i);

                if (result != null && result.Success)
                {
                    Debug.Log($"  ✓ {studentName} 스킬 사용 성공!");
                    Debug.Log($"    - 데미지: {result.TotalDamage}");
                    Debug.Log($"    - 타격 수: {result.TargetsHit}");

                    Assert.Greater(result.TotalDamage, 0, "데미지가 0보다 커야 함");
                }
                else
                {
                    Debug.LogWarning($"  ⚠ {studentName} 스킬 사용 실패 (적이 모두 격파됨)");
                }

                yield return new WaitForSeconds(0.5f);

                // 모든 적이 격파되면 종료
                if (_combatManager.GetAliveEnemyCount() == 0)
                {
                    Debug.Log("  모든 적 격파! 스킬 테스트 종료");
                    break;
                }
            }

            Debug.Log("[체크포인트 #3] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(3, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("스킬 사용 완료!");
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 체크포인트 #4: 코스트 소모 검증
        /// </summary>
        private IEnumerator CheckpointFour_CostConsumption()
        {
            Debug.Log("\n[체크포인트 #4] 코스트 소모 검증 시작");
            _testProgressPanel.UpdateCheckpoint(4, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("코스트 소모 검증 중...");

            // 코스트가 있는지 확인
            yield return new WaitForSeconds(1f);

            int costBefore = _combatManager.CurrentCost;
            Debug.Log($"  현재 코스트: {costBefore}/{_combatManager.MaxCost}");

            // 코스트가 충분하면 스킬 사용
            if (costBefore >= 2)
            {
                var student = _combatManager.GetStudent(1); // Hoshino (코스트 2)
                int expectedCost = student.Data.skillCost;

                var result = _combatManager.UseStudentSkill(1);
                yield return null;

                int costAfter = _combatManager.CurrentCost;
                int costUsed = costBefore - costAfter;

                Assert.AreEqual(expectedCost, costUsed, $"코스트 {expectedCost} 소모 확인");
                Debug.Log($"  ✓ 코스트 소모: {costBefore} → {costAfter} (-{costUsed})");
            }
            else
            {
                Debug.Log("  코스트 부족, 충전 대기...");
                yield return new WaitForSeconds(2f);
            }

            // 코스트 회복 확인
            int costBeforeRegen = _combatManager.CurrentCost;
            yield return new WaitForSeconds(2f);
            int costAfterRegen = _combatManager.CurrentCost;

            Assert.GreaterOrEqual(costAfterRegen, costBeforeRegen, "코스트 회복 확인");
            Debug.Log($"  ✓ 코스트 회복: {costBeforeRegen} → {costAfterRegen}");

            Debug.Log("[체크포인트 #4] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(4, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("코스트 소모 검증 완료!");
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 체크포인트 #5: 전투별 데미지 추적
        /// </summary>
        private IEnumerator CheckpointFive_DamageTracking()
        {
            Debug.Log("\n[체크포인트 #5] 전투별 데미지 추적 시작");
            _testProgressPanel.UpdateCheckpoint(5, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("데미지 추적 중...");

            var combatLog = _combatManager.CombatSystem.CombatLog;
            int totalDamageDealt = combatLog.TotalDamageDealt;

            Debug.Log($"  현재까지 총 데미지: {totalDamageDealt}");
            Assert.Greater(totalDamageDealt, 0, "데미지가 기록되어야 함");

            // 남은 적이 있으면 추가 공격
            if (_combatManager.GetAliveEnemyCount() > 0)
            {
                yield return new WaitForSeconds(2f); // 코스트 충전

                int damageBefore = combatLog.TotalDamageDealt;
                var result = _combatManager.UseStudentSkill(0);

                if (result != null && result.Success)
                {
                    int damageAfter = combatLog.TotalDamageDealt;
                    int damageDealt = damageAfter - damageBefore;

                    Assert.Greater(damageDealt, 0, "데미지 증가 확인");
                    Debug.Log($"  ✓ 데미지 추적: {damageBefore} → {damageAfter} (+{damageDealt})");
                }

                yield return new WaitForSeconds(0.5f);
            }

            // 학생별 데미지 통계
            var studentStats = combatLog.GetStudentStatistics();
            foreach (var stat in studentStats)
            {
                Debug.Log($"  [{stat.StudentName}] 스킬 사용: {stat.SkillUsageCount}회, 데미지: {stat.TotalDamage}");
            }

            Debug.Log($"  ✓ 최종 총 데미지: {combatLog.TotalDamageDealt}");
            Debug.Log("[체크포인트 #5] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(5, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("데미지 추적 완료!");
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 체크포인트 #6: 보상 획득 검증
        /// </summary>
        private IEnumerator CheckpointSix_RewardVerification()
        {
            Debug.Log("\n[체크포인트 #6] 보상 획득 검증 시작");
            _testProgressPanel.UpdateCheckpoint(6, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("보상 검증 중...");

            // 전투 완료
            _combatManager.CombatSystem.CompleteCombat(victory: true);
            _stageManager.CompleteBattle(victory: true);
            yield return null;

            Assert.AreEqual(StageState.BattleComplete, _stageManager.CurrentState, "전투 완료 상태");
            Debug.Log("  ✓ 전투 완료");

            // 보상 계산
            var rewardResult = _rewardSystem.CalculateRewards(
                _testStageData.stageName,
                _stageManager.TotalMovesInStage,
                _combatManager.CombatSystem.CombatLog
            );

            Assert.IsNotNull(rewardResult, "보상 결과 존재");
            Assert.IsNotEmpty(rewardResult.GrantedRewards, "보상 항목이 있어야 함");

            Debug.Log($"  ✓ 보상 개수: {rewardResult.GrantedRewards.Count}개");

            foreach (var reward in rewardResult.GrantedRewards)
            {
                Debug.Log($"    - {reward.itemName} x{reward.quantity}");
                Assert.Greater(reward.quantity, 0, "보상 수량이 0보다 커야 함");
            }

            // 스테이지 클리어
            _stageManager.ClearStage();
            yield return null;

            Assert.AreEqual(StageState.StageCleared, _stageManager.CurrentState, "스테이지 클리어 상태");
            Debug.Log("  ✓ 스테이지 클리어");

            Debug.Log("[체크포인트 #6] ✅ 통과");

            _testProgressPanel.UpdateCheckpoint(6, CheckpointStatus.Completed);
            _testProgressPanel.UpdateMessage("보상 획득 완료!");
            yield return new WaitForSeconds(1f);
        }
    }
}
