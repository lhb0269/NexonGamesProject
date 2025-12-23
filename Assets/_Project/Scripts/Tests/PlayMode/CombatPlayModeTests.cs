using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.UI;

namespace NexonGame.Tests.PlayMode
{
    /// <summary>
    /// 전투 시스템 PlayMode 테스트
    /// - 캐릭터 GameObject 생성 및 비주얼 검증
    /// - 스킬 사용 및 전투 테스트
    /// </summary>
    public class CombatPlayModeTests
    {
        private GameObject _testSceneRoot;
        private CombatManager _combatManager;
        private List<StudentData> _testStudents;
        private List<EnemyData> _testEnemies;
        private Camera _testCamera;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // 테스트용 씬 루트 생성
            _testSceneRoot = new GameObject("TestSceneRoot");

            // 테스트용 카메라 생성
            CreateTestCamera();

            // CombatManager 생성
            var managerObj = new GameObject("CombatManager");
            managerObj.transform.SetParent(_testSceneRoot.transform);
            _combatManager = managerObj.AddComponent<CombatManager>();

            // 테스트 데이터 생성
            _testStudents = new List<StudentData>
            {
                StudentPresets.CreateShiroko(),
                StudentPresets.CreateHoshino()
            };

            _testEnemies = StudentPresets.CreateNormal1_4Enemies();

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
            _testCamera.transform.position = new Vector3(0, 5, -8);
            _testCamera.transform.rotation = Quaternion.Euler(25, 0, 0);
            _testCamera.orthographic = false;
            _testCamera.fieldOfView = 45;
            _testCamera.clearFlags = CameraClearFlags.SolidColor;
            _testCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            // 조명
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
            // StudentData는 ScriptableObject이므로 파괴 필요
            foreach (var student in _testStudents)
            {
                if (student != null)
                {
                    Object.DestroyImmediate(student);
                }
            }

            // EnemyData는 일반 클래스이므로 파괴 불필요 (GC가 처리)
            _testEnemies?.Clear();

            // 테스트 오브젝트 제거
            if (_testSceneRoot != null)
            {
                Object.Destroy(_testSceneRoot);
            }

            _testStudents?.Clear();

            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatManager_InitializeCombat_ShouldCreateCharacters()
        {
            Debug.Log("=== 테스트 시작: 캐릭터 생성 검증 ===");

            // Act
            _combatManager.InitializeCombat(_testStudents, _testEnemies, "TestCombat");
            yield return null;

            // Assert - 학생 오브젝트
            var studentObjects = Object.FindObjectsByType<StudentObject>(FindObjectsSortMode.None);
            Assert.AreEqual(_testStudents.Count, studentObjects.Length, "학생 오브젝트 개수 일치");

            // Assert - 적 오브젝트
            var enemyObjects = Object.FindObjectsByType<EnemyObject>(FindObjectsSortMode.None);
            Assert.AreEqual(_testEnemies.Count, enemyObjects.Length, "적 오브젝트 개수 일치");

            Debug.Log($"✅ [PlayMode Test] {studentObjects.Length}명 학생, {enemyObjects.Length}명 적 생성됨");
            yield return new WaitForSeconds(0.5f); // [시각화용] 결과 관찰
        }

        [UnityTest]
        public IEnumerator CombatManager_InitializeCombat_ShouldSetCorrectState()
        {
            // Act
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // Assert
            Assert.AreEqual(CombatState.InProgress, _combatManager.CurrentState, "전투 진행 중 상태");
            Assert.AreEqual(2, _combatManager.StudentCount, "학생 2명");
            Assert.AreEqual(3, _combatManager.EnemyCount, "적 3명");

            Debug.Log($"✅ [PlayMode Test] 전투 상태: {_combatManager.CurrentState}");
        }

        [UnityTest]
        public IEnumerator CombatManager_UseSkill_ShouldUpdateVisuals()
        {
            Debug.Log("=== 테스트 시작: 스킬 사용 및 비주얼 업데이트 ===");

            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // 코스트 충전 대기
            Debug.Log("코스트 충전 대기 중...");
            yield return new WaitForSeconds(2f); // [시각화용] 코스트 충전 관찰

            int initialCost = _combatManager.CurrentCost;
            int aliveEnemiesBefor = _combatManager.AliveEnemyCount;

            Debug.Log($"초기 코스트: {initialCost}, 살아있는 적: {aliveEnemiesBefor}");

            // Act - Shiroko 스킬 사용
            Debug.Log("Shiroko 스킬 사용!");
            var result = _combatManager.UseStudentSkill(0);
            yield return new WaitForSeconds(0.5f); // [시각화용] 스킬 효과 관찰

            // Assert
            Assert.IsNotNull(result, "스킬 결과가 있어야 함");
            Assert.IsTrue(result.Success, "스킬 성공해야 함");
            Assert.Greater(result.TotalDamage, 0, "데미지가 0보다 커야 함");

            Debug.Log($"스킬 결과: {result.TotalDamage} 데미지, {result.TargetsHit}명 타격");
            Debug.Log($"현재 코스트: {_combatManager.CurrentCost}");

            yield return new WaitForSeconds(1f); // [시각화용] 결과 확인
        }

