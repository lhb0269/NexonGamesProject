using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Stage;
using NexonGame.BlueArchive.Data;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 전투 진입 검증 테스트 (테스트 체크포인트 #2 관련)
    /// </summary>
    public class CombatEntryTests
    {
        private StageController _stageController;
        private CombatEntryValidator _entryValidator;
        private StageData _testStageData;

        [SetUp]
        public void Setup()
        {
            _stageController = new StageController();

            // 테스트용 스테이지 데이터 생성
            _testStageData = ScriptableObject.CreateInstance<StageData>();
            _testStageData.stageName = "Test Stage";
            _testStageData.stageId = 999;
            _testStageData.gridWidth = 5;
            _testStageData.gridHeight = 3;
            _testStageData.startPosition = new Vector2Int(0, 1);
            _testStageData.battlePosition = new Vector2Int(4, 1);
            _testStageData.platformPositions = new List<Vector2Int>
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1)
            };

            _entryValidator = new CombatEntryValidator(_stageController);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testStageData);
        }

        [Test]
        public void CombatEntry_ValidateEntry_ShouldFailWhenStageNotInitialized()
        {
            // Act
            CombatEntryResult result = _entryValidator.ValidateEntry();

            // Assert
            Assert.IsFalse(result.CanEnterCombat);
            Assert.IsTrue(result.FailureReason.Contains("초기화되지 않음"));
            Assert.AreEqual(1, _entryValidator.TotalEntryAttempts);
            Assert.AreEqual(1, _entryValidator.FailedEntries);
        }

        [Test]
        public void CombatEntry_ValidateEntry_ShouldFailWhenNotAtBattlePosition()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);
            // 플레이어는 시작 위치에 있음

            // Act
            CombatEntryResult result = _entryValidator.ValidateEntry();

            // Assert
            Assert.IsFalse(result.CanEnterCombat);
            Assert.AreEqual(1, _entryValidator.FailedEntries);
        }

        [Test]
        public void CombatEntry_ValidateEntry_ShouldSucceedWhenAtBattlePosition()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);

            // 전투 위치까지 이동
            _stageController.MovePlayer(new Vector2Int(1, 1));
            _stageController.MovePlayer(new Vector2Int(2, 1));
            _stageController.MovePlayer(new Vector2Int(3, 1));
            _stageController.MovePlayer(new Vector2Int(4, 1));

            // Act
            CombatEntryResult result = _entryValidator.ValidateEntry();

            // Assert
            Assert.IsTrue(result.CanEnterCombat);
            Assert.AreEqual(1, _entryValidator.SuccessfulEntries);
        }

        [Test]
        public void CombatEntry_TryEnterCombat_ShouldSucceedWhenReady()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);

            // 전투 위치까지 이동
            _stageController.MovePlayer(new Vector2Int(1, 1));
            _stageController.MovePlayer(new Vector2Int(2, 1));
            _stageController.MovePlayer(new Vector2Int(3, 1));
            _stageController.MovePlayer(new Vector2Int(4, 1));

            // Act
            bool entered = _entryValidator.TryEnterCombat();

            // Assert
            Assert.IsTrue(entered);
            Assert.AreEqual(StageState.InBattle, _stageController.CurrentState);
        }

        [Test]
        public void CombatEntry_TryEnterCombat_ShouldFailWhenNotReady()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);
            // 전투 위치에 도달하지 않음

            // Act
            bool entered = _entryValidator.TryEnterCombat();

            // Assert
            Assert.IsFalse(entered);
            Assert.AreNotEqual(StageState.InBattle, _stageController.CurrentState);
        }

        [Test]
        public void CombatEntry_MultipleAttempts_ShouldTrackStatistics()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);

            // Act
            _entryValidator.ValidateEntry(); // 실패 (위치 X)
            _stageController.MovePlayer(new Vector2Int(1, 1));
            _entryValidator.ValidateEntry(); // 실패 (위치 X)
            _stageController.MovePlayer(new Vector2Int(2, 1));
            _stageController.MovePlayer(new Vector2Int(3, 1));
            _stageController.MovePlayer(new Vector2Int(4, 1));
            _entryValidator.ValidateEntry(); // 성공

            // Assert
            Assert.AreEqual(3, _entryValidator.TotalEntryAttempts);
            Assert.AreEqual(1, _entryValidator.SuccessfulEntries);
            Assert.AreEqual(2, _entryValidator.FailedEntries);
        }

        [Test]
        public void CombatEntry_GetEntryRequirementsChecklist_ShouldReturnValidString()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);

            // Act
            string checklist = _entryValidator.GetEntryRequirementsChecklist();

            // Assert
            Assert.IsNotEmpty(checklist);
            Assert.IsTrue(checklist.Contains("전투 진입 요구사항"));
        }

        [Test]
        public void CombatEntry_GetStatistics_ShouldReturnCorrectInfo()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);
            _entryValidator.ValidateEntry();
            _entryValidator.ValidateEntry();

            // Act
            string stats = _entryValidator.GetStatistics();

            // Assert
            Assert.IsNotEmpty(stats);
            Assert.IsTrue(stats.Contains("총 시도 횟수: 2"));
        }

        [Test]
        public void CombatEntry_ResetStatistics_ShouldClearCounts()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);
            _entryValidator.ValidateEntry();
            _entryValidator.ValidateEntry();

            // Act
            _entryValidator.ResetStatistics();

            // Assert
            Assert.AreEqual(0, _entryValidator.TotalEntryAttempts);
            Assert.AreEqual(0, _entryValidator.SuccessfulEntries);
            Assert.AreEqual(0, _entryValidator.FailedEntries);
        }

        [Test]
        public void CombatEntry_StateTransitionFlow_ShouldWorkCorrectly()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);
            List<StageState> stateHistory = new List<StageState>();

            _stageController.OnStateChanged += (oldState, newState) =>
            {
                stateHistory.Add(newState);
            };

            // Act - 전체 흐름
            // 1. 전투 위치로 이동 시도 (실패)
            CombatEntryResult result1 = _entryValidator.ValidateEntry();

            // 2. 전투 위치까지 이동
            _stageController.MovePlayer(new Vector2Int(1, 1));
            _stageController.MovePlayer(new Vector2Int(2, 1));
            _stageController.MovePlayer(new Vector2Int(3, 1));
            _stageController.MovePlayer(new Vector2Int(4, 1));

            // 3. 전투 진입 시도 (성공)
            CombatEntryResult result2 = _entryValidator.ValidateEntry();
            bool entered = _entryValidator.TryEnterCombat();

            // Assert
            Assert.IsFalse(result1.CanEnterCombat);
            Assert.IsTrue(result2.CanEnterCombat);
            Assert.IsTrue(entered);

            Assert.Contains(StageState.MovingToBattle, stateHistory);
            Assert.Contains(StageState.ReadyForBattle, stateHistory);
            Assert.Contains(StageState.InBattle, stateHistory);

            Assert.AreEqual(StageState.InBattle, _stageController.CurrentState);
        }

        [Test]
        public void CombatEntry_ValidateEntry_ShouldFailWhenInWrongState()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);

            // 전투 위치까지 이동하고 전투 시작
            _stageController.MovePlayer(new Vector2Int(1, 1));
            _stageController.MovePlayer(new Vector2Int(2, 1));
            _stageController.MovePlayer(new Vector2Int(3, 1));
            _stageController.MovePlayer(new Vector2Int(4, 1));
            _stageController.StartBattle();

            // Act - 이미 전투 중인 상태에서 진입 시도
            CombatEntryResult result = _entryValidator.ValidateEntry();

            // Assert
            Assert.IsFalse(result.CanEnterCombat);
            Assert.IsTrue(result.FailureReason.Contains("준비 상태가 아님"));
        }

        [Test]
        public void CombatEntry_FullCombatFlow_WithValidation()
        {
            // Arrange
            _stageController.InitializeStage(_testStageData);

            // Act & Assert - 단계별 검증
            // 1. 초기 상태: 전투 진입 불가
            Assert.IsFalse(_entryValidator.ValidateEntry().CanEnterCombat);

            // 2. 플랫폼 이동 (체크포인트 #1)
            Assert.IsTrue(_stageController.MovePlayer(new Vector2Int(1, 1)));
            Assert.IsTrue(_stageController.MovePlayer(new Vector2Int(2, 1)));
            Assert.IsTrue(_stageController.MovePlayer(new Vector2Int(3, 1)));

            // 3. 전투 위치 도달
            Assert.IsTrue(_stageController.MovePlayer(new Vector2Int(4, 1)));
            Assert.AreEqual(StageState.ReadyForBattle, _stageController.CurrentState);

            // 4. 전투 진입 검증 성공 (체크포인트 #2)
            Assert.IsTrue(_entryValidator.ValidateEntry().CanEnterCombat);

            // 5. 전투 시작
            Assert.IsTrue(_entryValidator.TryEnterCombat());
            Assert.AreEqual(StageState.InBattle, _stageController.CurrentState);

            // 통계 확인
            Assert.AreEqual(2, _entryValidator.TotalEntryAttempts);
            Assert.AreEqual(1, _entryValidator.SuccessfulEntries);
        }
    }
}
