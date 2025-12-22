using NUnit.Framework;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 코스트 시스템 테스트 (테스트 체크포인트 #4 관련)
    /// </summary>
    public class CostSystemTests
    {
        private CostSystem _costSystem;

        [SetUp]
        public void Setup()
        {
            // 최대 코스트 10, 초당 1 회복, 시작 코스트 0
            _costSystem = new CostSystem(maxCost: 10, regenRate: 1f, startingCost: 0);
        }

        [Test]
        public void CostSystem_Initialization_ShouldStartWithZeroCost()
        {
            // Assert
            Assert.AreEqual(0, _costSystem.CurrentCost);
            Assert.AreEqual(10, _costSystem.MaxCost);
            Assert.AreEqual(1f, _costSystem.CostRegenRate);
            Assert.AreEqual(0, _costSystem.TotalCostGained);
            Assert.AreEqual(0, _costSystem.TotalCostSpent);
        }

        [Test]
        public void CostSystem_AddCost_ShouldIncreaseCost()
        {
            // Act
            _costSystem.AddCost(3);

            // Assert
            Assert.AreEqual(3, _costSystem.CurrentCost);
            Assert.AreEqual(3, _costSystem.TotalCostGained);
        }

        [Test]
        public void CostSystem_AddCost_ShouldNotExceedMaxCost()
        {
            // Act
            _costSystem.AddCost(15); // 최대치 초과 시도

            // Assert
            Assert.AreEqual(10, _costSystem.CurrentCost); // MaxCost로 제한됨
            Assert.AreEqual(10, _costSystem.TotalCostGained);
        }

        [Test]
        public void CostSystem_Update_ShouldRegenerateCost()
        {
            // Arrange
            _costSystem.SetCost(0);

            // Act - 3초 경과 (초당 1 회복)
            _costSystem.Update(1f);
            _costSystem.Update(1f);
            _costSystem.Update(1f);

            // Assert
            Assert.AreEqual(3, _costSystem.CurrentCost);
        }

        [Test]
        public void CostSystem_Update_ShouldNotExceedMaxCostDuringRegen()
        {
            // Arrange
            _costSystem.SetCost(8);

            // Act - 5초 경과 (최대치 초과)
            for (int i = 0; i < 5; i++)
            {
                _costSystem.Update(1f);
            }

            // Assert
            Assert.AreEqual(10, _costSystem.CurrentCost); // MaxCost로 제한됨
        }

        [Test]
        public void CostSystem_TrySpendCost_ShouldReturnTrueWhenEnoughCost()
        {
            // Arrange
            _costSystem.AddCost(5);

            // Act
            bool result = _costSystem.TrySpendCost(3);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, _costSystem.CurrentCost);
            Assert.AreEqual(3, _costSystem.TotalCostSpent);
            Assert.AreEqual(1, _costSystem.SkillUsageCount);
        }

        [Test]
        public void CostSystem_TrySpendCost_ShouldReturnFalseWhenNotEnoughCost()
        {
            // Arrange
            _costSystem.AddCost(2);

            // Act
            bool result = _costSystem.TrySpendCost(5);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(2, _costSystem.CurrentCost); // 코스트 변화 없음
            Assert.AreEqual(0, _costSystem.TotalCostSpent);
            Assert.AreEqual(0, _costSystem.SkillUsageCount);
        }

        [Test]
        public void CostSystem_HasEnoughCost_ShouldReturnCorrectValue()
        {
            // Arrange
            _costSystem.AddCost(5);

            // Assert
            Assert.IsTrue(_costSystem.HasEnoughCost(3));
            Assert.IsTrue(_costSystem.HasEnoughCost(5));
            Assert.IsFalse(_costSystem.HasEnoughCost(6));
        }

        [Test]
        public void CostSystem_FillCost_ShouldSetCostToMax()
        {
            // Arrange
            _costSystem.AddCost(3);

            // Act
            _costSystem.FillCost();

            // Assert
            Assert.AreEqual(10, _costSystem.CurrentCost);
        }

        [Test]
        public void CostSystem_Reset_ShouldClearAllStats()
        {
            // Arrange
            _costSystem.AddCost(5);
            _costSystem.TrySpendCost(3);

            // Act
            _costSystem.Reset();

            // Assert
            Assert.AreEqual(0, _costSystem.CurrentCost);
            Assert.AreEqual(0, _costSystem.TotalCostGained);
            Assert.AreEqual(0, _costSystem.TotalCostSpent);
            Assert.AreEqual(0, _costSystem.SkillUsageCount);
        }

        [Test]
        public void CostSystem_MultipleSkillUsage_ShouldTrackCorrectly()
        {
            // Arrange
            _costSystem.FillCost(); // 10 코스트

            // Act
            _costSystem.TrySpendCost(3); // 7 남음
            _costSystem.TrySpendCost(2); // 5 남음
            _costSystem.TrySpendCost(4); // 1 남음

            // Assert
            Assert.AreEqual(1, _costSystem.CurrentCost);
            Assert.AreEqual(9, _costSystem.TotalCostSpent);
            Assert.AreEqual(3, _costSystem.SkillUsageCount);
        }

        [Test]
        public void CostSystem_Update_WithFractionalTime_ShouldAccumulateCorrectly()
        {
            // Arrange
            _costSystem.SetCost(0);

            // Act - 0.5초씩 6번 = 3초 (3 코스트 회복)
            for (int i = 0; i < 6; i++)
            {
                _costSystem.Update(0.5f);
            }

            // Assert
            Assert.AreEqual(3, _costSystem.CurrentCost);
        }

        [Test]
        public void CostSystem_EventTrigger_OnCostChanged()
        {
            // Arrange
            int eventCallCount = 0;
            int lastCostValue = -1;

            _costSystem.OnCostChanged += (cost) =>
            {
                eventCallCount++;
                lastCostValue = cost;
            };

            // Act
            _costSystem.AddCost(5);
            _costSystem.TrySpendCost(2);

            // Assert
            Assert.AreEqual(2, eventCallCount);
            Assert.AreEqual(3, lastCostValue);
        }

        [Test]
        public void CostSystem_EventTrigger_OnCostSpent()
        {
            // Arrange
            int spentAmount = -1;
            int remainingCost = -1;

            _costSystem.OnCostSpent += (spent, remaining) =>
            {
                spentAmount = spent;
                remainingCost = remaining;
            };

            _costSystem.AddCost(10);

            // Act
            _costSystem.TrySpendCost(4);

            // Assert
            Assert.AreEqual(4, spentAmount);
            Assert.AreEqual(6, remainingCost);
        }
    }
}
