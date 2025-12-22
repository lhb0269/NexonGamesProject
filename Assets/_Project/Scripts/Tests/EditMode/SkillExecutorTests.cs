using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Skill;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Data;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 스킬 실행 시스템 테스트 (테스트 체크포인트 #3 관련)
    /// </summary>
    public class SkillExecutorTests
    {
        private SkillExecutor _skillExecutor;
        private CostSystem _costSystem;
        private CombatLogSystem _combatLog;
        private StudentData _testStudentData;
        private SkillData _testSkillData;

        [SetUp]
        public void Setup()
        {
            _costSystem = new CostSystem(maxCost: 10, regenRate: 1f, startingCost: 5);
            _combatLog = new CombatLogSystem();
            _skillExecutor = new SkillExecutor(_costSystem, _combatLog);

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
        public void SkillExecutor_ExecuteSkill_ShouldSucceedWithValidConditions()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));
            List<Enemy> enemies = new List<Enemy> { enemy };

            _costSystem.FillCost();

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.Greater(result.TotalDamage, 0);
            Assert.AreEqual(1, result.TargetsHit.Count);
            Assert.AreEqual(3, result.CostSpent);
            Assert.AreEqual(1, student.SkillUsedCount);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldFailWhenSkillDataMissing()
        {
            // Arrange
            StudentData dataWithoutSkill = ScriptableObject.CreateInstance<StudentData>();
            dataWithoutSkill.studentName = "No Skill Student";
            dataWithoutSkill.exSkill = null;

            Student student = new Student(dataWithoutSkill);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("스킬 데이터 없음"));

            Object.DestroyImmediate(dataWithoutSkill);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldFailWhenOnCooldown()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));
            List<Enemy> enemies = new List<Enemy> { enemy };

            _costSystem.FillCost();
            _skillExecutor.ExecuteSkill(student, enemies); // 첫 번째 사용

            _costSystem.FillCost(); // 코스트 다시 충전

            // Act - 쿨다운 중 두 번째 사용 시도
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("쿨다운"));
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldFailWhenNotEnoughCost()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _costSystem.SetCost(2); // 필요한 코스트(3)보다 적음

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("코스트 부족"));
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldLogToFalseSystem()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _costSystem.FillCost();

            // Act
            _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Assert
            Assert.AreEqual(1, _combatLog.TotalSkillsUsed);
            Assert.Greater(_combatLog.TotalDamageDealt, 0);

            List<CombatLogEntry> skillLogs = _combatLog.GetLogsByType(CombatLogType.SkillUsed);
            Assert.AreEqual(1, skillLogs.Count);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_SingleTarget_ShouldHitOneEnemy()
        {
            // Arrange
            _testSkillData.targetType = SkillTargetType.Single;

            Student student = new Student(_testStudentData);
            List<Enemy> enemies = new List<Enemy>
            {
                new Enemy(new EnemyData("Enemy 1", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 2", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 3", 1000, 50, 20))
            };

            _costSystem.FillCost();

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.TargetsHit.Count);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_MultipleTarget_ShouldHitMultipleEnemies()
        {
            // Arrange
            _testSkillData.targetType = SkillTargetType.Multiple;

            Student student = new Student(_testStudentData);
            List<Enemy> enemies = new List<Enemy>
            {
                new Enemy(new EnemyData("Enemy 1", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 2", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 3", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 4", 1000, 50, 20))
            };

            _costSystem.FillCost();

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.TargetsHit.Count); // Multiple은 최대 3명
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_AreaTarget_ShouldHitAllEnemies()
        {
            // Arrange
            _testSkillData.targetType = SkillTargetType.Area;

            Student student = new Student(_testStudentData);
            List<Enemy> enemies = new List<Enemy>
            {
                new Enemy(new EnemyData("Enemy 1", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 2", 1000, 50, 20)),
                new Enemy(new EnemyData("Enemy 3", 1000, 50, 20))
            };

            _costSystem.FillCost();

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.TargetsHit.Count);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldSkipDeadEnemies()
        {
            // Arrange
            Student student = new Student(_testStudentData);

            Enemy aliveEnemy = new Enemy(new EnemyData("Alive Enemy", 1000, 50, 20));
            Enemy deadEnemy = new Enemy(new EnemyData("Dead Enemy", 1000, 50, 20));
            deadEnemy.TakeDamage(1000); // 격파

            List<Enemy> enemies = new List<Enemy> { deadEnemy, aliveEnemy };

            _costSystem.FillCost();

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.TargetsHit.Count);
            Assert.AreEqual("Alive Enemy", result.TargetsHit[0]);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldUpdateStudentStatistics()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _costSystem.FillCost();

            // Act
            _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Assert
            Assert.AreEqual(1, student.SkillUsedCount);
            Assert.Greater(student.TotalDamageDealt, 0);
        }

        [Test]
        public void SkillExecutor_ExecuteSkill_ShouldLogEnemyDefeat()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy weakEnemy = new Enemy(new EnemyData("Weak Enemy", 100, 10, 0));

            _costSystem.FillCost();

            // Act
            _skillExecutor.ExecuteSkill(student, new List<Enemy> { weakEnemy });

            // Assert
            Assert.IsFalse(weakEnemy.IsAlive);

            List<CombatLogEntry> defeatLogs = _combatLog.GetLogsByType(CombatLogType.UnitDefeated);
            Assert.AreEqual(1, defeatLogs.Count);
        }

        [Test]
        public void SkillExecutor_CanExecuteSkill_ShouldReturnCorrectValue()
        {
            // Arrange
            Student student = new Student(_testStudentData);

            // 조건 1: 코스트 충분, 쿨다운 완료
            _costSystem.SetCost(5);
            Assert.IsTrue(_skillExecutor.CanExecuteSkill(student));

            // 조건 2: 코스트 부족
            _costSystem.SetCost(2);
            Assert.IsFalse(_skillExecutor.CanExecuteSkill(student));

            // 조건 3: 쿨다운 중
            _costSystem.SetCost(5);
            student.UseSkill(); // 쿨다운 시작
            Assert.IsFalse(_skillExecutor.CanExecuteSkill(student));
        }

        [Test]
        public void SkillExecutor_MultipleSkillExecutions_ShouldTrackStatistics()
        {
            // Arrange
            Student student1 = new Student(_testStudentData);
            Student student2 = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 5000, 50, 20));

            _costSystem.FillCost();

            // Act
            _skillExecutor.ExecuteSkill(student1, new List<Enemy> { enemy });

            _costSystem.FillCost();
            _skillExecutor.ExecuteSkill(student2, new List<Enemy> { enemy });

            // Assert
            Assert.AreEqual(2, _skillExecutor.TotalSkillsExecuted);
            Assert.Greater(_skillExecutor.TotalDamageDealt, 0);
        }

        [Test]
        public void SkillExecutor_ResetStatistics_ShouldClearData()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _costSystem.FillCost();
            _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Act
            _skillExecutor.ResetStatistics();

            // Assert
            Assert.AreEqual(0, _skillExecutor.TotalSkillsExecuted);
            Assert.AreEqual(0, _skillExecutor.TotalDamageDealt);
        }

        [Test]
        public void SkillExecutor_GetStatistics_ShouldReturnValidString()
        {
            // Arrange
            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 50, 20));

            _costSystem.FillCost();
            _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Act
            string stats = _skillExecutor.GetStatistics();

            // Assert
            Assert.IsNotEmpty(stats);
            Assert.IsTrue(stats.Contains("스킬 실행"));
        }

        [Test]
        public void SkillExecutor_DamageCalculation_ShouldApplyMultiplier()
        {
            // Arrange
            _testSkillData.baseDamage = 100;
            _testSkillData.damageMultiplier = 2.0f;

            Student student = new Student(_testStudentData);
            Enemy enemy = new Enemy(new EnemyData("Test Enemy", 1000, 0, 0)); // 방어력 0

            _costSystem.FillCost();

            // Act
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, new List<Enemy> { enemy });

            // Assert
            // baseDamage(100) * multiplier(2.0) = 200
            // 방어력 0이므로 실제 데미지도 200
            Assert.AreEqual(200, result.TotalDamage);
        }
    }
}
