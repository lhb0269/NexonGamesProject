using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Data;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 전투 시스템 테스트 (테스트 체크포인트 #3, #4, #5 관련)
    /// </summary>
    public class CombatSystemTests
    {
        private CombatSystem _combatSystem;
        private StudentData _testStudentData;
        private SkillData _testSkillData;

        [SetUp]
        public void Setup()
        {
            _combatSystem = new CombatSystem();

            // 테스트용 스킬 데이터
            _testSkillData = ScriptableObject.CreateInstance<SkillData>();
            _testSkillData.skillName = "Test EX Skill";
            _testSkillData.costAmount = 3;
            _testSkillData.baseDamage = 500;
            _testSkillData.damageMultiplier = 1.5f;
            _testSkillData.targetType = SkillTargetType.Single;
            _testSkillData.cooldownTime = 20f;

            // 테스트용 학생 데이터
            _testStudentData = ScriptableObject.CreateInstance<StudentData>();
            _testStudentData.studentName = "Test Student";
            _testStudentData.studentId = 1;
            _testStudentData.maxHP = 1000;
            _testStudentData.attack = 100;
            _testStudentData.defense = 50;
            _testStudentData.exSkill = _testSkillData;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testSkillData);
            Object.DestroyImmediate(_testStudentData);
        }

        [Test]
        public void CombatSystem_Initialization_ShouldBeNotStarted()
        {
            // Assert
            Assert.AreEqual(CombatState.NotStarted, _combatSystem.CurrentState);
            Assert.IsNotNull(_combatSystem.CostSystem);
            Assert.IsNotNull(_combatSystem.CombatLog);
            Assert.IsNotNull(_combatSystem.SkillExecutor);
        }

        [Test]
        public void CombatSystem_InitializeCombat_ShouldSetup()
        {
            // Arrange
            List<Student> students = new List<Student>
            {
                new Student(_testStudentData)
            };

            List<Enemy> enemies = new List<Enemy>
            {
                new Enemy(new EnemyData("Test Enemy", 1000, 50, 20))
            };

            // Act
            _combatSystem.InitializeCombat(students, enemies, "Test Stage");

            // Assert
            Assert.AreEqual(CombatState.InProgress, _combatSystem.CurrentState);
            Assert.AreEqual(1, _combatSystem.Students.Count);
            Assert.AreEqual(1, _combatSystem.Enemies.Count);
        }

        [Test]
        public void CombatSystem_UseStudentSkill_ShouldSucceed()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            List<Student> students = new List<Student> { student };
            List<Enemy> enemies = new List<Enemy> { enemy };

            _combatSystem.InitializeCombat(students, enemies);
            _combatSystem.CostSystem.FillCost(); // 코스트 충전

            // Act
            SkillExecutionResult result = _combatSystem.UseStudentSkill(student);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.Greater(result.TotalDamage, 0);
            Assert.AreEqual(1, result.TargetsHit.Count);
            Assert.AreEqual(3, result.CostSpent);
        }

        [Test]
        public void CombatSystem_UseStudentSkill_ShouldFailWhenNotEnoughCost()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            _combatSystem.CostSystem.SetCost(0); // 코스트 0

            // Act
            SkillExecutionResult result = _combatSystem.UseStudentSkill(student);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("코스트 부족"));
        }

        [Test]
        public void CombatSystem_UseStudentSkill_ShouldFailWhenOnCooldown()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            _combatSystem.CostSystem.FillCost();
            _combatSystem.UseStudentSkill(student); // 첫 번째 사용
            _combatSystem.CostSystem.FillCost(); // 코스트 다시 충전

            // Act - 쿨다운 중 두 번째 사용 시도
            SkillExecutionResult result = _combatSystem.UseStudentSkill(student);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("쿨다운"));
        }

        [Test]
        public void CombatSystem_ProcessEnemyAttack_ShouldDamageStudent()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            int initialHP = student.CurrentHP;

            // Act
            _combatSystem.ProcessEnemyAttack(enemy);

            // Assert
            Assert.Less(student.CurrentHP, initialHP);
        }

        [Test]
        public void CombatSystem_Update_ShouldRegenerateCost()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            _combatSystem.CostSystem.SetCost(0);

            // Act
            _combatSystem.Update(3f); // 3초 경과 (3 코스트 회복)

            // Assert
            Assert.AreEqual(3, _combatSystem.CostSystem.CurrentCost);
        }

        [Test]
        public void CombatSystem_Update_ShouldReduceSkillCooldown()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            _combatSystem.CostSystem.FillCost();
            _combatSystem.UseStudentSkill(student);

            float initialCooldown = student.SkillCooldownRemaining;

            // Act
            _combatSystem.Update(5f);

            // Assert
            Assert.Less(student.SkillCooldownRemaining, initialCooldown);
        }

        [Test]
        public void CombatSystem_Victory_WhenAllEnemiesDefeated()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy weakEnemy = new Enemy(new EnemyData("Weak Enemy", 100, 10, 0));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { weakEnemy }
            );

            _combatSystem.CostSystem.FillCost();

            // Act
            _combatSystem.UseStudentSkill(student); // 적 격파

            // Assert
            Assert.AreEqual(CombatState.Victory, _combatSystem.CurrentState);
        }

        [Test]
        public void CombatSystem_Defeat_WhenAllStudentsDefeated()
        {
            // Arrange
            Student weakStudent = new Student(_testStudentData);
            weakStudent.TakeDamage(900); // HP를 100으로 낮춤

            Enemy strongEnemy = new Enemy(new EnemyData("Strong Enemy", 5000, 200, 0));

            _combatSystem.InitializeCombat(
                new List<Student> { weakStudent },
                new List<Enemy> { strongEnemy }
            );

            // Act
            _combatSystem.ProcessEnemyAttack(strongEnemy); // 학생 격파

            // Assert
            Assert.AreEqual(CombatState.Defeat, _combatSystem.CurrentState);
        }

        [Test]
        public void CombatSystem_GetAliveCount_ShouldReturnCorrectCount()
        {
            // Arrange
            Student student1 = new Student(_testStudentData);
            Student student2 = new Student(_testStudentData);
            Enemy enemy1 = new Enemy(new EnemyData("Enemy 1", 1000, 50, 20));
            Enemy enemy2 = new Enemy(new EnemyData("Enemy 2", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student1, student2 },
                new List<Enemy> { enemy1, enemy2 }
            );

            // Act
            student1.TakeDamage(1000); // 학생 1 격파
            enemy1.TakeDamage(1000);   // 적 1 격파

            // Assert
            Assert.AreEqual(1, _combatSystem.GetAliveStudentCount());
            Assert.AreEqual(1, _combatSystem.GetAliveEnemyCount());
        }

        [Test]
        public void CombatSystem_GetStudentsWithAvailableSkills_ShouldReturnCorrectList()
        {
            // Arrange
            Student student1 = new Student(_testStudentData);
            Student student2 = new Student(_testStudentData);

            _combatSystem.InitializeCombat(
                new List<Student> { student1, student2 },
                new List<Enemy> { new Enemy(new EnemyData("Enemy", 1000, 50, 20)) }
            );

            _combatSystem.CostSystem.FillCost();
            _combatSystem.UseStudentSkill(student1); // student1 쿨다운 시작

            // Act
            List<Student> available = _combatSystem.GetStudentsWithAvailableSkills();

            // Assert
            Assert.AreEqual(1, available.Count);
            Assert.Contains(student2, available);
            Assert.IsFalse(available.Contains(student1));
        }

        [Test]
        public void CombatSystem_GetCombatResult_ShouldReturnCorrectData()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Enemy", 100, 10, 0));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            _combatSystem.CostSystem.FillCost();
            _combatSystem.UseStudentSkill(student); // 적 격파

            // Act
            CombatResult result = _combatSystem.GetCombatResult();

            // Assert
            Assert.AreEqual(CombatState.Victory, result.State);
            Assert.Greater(result.TotalDamageDealt, 0);
            Assert.AreEqual(1, result.EnemiesDefeated);
            Assert.AreEqual(1, result.SkillsUsed);
            Assert.IsTrue(result.AllStudentsAlive);
        }

        [Test]
        public void CombatSystem_GetCombatStatus_ShouldReturnValidString()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            // Act
            string status = _combatSystem.GetCombatStatus();

            // Assert
            Assert.IsNotEmpty(status);
            Assert.IsTrue(status.Contains("전투 상태"));
        }

        [Test]
        public void CombatSystem_Reset_ShouldClearAllData()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Enemy", 1000, 50, 20));

            _combatSystem.InitializeCombat(
                new List<Student> { student },
                new List<Enemy> { enemy }
            );

            _combatSystem.CostSystem.FillCost();
            _combatSystem.UseStudentSkill(student);

            // Act
            _combatSystem.Reset();

            // Assert
            Assert.AreEqual(CombatState.NotStarted, _combatSystem.CurrentState);
            Assert.AreEqual(0, _combatSystem.Students.Count);
            Assert.AreEqual(0, _combatSystem.Enemies.Count);
            Assert.AreEqual(0, _combatSystem.CombatLog.LogCount);
        }

        [Test]
        public void CombatSystem_FullCombat_Scenario()
        {
            // Arrange - 2명의 학생 vs 3명의 적
            Student student1 = new Student(_testStudentData);
            Student student2 = new Student(_testStudentData);

            Enemy enemy1 = new Enemy(new EnemyData("Enemy 1", 500, 30, 10));
            Enemy enemy2 = new Enemy(new EnemyData("Enemy 2", 500, 30, 10));
            Enemy enemy3 = new Enemy(new EnemyData("Enemy 3", 500, 30, 10));

            _combatSystem.InitializeCombat(
                new List<Student> { student1, student2 },
                new List<Enemy> { enemy1, enemy2, enemy3 },
                "Full Combat Test"
            );

            // Act - 전투 시뮬레이션
            _combatSystem.CostSystem.FillCost();

            // 턴 1: student1 스킬 사용
            SkillExecutionResult result1 = _combatSystem.UseStudentSkill(student1);
            Assert.IsTrue(result1.Success);

            // 코스트 회복 대기
            _combatSystem.Update(5f);
            _combatSystem.CostSystem.AddCost(5); // 빠른 진행을 위해 코스트 추가

            // 턴 2: student2 스킬 사용
            SkillExecutionResult result2 = _combatSystem.UseStudentSkill(student2);
            Assert.IsTrue(result2.Success);

            // Assert - 전투 결과 확인
            Assert.IsTrue(_combatSystem.CurrentState == CombatState.InProgress ||
                         _combatSystem.CurrentState == CombatState.Victory);

            Assert.AreEqual(2, _combatSystem.CombatLog.TotalSkillsUsed);
            Assert.Greater(_combatSystem.CombatLog.TotalDamageDealt, 0);

            // 모든 학생이 살아있어야 함
            Assert.AreEqual(2, _combatSystem.GetAliveStudentCount());
        }
    }
}
