using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Reward;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Combat;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 보상 시스템 테스트 (테스트 체크포인트 #6 관련)
    /// </summary>
    public class RewardSystemTests
    {
        private RewardSystem _rewardSystem;
        private RewardValidator _rewardValidator;
        private StageData _testStageData;

        [SetUp]
        public void Setup()
        {
            _rewardSystem = new RewardSystem();
            _rewardValidator = new RewardValidator(_rewardSystem);

            // 테스트용 스테이지 데이터
            _testStageData = StagePresets.CreateNormal1_4();
        }

        [TearDown]
        public void TearDown()
        {
            StagePresets.DestroyStageData(_testStageData);
        }

        [Test]
        public void RewardSystem_Initialization_ShouldBeEmpty()
        {
            // Assert
            Assert.AreEqual(0, _rewardSystem.TotalRewardsGranted);
            Assert.AreEqual(0, _rewardSystem.TotalCurrencyGained);
            Assert.AreEqual(0, _rewardSystem.TotalMaterialsGained);
            Assert.AreEqual(0, _rewardSystem.TotalEquipmentsGained);
            Assert.AreEqual(0, _rewardSystem.TotalExpGained);
        }

        [Test]
        public void RewardSystem_GrantStageRewards_ShouldSucceedWhenVictory()
        {
            // Arrange
            CombatResult combatResult = new CombatResult
            {
                State = CombatState.Victory
            };

            // Act
            RewardGrantResult result = _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(4, result.TotalRewardCount); // Normal 1-4는 4개 보상
            Assert.AreEqual(4, result.GrantedRewards.Count);
        }

        [Test]
        public void RewardSystem_GrantStageRewards_ShouldFailWhenNotVictory()
        {
            // Arrange
            CombatResult combatResult = new CombatResult
            {
                State = CombatState.Defeat
            };

            // Act
            RewardGrantResult result = _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("승리가 아님"));
        }

        [Test]
        public void RewardSystem_GrantRewards_ShouldUpdateInventory()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };

            // Act
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.AreEqual(1000, _rewardSystem.GetInventoryCount(RewardItemType.Currency));
            Assert.AreEqual(5, _rewardSystem.GetInventoryCount(RewardItemType.Material));
            Assert.AreEqual(1, _rewardSystem.GetInventoryCount(RewardItemType.Equipment));
            Assert.AreEqual(150, _rewardSystem.GetInventoryCount(RewardItemType.Exp));
        }

        [Test]
        public void RewardSystem_GrantRewards_ShouldUpdateStatistics()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };

            // Act
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.AreEqual(4, _rewardSystem.TotalRewardsGranted);
            Assert.AreEqual(1000, _rewardSystem.TotalCurrencyGained);
            Assert.AreEqual(5, _rewardSystem.TotalMaterialsGained);
            Assert.AreEqual(1, _rewardSystem.TotalEquipmentsGained);
            Assert.AreEqual(150, _rewardSystem.TotalExpGained);
        }

        [Test]
        public void RewardSystem_MultipleGrants_ShouldAccumulate()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };

            // Act - 2번 클리어
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.AreEqual(8, _rewardSystem.TotalRewardsGranted); // 4 x 2
            Assert.AreEqual(2000, _rewardSystem.TotalCurrencyGained); // 1000 x 2
            Assert.AreEqual(10, _rewardSystem.TotalMaterialsGained); // 5 x 2
        }

        [Test]
        public void RewardSystem_GetInventory_ShouldReturnCorrectData()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Act
            var inventory = _rewardSystem.GetInventory();

            // Assert
            Assert.AreEqual(1000, inventory[RewardItemType.Currency]);
            Assert.AreEqual(5, inventory[RewardItemType.Material]);
            Assert.AreEqual(1, inventory[RewardItemType.Equipment]);
            Assert.AreEqual(150, inventory[RewardItemType.Exp]);
        }

        [Test]
        public void RewardSystem_Reset_ShouldClearAllData()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Act
            _rewardSystem.Reset();

            // Assert
            Assert.AreEqual(0, _rewardSystem.TotalRewardsGranted);
            Assert.AreEqual(0, _rewardSystem.GetInventoryCount(RewardItemType.Currency));
            Assert.AreEqual(0, _rewardSystem.GetInventoryCount(RewardItemType.Material));
        }

        [Test]
        public void RewardSystem_EventTrigger_OnRewardGranted()
        {
            // Arrange
            int eventCallCount = 0;
            _rewardSystem.OnRewardGranted += (reward) => eventCallCount++;

            CombatResult combatResult = new CombatResult { State = CombatState.Victory };

            // Act
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.AreEqual(4, eventCallCount); // 4개 보상
        }

        [Test]
        public void RewardSystem_EventTrigger_OnAllRewardsGranted()
        {
            // Arrange
            bool eventTriggered = false;
            List<RewardItemData> grantedRewards = null;

            _rewardSystem.OnAllRewardsGranted += (rewards) =>
            {
                eventTriggered = true;
                grantedRewards = rewards;
            };

            CombatResult combatResult = new CombatResult { State = CombatState.Victory };

            // Act
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Assert
            Assert.IsTrue(eventTriggered);
            Assert.IsNotNull(grantedRewards);
            Assert.AreEqual(4, grantedRewards.Count);
        }

        [Test]
        public void RewardValidator_ValidateConditions_ShouldSucceedWhenVictory()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };

            // Act
            RewardValidationResult result = _rewardValidator.ValidateRewardConditions(_testStageData, combatResult);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(4, result.ExpectedRewardCount);
        }

        [Test]
        public void RewardValidator_ValidateConditions_ShouldFailWhenDefeat()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Defeat };

            // Act
            RewardValidationResult result = _rewardValidator.ValidateRewardConditions(_testStageData, combatResult);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.FailureReason.Contains("승리 조건 미충족"));
        }

        [Test]
        public void RewardValidator_ValidateGrant_ShouldSucceedWhenCorrect()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };
            RewardGrantResult grantResult = _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Act
            RewardValidationResult validation = _rewardValidator.ValidateRewardGrant(_testStageData, grantResult);

            // Assert
            Assert.IsTrue(validation.IsValid);
            Assert.AreEqual(4, validation.ExpectedRewardCount);
            Assert.AreEqual(4, validation.ActualRewardCount);
        }

        [Test]
        public void RewardValidator_ValidateFullProcess_ShouldWork()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };
            RewardGrantResult grantResult = _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Act
            RewardValidationResult validation = _rewardValidator.ValidateFullRewardProcess(_testStageData, combatResult, grantResult);

            // Assert
            Assert.IsTrue(validation.IsValid);
            Assert.AreEqual(0, validation.ValidationErrors.Count);
        }

        [Test]
        public void RewardValidator_Statistics_ShouldTrack()
        {
            // Arrange
            CombatResult victory = new CombatResult { State = CombatState.Victory };
            CombatResult defeat = new CombatResult { State = CombatState.Defeat };

            // Act
            _rewardValidator.ValidateRewardConditions(_testStageData, victory);
            _rewardValidator.ValidateRewardConditions(_testStageData, defeat);

            // Assert
            Assert.AreEqual(2, _rewardValidator.TotalValidations);
            Assert.AreEqual(1, _rewardValidator.SuccessfulValidations);
            Assert.AreEqual(1, _rewardValidator.FailedValidations);
        }

        [Test]
        public void RewardSystem_GetStatistics_ShouldReturnValidString()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Act
            string stats = _rewardSystem.GetStatistics();

            // Assert
            Assert.IsNotEmpty(stats);
            Assert.IsTrue(stats.Contains("보상 통계"));
            Assert.IsTrue(stats.Contains("4개")); // 총 4개 보상
        }

        [Test]
        public void RewardSystem_GetInventoryStatus_ShouldReturnValidString()
        {
            // Arrange
            CombatResult combatResult = new CombatResult { State = CombatState.Victory };
            _rewardSystem.GrantStageRewards(_testStageData, combatResult);

            // Act
            string status = _rewardSystem.GetInventoryStatus();

            // Assert
            Assert.IsNotEmpty(status);
            Assert.IsTrue(status.Contains("인벤토리"));
        }
    }
}
