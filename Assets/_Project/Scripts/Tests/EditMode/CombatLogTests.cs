using NUnit.Framework;
using NexonGame.BlueArchive.Combat;
using System.Linq;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 전투 로그 시스템 테스트 (테스트 체크포인트 #5 관련)
    /// </summary>
    public class CombatLogTests
    {
        private CombatLogSystem _logSystem;

        [SetUp]
        public void Setup()
        {
            _logSystem = new CombatLogSystem();
        }

        [TearDown]
        public void TearDown()
        {
            _logSystem.Clear();
        }

        [Test]
        public void CombatLog_Initialization_ShouldBeEmpty()
        {
            // Assert
            Assert.AreEqual(0, _logSystem.LogCount);
            Assert.AreEqual(0, _logSystem.TotalDamageDealt);
            Assert.AreEqual(0, _logSystem.TotalDamageTaken);
            Assert.AreEqual(0, _logSystem.TotalSkillsUsed);
            Assert.AreEqual(0, _logSystem.TotalEnemiesDefeated);
            Assert.IsFalse(_logSystem.IsCombatActive());
        }

        [Test]
        public void CombatLog_LogCombatStart_ShouldSetActiveState()
        {
            // Act
            _logSystem.LogCombatStart("Normal 1-4");

            // Assert
            Assert.IsTrue(_logSystem.IsCombatActive());
            Assert.AreEqual(1, _logSystem.LogCount);
            Assert.AreEqual(CombatLogType.CombatStart, _logSystem.Logs[0].LogType);
        }

        [Test]
        public void CombatLog_LogCombatEnd_ShouldSetInactiveState()
        {
            // Arrange
            _logSystem.LogCombatStart("Normal 1-4");

            // Act
            _logSystem.LogCombatEnd(true);

            // Assert
            Assert.IsFalse(_logSystem.IsCombatActive());
            Assert.AreEqual(2, _logSystem.LogCount);
            Assert.AreEqual(CombatLogType.CombatEnd, _logSystem.Logs[1].LogType);
        }

        [Test]
        public void CombatLog_LogSkillUsed_ShouldIncrementSkillCount()
        {
            // Act
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogSkillUsed("Hoshino", "EX Skill", 4);

            // Assert
            Assert.AreEqual(2, _logSystem.TotalSkillsUsed);
            Assert.AreEqual(2, _logSystem.LogCount);

            var skillLogs = _logSystem.GetLogsByType(CombatLogType.SkillUsed);
            Assert.AreEqual(2, skillLogs.Count);
            Assert.AreEqual("Shiroko", skillLogs[0].ActorName);
            Assert.AreEqual(3, skillLogs[0].Value);
        }

        [Test]
        public void CombatLog_LogDamageDealt_ShouldAccumulateDamage()
        {
            // Act
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 500);
            _logSystem.LogDamageDealt("Shiroko", "Enemy2", 750);
            _logSystem.LogDamageDealt("Hoshino", "Enemy1", 300);

            // Assert
            Assert.AreEqual(1550, _logSystem.TotalDamageDealt);
            Assert.AreEqual(3, _logSystem.LogCount);

            var damageLogs = _logSystem.GetLogsByType(CombatLogType.DamageDealt);
            Assert.AreEqual(3, damageLogs.Count);
        }

        [Test]
        public void CombatLog_LogDamageTaken_ShouldAccumulateDamage()
        {
            // Act
            _logSystem.LogDamageTaken("Shiroko", 150, 850);
            _logSystem.LogDamageTaken("Hoshino", 200, 1800);

            // Assert
            Assert.AreEqual(350, _logSystem.TotalDamageTaken);
            Assert.AreEqual(2, _logSystem.LogCount);
        }

        [Test]
        public void CombatLog_LogUnitDefeated_ShouldIncrementDefeatCount()
        {
            // Act
            _logSystem.LogUnitDefeated("Enemy1", "Shiroko");
            _logSystem.LogUnitDefeated("Enemy2", "Hoshino");
            _logSystem.LogUnitDefeated("Enemy3", "Shiroko");

            // Assert
            Assert.AreEqual(3, _logSystem.TotalEnemiesDefeated);
            Assert.AreEqual(3, _logSystem.LogCount);

            var defeatLogs = _logSystem.GetLogsByType(CombatLogType.UnitDefeated);
            Assert.AreEqual(3, defeatLogs.Count);
        }

        [Test]
        public void CombatLog_LogCostSpent_ShouldAccumulateCost()
        {
            // Act
            _logSystem.LogCostSpent("Shiroko", 3, 7);
            _logSystem.LogCostSpent("Hoshino", 4, 6);
            _logSystem.LogCostSpent("Shiroko", 2, 8);

            // Assert
            Assert.AreEqual(9, _logSystem.TotalCostSpent);
            Assert.AreEqual(3, _logSystem.LogCount);
        }

        [Test]
        public void CombatLog_GetLogsByType_ShouldFilterCorrectly()
        {
            // Arrange
            _logSystem.LogCombatStart("Normal 1-4");
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 500);
            _logSystem.LogSkillUsed("Hoshino", "EX Skill", 4);
            _logSystem.LogCombatEnd(true);

            // Act
            var skillLogs = _logSystem.GetLogsByType(CombatLogType.SkillUsed);
            var combatLogs = _logSystem.GetLogsByType(CombatLogType.CombatStart);

            // Assert
            Assert.AreEqual(2, skillLogs.Count);
            Assert.AreEqual(1, combatLogs.Count);
            Assert.AreEqual(5, _logSystem.LogCount);
        }

        [Test]
        public void CombatLog_GetLogsByActor_ShouldFilterCorrectly()
        {
            // Arrange
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 500);
            _logSystem.LogDamageDealt("Hoshino", "Enemy2", 300);
            _logSystem.LogSkillUsed("Shiroko", "Normal Attack", 0);

            // Act
            var shirokoLogs = _logSystem.GetLogsByActor("Shiroko");
            var hoshinoLogs = _logSystem.GetLogsByActor("Hoshino");

            // Assert
            Assert.AreEqual(3, shirokoLogs.Count);
            Assert.AreEqual(1, hoshinoLogs.Count);
        }

        [Test]
        public void CombatLog_Clear_ShouldResetAllData()
        {
            // Arrange
            _logSystem.LogCombatStart("Normal 1-4");
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 500);

            // Act
            _logSystem.Clear();

            // Assert
            Assert.AreEqual(0, _logSystem.LogCount);
            Assert.AreEqual(0, _logSystem.TotalDamageDealt);
            Assert.AreEqual(0, _logSystem.TotalSkillsUsed);
            Assert.IsFalse(_logSystem.IsCombatActive());
        }

        [Test]
        public void CombatLog_FullCombatScenario_ShouldTrackAllEvents()
        {
            // Arrange & Act - 전체 전투 시나리오
            _logSystem.LogCombatStart("Normal 1-4");

            // 첫 번째 스킬 사용
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 750);
            _logSystem.LogDamageTaken("Enemy1", 750, 250);

            // 두 번째 스킬 사용
            _logSystem.LogSkillUsed("Hoshino", "EX Skill", 4);
            _logSystem.LogDamageDealt("Hoshino", "Enemy1", 300);
            _logSystem.LogUnitDefeated("Enemy1", "Hoshino");

            // 아군 피격
            _logSystem.LogDamageTaken("Shiroko", 100, 900);

            _logSystem.LogCombatEnd(true);

            // Assert
            Assert.AreEqual(9, _logSystem.LogCount);
            Assert.AreEqual(1050, _logSystem.TotalDamageDealt);
            Assert.AreEqual(850, _logSystem.TotalDamageTaken);
            Assert.AreEqual(2, _logSystem.TotalSkillsUsed);
            Assert.AreEqual(1, _logSystem.TotalEnemiesDefeated);
            Assert.AreEqual(7, _logSystem.TotalCostSpent); // 3 + 4
        }

        [Test]
        public void CombatLog_GetCombatSummary_ShouldReturnValidString()
        {
            // Arrange
            _logSystem.LogCombatStart("Normal 1-4");
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 500);

            // Act
            string summary = _logSystem.GetCombatSummary();

            // Assert
            Assert.IsNotEmpty(summary);
            Assert.IsTrue(summary.Contains("총 로그 수"));
            Assert.IsTrue(summary.Contains("총 데미지"));
        }

        [Test]
        public void CombatLog_GetFullLog_ShouldReturnAllLogs()
        {
            // Arrange
            _logSystem.LogCombatStart("Normal 1-4");
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);

            // Act
            string fullLog = _logSystem.GetFullLog();

            // Assert
            Assert.IsNotEmpty(fullLog);
            Assert.IsTrue(fullLog.Contains("전투 로그"));
        }

        [Test]
        public void CombatLog_EventTrigger_OnLogAdded()
        {
            // Arrange
            int eventCallCount = 0;
            CombatLogEntry lastEntry = null;

            _logSystem.OnLogAdded += (entry) =>
            {
                eventCallCount++;
                lastEntry = entry;
            };

            // Act
            _logSystem.LogSkillUsed("Shiroko", "EX Skill", 3);
            _logSystem.LogDamageDealt("Shiroko", "Enemy1", 500);

            // Assert
            Assert.AreEqual(2, eventCallCount);
            Assert.IsNotNull(lastEntry);
            Assert.AreEqual(CombatLogType.DamageDealt, lastEntry.LogType);
        }
    }
}