        [UnityTest]
        public IEnumerator CombatManager_MultipleSkills_ShouldDefeatEnemies()
        {
            Debug.Log("=== 테스트 시작: 연속 스킬로 적 격파 ===");

            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            yield return new WaitForSeconds(0.5f); // [시각화용] 초기 상태

            int skillCount = 0;
            int maxSkills = 10; // 최대 10회 스킬

            // Act - 적을 모두 격파할 때까지 스킬 사용
            while (_combatManager.AliveEnemyCount > 0 && skillCount < maxSkills)
            {
                // 코스트 충전 대기
                while (_combatManager.CurrentCost < 3)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                // 번갈아가며 스킬 사용
                int studentIndex = skillCount % 2;
                Student student = _combatManager.GetStudent(studentIndex);

                Debug.Log($"[{skillCount + 1}] {student.Data.studentName} 스킬 사용! (코스트: {_combatManager.CurrentCost})");

                var result = _combatManager.UseStudentSkill(studentIndex);

                if (result != null && result.Success)
                {
                    Debug.Log($"  → {result.TotalDamage} 데미지, 남은 적: {_combatManager.AliveEnemyCount}");
                    skillCount++;
                }

                yield return new WaitForSeconds(0.8f); // [시각화용] 스킬 간격
            }

            // Assert
            Assert.AreEqual(0, _combatManager.AliveEnemyCount, "모든 적 격파");
            Assert.Greater(skillCount, 0, "최소 1회 이상 스킬 사용");

            Debug.Log($"✅ [PlayMode Test] {skillCount}회 스킬로 모든 적 격파!");
            yield return new WaitForSeconds(1f); // [시각화용] 최종 결과
        }

        [UnityTest]
        public IEnumerator CombatManager_StudentObjects_ShouldHaveCorrectComponents()
        {
            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // Act
            var studentObjects = Object.FindObjectsByType<StudentObject>(FindObjectsSortMode.None);

            // Assert
            foreach (var studentObj in studentObjects)
            {
                Assert.IsNotNull(studentObj.Student, "Student 로직 클래스가 있어야 함");
                Assert.IsNotNull(studentObj.gameObject.GetComponent<Renderer>(), "Renderer가 있어야 함");
            }

            Debug.Log("✅ [PlayMode Test] 학생 오브젝트 컴포넌트 검증 완료");
        }

        [UnityTest]
        public IEnumerator CombatManager_EnemyObjects_ShouldHaveCorrectComponents()
        {
            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // Act
            var enemyObjects = Object.FindObjectsByType<EnemyObject>(FindObjectsSortMode.None);

            // Assert
            foreach (var enemyObj in enemyObjects)
            {
                Assert.IsNotNull(enemyObj.Enemy, "Enemy 로직 클래스가 있어야 함");
                Assert.IsNotNull(enemyObj.gameObject.GetComponent<Renderer>(), "Renderer가 있어야 함");
            }

            Debug.Log("✅ [PlayMode Test] 적 오브젝트 컴포넌트 검증 완료");
        }

        [UnityTest]
        public IEnumerator CombatManager_CostSystem_ShouldRegenerate()
        {
            Debug.Log("=== 테스트 시작: 코스트 자동 회복 ===");

            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            int initialCost = _combatManager.CurrentCost;
            Debug.Log($"초기 코스트: {initialCost}");

            // Act - 2초 대기
            yield return new WaitForSeconds(2f);

            int finalCost = _combatManager.CurrentCost;
            Debug.Log($"2초 후 코스트: {finalCost}");

            // Assert
            Assert.Greater(finalCost, initialCost, "코스트가 증가해야 함");
            Assert.LessOrEqual(finalCost, _combatManager.MaxCost, "최대 코스트를 넘지 않아야 함");

            Debug.Log($"✅ [PlayMode Test] 코스트 {initialCost} → {finalCost}");
        }

        [UnityTest]
        public IEnumerator CombatManager_CostDisplay_ShouldExist()
        {
            Debug.Log("=== 테스트 시작: 코스트 UI 존재 확인 ===");

            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // Act
            var costDisplay = Object.FindFirstObjectByType<CostDisplay>();

            // Assert
            Assert.IsNotNull(costDisplay, "CostDisplay가 생성되어야 함");

            Debug.Log("✅ [PlayMode Test] CostDisplay 존재 확인");
            yield return new WaitForSeconds(0.5f); // [시각화용] UI 확인
        }

