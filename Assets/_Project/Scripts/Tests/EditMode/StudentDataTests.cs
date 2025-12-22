using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Character;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 학생 데이터 및 기본 기능 테스트
    /// </summary>
    public class StudentDataTests
    {
        private StudentData _testStudentData;
        private SkillData _testSkillData;

        [SetUp]
        public void Setup()
        {
            // 테스트용 스킬 데이터 생성
            _testSkillData = ScriptableObject.CreateInstance<SkillData>();
            _testSkillData.skillName = "테스트 EX 스킬";
            _testSkillData.skillId = 1;
            _testSkillData.costAmount = 3;
            _testSkillData.baseDamage = 500;
            _testSkillData.damageMultiplier = 1.5f;
            _testSkillData.cooldownTime = 20f;

            // 테스트용 학생 데이터 생성
            _testStudentData = ScriptableObject.CreateInstance<StudentData>();
            _testStudentData.studentName = "테스트 학생";
            _testStudentData.studentId = 1;
            _testStudentData.maxHP = 1000;
            _testStudentData.attack = 100;
            _testStudentData.defense = 50;
            _testStudentData.exSkill = _testSkillData;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testStudentData);
            Object.DestroyImmediate(_testSkillData);
        }

        [Test]
        public void Student_Creation_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var student = new Student(_testStudentData);

            // Assert
            Assert.AreEqual(_testStudentData, student.Data);
            Assert.AreEqual(1000, student.CurrentHP);
            Assert.IsTrue(student.IsAlive);
            Assert.IsTrue(student.IsSkillReady);
            Assert.AreEqual(0, student.TotalDamageDealt);
            Assert.AreEqual(0, student.TotalDamageTaken);
        }

        [Test]
        public void Student_TakeDamage_ShouldReduceHP()
        {
            // Arrange
            var student = new Student(_testStudentData);

            // Act
            int actualDamage = student.TakeDamage(200);

            // Assert
            // 200 - 50(방어력) = 150 실제 데미지
            Assert.AreEqual(150, actualDamage);
            Assert.AreEqual(850, student.CurrentHP);
            Assert.AreEqual(150, student.TotalDamageTaken);
        }

        [Test]
        public void Student_TakeDamage_ShouldNotGoBelowZero()
        {
            // Arrange
            var student = new Student(_testStudentData);

            // Act
            student.TakeDamage(2000);

            // Assert
            Assert.AreEqual(0, student.CurrentHP);
            Assert.IsFalse(student.IsAlive);
        }

        [Test]
        public void Student_UseSkill_ShouldSucceedWhenReady()
        {
            // Arrange
            var student = new Student(_testStudentData);

            // Act
            bool skillUsed = student.UseSkill();

            // Assert
            Assert.IsTrue(skillUsed);
            Assert.AreEqual(1, student.SkillUsedCount);
            Assert.IsFalse(student.IsSkillReady);
            Assert.AreEqual(20f, student.SkillCooldownRemaining);
        }

        [Test]
        public void Student_UseSkill_ShouldFailWhenOnCooldown()
        {
            // Arrange
            var student = new Student(_testStudentData);
            student.UseSkill();  // 첫 번째 사용

            // Act
            bool secondSkillUsed = student.UseSkill();  // 쿨다운 중 재사용 시도

            // Assert
            Assert.IsFalse(secondSkillUsed);
            Assert.AreEqual(1, student.SkillUsedCount);  // 여전히 1회만 사용
        }

        [Test]
        public void Student_UpdateCooldown_ShouldReduceCooldownTime()
        {
            // Arrange
            var student = new Student(_testStudentData);
            student.UseSkill();

            // Act
            student.UpdateCooldown(5f);

            // Assert
            Assert.AreEqual(15f, student.SkillCooldownRemaining);
            Assert.IsFalse(student.IsSkillReady);
        }

        [Test]
        public void Student_UpdateCooldown_ShouldBecomeReadyAfterCooldown()
        {
            // Arrange
            var student = new Student(_testStudentData);
            student.UseSkill();

            // Act
            student.UpdateCooldown(20f);

            // Assert
            Assert.AreEqual(0f, student.SkillCooldownRemaining);
            Assert.IsTrue(student.IsSkillReady);
        }

        [Test]
        public void Student_RecordDamage_ShouldAccumulateDamage()
        {
            // Arrange
            var student = new Student(_testStudentData);

            // Act
            student.RecordDamage(100);
            student.RecordDamage(200);
            student.RecordDamage(150);

            // Assert
            Assert.AreEqual(450, student.TotalDamageDealt);
        }

        [Test]
        public void Student_Heal_ShouldIncreaseHP()
        {
            // Arrange
            var student = new Student(_testStudentData);
            student.TakeDamage(500);

            // Act
            student.Heal(200);

            // Assert
            Assert.AreEqual(650, student.CurrentHP);  // 850 + 200 = 1050, but capped at maxHP
        }

        [Test]
        public void Student_Heal_ShouldNotExceedMaxHP()
        {
            // Arrange
            var student = new Student(_testStudentData);
            student.TakeDamage(100);

            // Act
            student.Heal(500);

            // Assert
            Assert.AreEqual(1000, student.CurrentHP);  // Should cap at maxHP
        }

        [Test]
        public void Student_GetSkillCost_ShouldReturnCorrectCost()
        {
            // Arrange
            var student = new Student(_testStudentData);

            // Act
            int cost = student.GetSkillCost();

            // Assert
            Assert.AreEqual(3, cost);  // _testSkillData.costAmount = 3
        }

        [Test]
        public void Student_CanUseSkill_ShouldReturnTrueWhenReady()
        {
            // Arrange
            var student = new Student(_testStudentData);

            // Assert
            Assert.IsTrue(student.CanUseSkill());
        }

        [Test]
        public void Student_CanUseSkill_ShouldReturnFalseWhenOnCooldown()
        {
            // Arrange
            var student = new Student(_testStudentData);
            student.UseSkill();

            // Assert
            Assert.IsFalse(student.CanUseSkill());
        }
    }
}
