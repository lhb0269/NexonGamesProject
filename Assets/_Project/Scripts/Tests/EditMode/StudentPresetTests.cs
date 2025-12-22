using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Character;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 학생 프리셋 데이터 테스트
    /// </summary>
    public class StudentPresetTests
    {
        private List<StudentData> _studentPresets;

        [SetUp]
        public void Setup()
        {
            _studentPresets = new List<StudentData>();
        }

        [TearDown]
        public void TearDown()
        {
            StudentPresets.DestroyAllStudents(_studentPresets);
            _studentPresets.Clear();
        }

        [Test]
        public void StudentPreset_Shiroko_ShouldHaveCorrectData()
        {
            // Act
            StudentData shiroko = StudentPresets.CreateShiroko();
            _studentPresets.Add(shiroko);

            // Assert
            Assert.AreEqual("Shiroko", shiroko.studentName);
            Assert.AreEqual(1, shiroko.studentId);
            Assert.AreEqual(2431, shiroko.maxHP);
            Assert.AreEqual(478, shiroko.attack);
            Assert.AreEqual(16, shiroko.defense);
            Assert.IsNotNull(shiroko.exSkill);
            Assert.AreEqual("섬멸의 주사위", shiroko.exSkill.skillName);
            Assert.AreEqual(3, shiroko.exSkill.costAmount);
            Assert.AreEqual(SkillTargetType.Single, shiroko.exSkill.targetType);
        }

        [Test]
        public void StudentPreset_Hoshino_ShouldHaveCorrectData()
        {
            // Act
            StudentData hoshino = StudentPresets.CreateHoshino();
            _studentPresets.Add(hoshino);

            // Assert
            Assert.AreEqual("Hoshino", hoshino.studentName);
            Assert.AreEqual(2, hoshino.studentId);
            Assert.AreEqual(3528, hoshino.maxHP);
            Assert.AreEqual(382, hoshino.attack);
            Assert.AreEqual(28, hoshino.defense);
            Assert.IsNotNull(hoshino.exSkill);
            Assert.AreEqual("낙천적 방패", hoshino.exSkill.skillName);
            Assert.AreEqual(4, hoshino.exSkill.costAmount);
        }

        [Test]
        public void StudentPreset_Aru_ShouldHaveCorrectData()
        {
            // Act
            StudentData aru = StudentPresets.CreateAru();
            _studentPresets.Add(aru);

            // Assert
            Assert.AreEqual("Aru", aru.studentName);
            Assert.AreEqual(3, aru.studentId);
            Assert.AreEqual(2156, aru.maxHP);
            Assert.AreEqual(524, aru.attack);
            Assert.AreEqual(14, aru.defense);
            Assert.IsNotNull(aru.exSkill);
            Assert.AreEqual("더 퍼펙트 범죄", aru.exSkill.skillName);
            Assert.AreEqual(3, aru.exSkill.costAmount);
            Assert.AreEqual(SkillTargetType.Multiple, aru.exSkill.targetType);
        }

        [Test]
        public void StudentPreset_Haruna_ShouldHaveCorrectData()
        {
            // Act
            StudentData haruna = StudentPresets.CreateHaruna();
            _studentPresets.Add(haruna);

            // Assert
            Assert.AreEqual("Haruna", haruna.studentName);
            Assert.AreEqual(4, haruna.studentId);
            Assert.AreEqual(1989, haruna.maxHP);
            Assert.AreEqual(612, haruna.attack);
            Assert.AreEqual(12, haruna.defense);
            Assert.IsNotNull(haruna.exSkill);
            Assert.AreEqual("라스트 불릿", haruna.exSkill.skillName);
            Assert.AreEqual(5, haruna.exSkill.costAmount);
        }

        [Test]
        public void StudentPreset_CreateAllStudents_ShouldReturn4Students()
        {
            // Act
            List<StudentData> students = StudentPresets.CreateAllStudents();
            _studentPresets.AddRange(students);

            // Assert
            Assert.AreEqual(4, students.Count);
            Assert.AreEqual("Shiroko", students[0].studentName);
            Assert.AreEqual("Hoshino", students[1].studentName);
            Assert.AreEqual("Aru", students[2].studentName);
            Assert.AreEqual("Haruna", students[3].studentName);
        }

        [Test]
        public void StudentPreset_AllStudents_ShouldHaveUniqueIds()
        {
            // Arrange
            List<StudentData> students = StudentPresets.CreateAllStudents();
            _studentPresets.AddRange(students);

            // Act
            HashSet<int> ids = new HashSet<int>();
            foreach (var student in students)
            {
                ids.Add(student.studentId);
            }

            // Assert
            Assert.AreEqual(4, ids.Count); // 모든 ID가 고유해야 함
        }

        [Test]
        public void StudentPreset_AllStudents_ShouldHaveValidSkills()
        {
            // Arrange
            List<StudentData> students = StudentPresets.CreateAllStudents();
            _studentPresets.AddRange(students);

            // Assert
            foreach (var student in students)
            {
                Assert.IsNotNull(student.exSkill, $"{student.studentName}의 스킬이 null입니다");
                Assert.Greater(student.exSkill.costAmount, 0, $"{student.studentName}의 코스트가 0 이하입니다");
                Assert.Greater(student.exSkill.baseDamage, 0, $"{student.studentName}의 기본 데미지가 0 이하입니다");
            }
        }

        [Test]
        public void StudentPreset_CreateStudent_ShouldWorkInCombat()
        {
            // Arrange
            StudentData shirokoData = StudentPresets.CreateShiroko();
            _studentPresets.Add(shirokoData);

            // Act
            Student shiroko = new Student(shirokoData);

            // Assert
            Assert.AreEqual(2431, shiroko.CurrentHP);
            Assert.IsTrue(shiroko.IsAlive);
            Assert.IsTrue(shiroko.CanUseSkill());
            Assert.AreEqual(3, shiroko.GetSkillCost());
        }

        [Test]
        public void StudentPreset_Normal1_4Enemies_ShouldHaveCorrectCount()
        {
            // Act
            List<EnemyData> enemies = StudentPresets.CreateNormal1_4Enemies();

            // Assert
            Assert.AreEqual(3, enemies.Count);
        }

        [Test]
        public void StudentPreset_Normal1_4Enemies_ShouldHaveValidData()
        {
            // Act
            List<EnemyData> enemies = StudentPresets.CreateNormal1_4Enemies();

            // Assert
            foreach (var enemyData in enemies)
            {
                Assert.IsNotEmpty(enemyData.enemyName);
                Assert.Greater(enemyData.maxHP, 0);
                Assert.Greater(enemyData.attack, 0);
                Assert.GreaterOrEqual(enemyData.defense, 0);
            }
        }

        [Test]
        public void StudentPreset_GetStudentInfo_ShouldReturnValidString()
        {
            // Act
            string info = StudentPresetInfo.GetStudentInfo();

            // Assert
            Assert.IsNotEmpty(info);
            Assert.IsTrue(info.Contains("Shiroko"));
            Assert.IsTrue(info.Contains("Hoshino"));
            Assert.IsTrue(info.Contains("Aru"));
            Assert.IsTrue(info.Contains("Haruna"));
        }

        [Test]
        public void StudentPreset_TeamBalance_ShouldBeReasonable()
        {
            // Arrange
            List<StudentData> students = StudentPresets.CreateAllStudents();
            _studentPresets.AddRange(students);

            // Act - 전체 팀 능력치 합산
            int totalHP = 0;
            int totalATK = 0;
            int totalDEF = 0;

            foreach (var student in students)
            {
                totalHP += student.maxHP;
                totalATK += student.attack;
                totalDEF += student.defense;
            }

            int avgHP = totalHP / students.Count;
            int avgATK = totalATK / students.Count;
            int avgDEF = totalDEF / students.Count;

            // Assert - 평균 능력치가 합리적인 범위 내에 있는지
            Assert.Greater(avgHP, 1500, "평균 HP가 너무 낮습니다");
            Assert.Less(avgHP, 4000, "평균 HP가 너무 높습니다");
            Assert.Greater(avgATK, 300, "평균 ATK가 너무 낮습니다");
            Assert.Less(avgATK, 700, "평균 ATK가 너무 높습니다");

            Debug.Log($"팀 밸런스 - 평균 HP: {avgHP}, 평균 ATK: {avgATK}, 평균 DEF: {avgDEF}");
        }
    }
}
