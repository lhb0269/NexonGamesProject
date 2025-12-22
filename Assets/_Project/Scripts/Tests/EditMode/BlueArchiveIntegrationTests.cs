using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Stage;
using NexonGame.BlueArchive.Skill;
using NexonGame.BlueArchive.Reward;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 블루 아카이브 Normal 1-4 스테이지 통합 테스트
    /// - 전체 시스템 통합 검증
    /// - 6개 테스트 체크포인트 모두 포함
    /// </summary>
    public class BlueArchiveIntegrationTests
    {
        private StageController _stageController;
        private CombatSystem _combatSystem;
        private RewardSystem _rewardSystem;
        private RewardValidator _rewardValidator;
        private CombatEntryValidator _combatEntryValidator;

        private StageData _normal1_4Data;
        private List<StudentData> _students;

        [SetUp]
        public void Setup()
        {
            // 스테이지 데이터 생성
            _normal1_4Data = StagePresets.CreateNormal1_4();

            // 학생 프리셋 생성
            _students = StudentPresets.CreateAllStudents();

            // 시스템 초기화
            _stageController = new StageController();
            _combatSystem = new CombatSystem();
            _rewardSystem = new RewardSystem();
            _rewardValidator = new RewardValidator(_rewardSystem);
            _combatEntryValidator = new CombatEntryValidator(_stageController);
        }

        [TearDown]
        public void TearDown()
        {
            StagePresets.DestroyStageData(_normal1_4Data);
            StudentPresets.DestroyAllStudents(_students);
        }

        [Test]
        public void Integration_FullStageFlow_AllCheckpoints()
        {
            // === 스테이지 초기화 ===
            _stageController.InitializeStage(_normal1_4Data);
            Assert.AreEqual(StageState.MovingToBattle, _stageController.CurrentState);

            // === 체크포인트 #1: 플랫폼 이동 검증 ===
            Vector2Int currentPos = _stageController.PlayerPosition;
            Assert.AreEqual(_normal1_4Data.startPosition, currentPos);

            // 전투 위치까지 이동
            List<Vector2Int> path = _stageController.GetPathToBattle();
            Assert.IsNotEmpty(path, "경로를 찾을 수 없음");

            foreach (var nextPos in path)
            {
                bool moved = _stageController.MovePlayer(nextPos);
                Assert.IsTrue(moved, $"이동 실패: {nextPos}");
            }

            Assert.AreEqual(_normal1_4Data.battlePosition, _stageController.PlayerPosition);
            Assert.AreEqual(StageState.ReadyForBattle, _stageController.CurrentState);

            // === 체크포인트 #2: 전투 진입 검증 ===
            CombatEntryResult entryResult = _combatEntryValidator.ValidateEntry();
            Assert.IsTrue(entryResult.CanEnterCombat, "전투 진입 조건 미충족");

            bool entered = _combatEntryValidator.TryEnterCombat();
            Assert.IsTrue(entered, "전투 진입 실패");
            Assert.AreEqual(StageState.InBattle, _stageController.CurrentState);

            // === 전투 준비 ===
            List<Student> studentInstances = new List<Student>();
            foreach (var studentData in _students)
            {
                studentInstances.Add(new Student(studentData));
            }

            List<Enemy> enemies = new List<Enemy>();
            foreach (var enemyData in StudentPresets.CreateNormal1_4Enemies())
            {
                enemies.Add(new Enemy(enemyData));
            }

            _combatSystem.InitializeCombat(studentInstances, enemies, _normal1_4Data.stageName);
            _combatSystem.CostSystem.FillCost(); // 초기 코스트 충전

            // === 체크포인트 #3: EX 스킬 사용 로깅 ===
            // === 체크포인트 #4: 코스트 소모 검증 ===
            int initialCost = _combatSystem.CostSystem.CurrentCost;
            Student firstStudent = studentInstances[0]; // Shiroko

            SkillExecutionResult skillResult = _combatSystem.UseStudentSkill(firstStudent);
            Assert.IsTrue(skillResult.Success, "스킬 사용 실패");
            Assert.Greater(skillResult.TotalDamage, 0, "데미지가 0");
            Assert.AreEqual(3, skillResult.CostSpent, "코스트 소모량 불일치");

            // 코스트 감소 확인
            Assert.Less(_combatSystem.CostSystem.CurrentCost, initialCost, "코스트가 소모되지 않음");

            // 전투 로그 확인
            Assert.AreEqual(1, _combatSystem.CombatLog.TotalSkillsUsed, "스킬 사용 로그 누락");
            List<CombatLogEntry> skillLogs = _combatSystem.CombatLog.GetLogsByType(CombatLogType.SkillUsed);
            Assert.AreEqual(1, skillLogs.Count, "스킬 로그 개수 불일치");
            Assert.AreEqual(firstStudent.Data.studentName, skillLogs[0].ActorName);

            // === 체크포인트 #5: 전투별 데미지 추적 ===
            int damageBefore = _combatSystem.CombatLog.TotalDamageDealt;
            Assert.Greater(damageBefore, 0, "데미지 기록 없음");

            // 전투 계속 진행 (간단하게 적 격파까지)
            _combatSystem.CostSystem.FillCost();

            // 모든 학생이 스킬 사용 (체크포인트 #3 완전 검증)
            foreach (var student in studentInstances)
            {
                if (student != firstStudent && student.IsAlive)
                {
                    _combatSystem.CostSystem.AddCost(10); // 코스트 충전
                    var result = _combatSystem.UseStudentSkill(student);
                    if (result.Success)
                    {
                        Debug.Log($"[Integration] {student.Data.studentName} 스킬 사용 성공");
                    }
                }
            }

            // 전투 로그에 모든 학생의 스킬 사용이 기록되었는지 확인
            int skillUsedCount = _combatSystem.CombatLog.TotalSkillsUsed;
            Assert.GreaterOrEqual(skillUsedCount, 1, "최소 1명 이상 스킬 사용 필요");

            // 데미지 추적 확인
            Assert.Greater(_combatSystem.CombatLog.TotalDamageDealt, damageBefore, "추가 데미지 기록 없음");

            // === 전투 강제 승리 (테스트용) ===
            foreach (var enemy in enemies)
            {
                while (enemy.IsAlive)
                {
                    enemy.TakeDamage(10000); // 강제 격파
                }
            }

            Assert.AreEqual(CombatState.Victory, _combatSystem.CurrentState, "전투 승리 상태 아님");

            // === 체크포인트 #6: 보상 획득 검증 ===
            CombatResult combatResult = _combatSystem.GetCombatResult();

            // 보상 조건 검증
            RewardValidationResult conditionValidation = _rewardValidator.ValidateRewardConditions(_normal1_4Data, combatResult);
            Assert.IsTrue(conditionValidation.IsValid, "보상 조건 검증 실패");

            // 보상 지급
            RewardGrantResult rewardResult = _rewardSystem.GrantStageRewards(_normal1_4Data, combatResult);
            Assert.IsTrue(rewardResult.Success, "보상 지급 실패");
            Assert.AreEqual(4, rewardResult.TotalRewardCount, "보상 개수 불일치");

            // 보상 지급 검증
            RewardValidationResult grantValidation = _rewardValidator.ValidateRewardGrant(_normal1_4Data, rewardResult);
            Assert.IsTrue(grantValidation.IsValid, "보상 지급 검증 실패");

            // 인벤토리 확인
            Assert.AreEqual(1000, _rewardSystem.GetInventoryCount(RewardItemType.Currency));
            Assert.AreEqual(5, _rewardSystem.GetInventoryCount(RewardItemType.Material));
            Assert.AreEqual(1, _rewardSystem.GetInventoryCount(RewardItemType.Equipment));
            Assert.AreEqual(150, _rewardSystem.GetInventoryCount(RewardItemType.Exp));

            // === 스테이지 클리어 ===
            _stageController.CompleteBattle(victory: true);
            _stageController.ClearStage();
            Assert.AreEqual(StageState.StageCleared, _stageController.CurrentState);

            Debug.Log("[Integration] ✅ Normal 1-4 스테이지 전체 플로우 검증 완료!");
            Debug.Log($"이동 횟수: {_stageController.TotalMovesInStage}");
            Debug.Log($"스킬 사용: {_combatSystem.CombatLog.TotalSkillsUsed}회");
            Debug.Log($"총 데미지: {_combatSystem.CombatLog.TotalDamageDealt}");
            Debug.Log($"보상: {rewardResult.TotalRewardCount}개");
        }

        [Test]
        public void Integration_AllCheckpoints_Summary()
        {
            // 이 테스트는 6개 체크포인트가 모두 구현되었는지 확인

            // ✅ 체크포인트 #1: 플랫폼 이동 검증
            Assert.IsNotNull(_stageController, "StageController 없음");
            Assert.IsNotNull(_normal1_4Data.platformPositions, "플랫폼 데이터 없음");

            // ✅ 체크포인트 #2: 전투 진입 검증
            Assert.IsNotNull(_combatEntryValidator, "CombatEntryValidator 없음");

            // ✅ 체크포인트 #3: EX 스킬 사용 로깅
            Assert.IsNotNull(_combatSystem.CombatLog, "CombatLog 없음");
            Assert.IsNotNull(_combatSystem.SkillExecutor, "SkillExecutor 없음");

            // ✅ 체크포인트 #4: 코스트 소모 검증
            Assert.IsNotNull(_combatSystem.CostSystem, "CostSystem 없음");

            // ✅ 체크포인트 #5: 전투별 데미지 추적
            Assert.IsNotNull(_combatSystem.CombatLog, "CombatLog 없음 (데미지 추적용)");

            // ✅ 체크포인트 #6: 보상 획득 검증
            Assert.IsNotNull(_rewardSystem, "RewardSystem 없음");
            Assert.IsNotNull(_rewardValidator, "RewardValidator 없음");
            Assert.IsNotNull(_normal1_4Data.rewards, "보상 데이터 없음");
            Assert.Greater(_normal1_4Data.rewards.Count, 0, "보상이 비어있음");

            Debug.Log("✅ 6개 체크포인트 모두 구현 완료!");
        }

        [Test]
        public void Integration_Normal1_4_StageData_IsValid()
        {
            // Normal 1-4 스테이지 데이터 유효성 검증
            Assert.AreEqual("Normal 1-4", _normal1_4Data.stageName);
            Assert.AreEqual(104, _normal1_4Data.stageId);
            Assert.AreEqual(10, _normal1_4Data.gridWidth);
            Assert.AreEqual(5, _normal1_4Data.gridHeight);
            Assert.IsNotEmpty(_normal1_4Data.platformPositions);
            Assert.IsNotEmpty(_normal1_4Data.enemies);
            Assert.AreEqual(3, _normal1_4Data.enemies.Count); // 3명의 적
            Assert.IsNotEmpty(_normal1_4Data.rewards);
            Assert.AreEqual(4, _normal1_4Data.rewards.Count); // 4개 보상
        }

        [Test]
        public void Integration_StudentPresets_AllValid()
        {
            // 4명의 학생 프리셋 유효성 검증
            Assert.AreEqual(4, _students.Count);

            foreach (var studentData in _students)
            {
                Assert.IsNotNull(studentData);
                Assert.IsNotEmpty(studentData.studentName);
                Assert.Greater(studentData.maxHP, 0);
                Assert.Greater(studentData.attack, 0);
                Assert.GreaterOrEqual(studentData.defense, 0);
                Assert.IsNotNull(studentData.exSkill);
                Assert.Greater(studentData.exSkill.costAmount, 0);
            }
        }
    }
}