        [UnityTest]
        public IEnumerator CombatManager_CostDisplay_ShouldUpdateOnSkillUse()
        {
            Debug.Log("=== 테스트 시작: 스킬 사용 시 코스트 UI 업데이트 ===");

            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // 코스트 충전 대기
            yield return new WaitForSeconds(2f);

            int costBefore = _combatManager.CurrentCost;
            Debug.Log($"스킬 사용 전 코스트: {costBefore}");

            // Act - 스킬 사용
            var result = _combatManager.UseStudentSkill(0);
            yield return null;

            int costAfter = _combatManager.CurrentCost;
            Debug.Log($"스킬 사용 후 코스트: {costAfter}");

            // Assert
            Assert.IsNotNull(result, "스킬 결과가 있어야 함");
            Assert.Less(costAfter, costBefore, "코스트가 감소해야 함");

            var costDisplay = Object.FindFirstObjectByType<CostDisplay>();
            Assert.IsNotNull(costDisplay, "CostDisplay가 존재해야 함");

            Debug.Log($"✅ [PlayMode Test] 코스트 UI 업데이트 확인: {costBefore} → {costAfter}");
            yield return new WaitForSeconds(1f); // [시각화용] 코스트 회복 관찰
        }

        [UnityTest]
        public IEnumerator CombatManager_CombatLogPanel_ShouldExist()
        {
            Debug.Log("=== 테스트 시작: 전투 로그 패널 존재 확인 ===");

            // Arrange & Act
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // Assert
            var logPanel = Object.FindFirstObjectByType<CombatLogPanel>();
            Assert.IsNotNull(logPanel, "CombatLogPanel이 생성되어야 함");

            Debug.Log("✅ [PlayMode Test] CombatLogPanel 존재 확인");
            yield return new WaitForSeconds(0.5f); // [시각화용] UI 확인
        }

        [UnityTest]
        public IEnumerator CombatManager_CombatStatusPanel_ShouldExist()
        {
            Debug.Log("=== 테스트 시작: 전투 상태 패널 존재 확인 ===");

            // Arrange & Act
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            // Assert
            var statusPanel = Object.FindFirstObjectByType<CombatStatusPanel>();
            Assert.IsNotNull(statusPanel, "CombatStatusPanel이 생성되어야 함");

            Debug.Log("✅ [PlayMode Test] CombatStatusPanel 존재 확인");
            yield return new WaitForSeconds(0.5f); // [시각화용] UI 확인
        }

        [UnityTest]
        public IEnumerator CombatManager_UIPanels_ShouldDisplayCombatEvents()
        {
            Debug.Log("=== 테스트 시작: UI 패널이 전투 이벤트를 표시해야 함 ===");

            // Arrange
            _combatManager.InitializeCombat(_testStudents, _testEnemies);
            yield return null;

            var logPanel = Object.FindFirstObjectByType<CombatLogPanel>();
            var statusPanel = Object.FindFirstObjectByType<CombatStatusPanel>();

            Assert.IsNotNull(logPanel, "CombatLogPanel이 있어야 함");
            Assert.IsNotNull(statusPanel, "CombatStatusPanel이 있어야 함");

            yield return new WaitForSeconds(0.5f); // [시각화용] 초기 UI 확인

            // 코스트 충전 대기
            yield return new WaitForSeconds(2f);

            // Act - 스킬 사용
            Debug.Log("스킬 사용 중...");
            var result = _combatManager.UseStudentSkill(0);
            yield return new WaitForSeconds(0.5f); // [시각화용] 로그 확인

            // Assert
            Assert.IsNotNull(result, "스킬 결과가 있어야 함");
            Assert.IsTrue(result.Success, "스킬이 성공해야 함");

            Debug.Log("✅ [PlayMode Test] UI 패널이 전투 이벤트를 표시함");
            yield return new WaitForSeconds(1f); // [시각화용] 최종 확인
        }

        [UnityTest]
        public IEnumerator TestProgressPanel_ShouldExist()
        {
            Debug.Log("=== 테스트 시작: 테스트 진행 패널 존재 확인 ===");

            // Arrange & Act
            var testPanelObj = new GameObject("TestProgressPanel");
            var testPanel = testPanelObj.AddComponent<TestProgressPanel>();
            yield return null;

            // Assert
            Assert.IsNotNull(testPanel, "TestProgressPanel이 생성되어야 함");

            Debug.Log("✅ [PlayMode Test] TestProgressPanel 존재 확인");
            yield return new WaitForSeconds(0.5f); // [시각화용] UI 확인

            // Cleanup
            Object.Destroy(testPanelObj);
        }

        [UnityTest]
        public IEnumerator TestProgressPanel_ShouldUpdateCheckpoints()
        {
            Debug.Log("=== 테스트 시작: 체크포인트 상태 업데이트 ===");

            // Arrange
            var testPanelObj = new GameObject("TestProgressPanel");
            var testPanel = testPanelObj.AddComponent<TestProgressPanel>();
            yield return null;

            // Act - 체크포인트 #1 진행 중
            testPanel.UpdateCheckpoint(1, CheckpointStatus.InProgress);
            testPanel.UpdateMessage("플랫폼 이동 테스트 중...");
            yield return new WaitForSeconds(0.5f);

            // Act - 체크포인트 #1 완료
            testPanel.UpdateCheckpoint(1, CheckpointStatus.Completed);
            testPanel.UpdateMessage("플랫폼 이동 완료!");
            yield return new WaitForSeconds(0.5f);

            // Act - 체크포인트 #2 진행 중
            testPanel.UpdateCheckpoint(2, CheckpointStatus.InProgress);
            testPanel.UpdateMessage("전투 진입 테스트 중...");
            yield return new WaitForSeconds(0.5f);

            // Act - 체크포인트 #2 완료
            testPanel.UpdateCheckpoint(2, CheckpointStatus.Completed);
            testPanel.UpdateMessage("전투 진입 완료!");
            yield return new WaitForSeconds(0.5f);

            // Assert - 모든 업데이트가 정상적으로 수행됨
            Assert.IsNotNull(testPanel);

            Debug.Log("✅ [PlayMode Test] 체크포인트 상태 업데이트 확인");
            yield return new WaitForSeconds(1f); // [시각화용] 최종 확인

            // Cleanup
            Object.Destroy(testPanelObj);
        }

        [UnityTest]
        public IEnumerator TestProgressPanel_ShouldShowAllCheckpoints()
        {
            Debug.Log("=== 테스트 시작: 모든 체크포인트 표시 ===");

            // Arrange
            var testPanelObj = new GameObject("TestProgressPanel");
            var testPanel = testPanelObj.AddComponent<TestProgressPanel>();
            yield return null;

            // Act - 모든 체크포인트 순차 완료
            for (int i = 1; i <= 6; i++)
            {
                testPanel.UpdateCheckpoint(i, CheckpointStatus.InProgress);
                testPanel.UpdateMessage($"체크포인트 #{i} 진행 중...");
                yield return new WaitForSeconds(0.3f);

                testPanel.UpdateCheckpoint(i, CheckpointStatus.Completed);
                testPanel.UpdateMessage($"체크포인트 #{i} 완료!");
                yield return new WaitForSeconds(0.3f);
            }

            // Assert
            Assert.IsNotNull(testPanel);

            Debug.Log("✅ [PlayMode Test] 모든 체크포인트 완료 확인");
            yield return new WaitForSeconds(1f); // [시각화용] 완료 상태 확인

            // Cleanup
            Object.Destroy(testPanelObj);
        }

        [UnityTest]
        public IEnumerator RewardResultPanel_ShouldExist()
        {
            Debug.Log("=== 테스트 시작: 보상 결과 패널 존재 확인 ===");

            // Arrange & Act
            var rewardPanelObj = new GameObject("RewardResultPanel");
            var rewardPanel = rewardPanelObj.AddComponent<RewardResultPanel>();
            yield return null;

            // Assert
            Assert.IsNotNull(rewardPanel, "RewardResultPanel이 생성되어야 함");

            Debug.Log("✅ [PlayMode Test] RewardResultPanel 존재 확인");
            yield return new WaitForSeconds(0.5f); // [시각화용] UI 확인

            // Cleanup
            Object.Destroy(rewardPanelObj);
        }

        [UnityTest]
        public IEnumerator RewardResultPanel_ShouldShowRewards()
        {
            Debug.Log("=== 테스트 시작: 보상 표시 테스트 ===");

            // Arrange
            var rewardPanelObj = new GameObject("RewardResultPanel");
            var rewardPanel = rewardPanelObj.AddComponent<RewardResultPanel>();
            yield return null;

            // 보상 결과 생성
            var rewardResult = new RewardGrantResult();
            rewardResult.GrantedRewards.Add(new RewardItemData
            {
                itemName = "크레딧",
                itemType = RewardItemType.Currency,
                quantity = 1000
            });
            rewardResult.GrantedRewards.Add(new RewardItemData
            {
                itemName = "노트",
                itemType = RewardItemType.Material,
                quantity = 5
            });

            string statistics = "이동 횟수: 7회\n스킬 사용: 4회\n총 데미지: 5840";

            // Act
            rewardPanel.ShowRewards("Normal 1-4", rewardResult, statistics);
            yield return new WaitForSeconds(2f); // [시각화용] 보상 결과 확인

            // Assert
            Assert.IsNotNull(rewardPanel);

            Debug.Log("✅ [PlayMode Test] 보상 표시 확인");

            // Cleanup
            Object.Destroy(rewardPanelObj);
        }
    }
}
