using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Stage;
using NexonGame.BlueArchive.Data;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 스테이지 컨트롤러 테스트 (테스트 체크포인트 #1, #2 관련)
    /// </summary>
    public class StageControllerTests
    {
        private StageController _controller;
        private StageData _testStageData;

        [SetUp]
        public void Setup()
        {
            _controller = new StageController();

            // 테스트용 스테이지 데이터 생성
            _testStageData = ScriptableObject.CreateInstance<StageData>();
            _testStageData.stageName = "Test Stage";
            _testStageData.stageId = 999;
            _testStageData.gridWidth = 5;
            _testStageData.gridHeight = 3;
            _testStageData.startPosition = new Vector2Int(0, 1);
            _testStageData.battlePosition = new Vector2Int(4, 1);

            // 플랫폼 위치 설정 (일직선)
            _testStageData.platformPositions = new List<Vector2Int>
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1)
            };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testStageData);
        }

        [Test]
        public void StageController_InitializeStage_ShouldSetCorrectState()
        {
            // Act
            _controller.InitializeStage(_testStageData);

            // Assert
            Assert.AreEqual(StageState.MovingToBattle, _controller.CurrentState);
            Assert.AreEqual(new Vector2Int(0, 1), _controller.PlayerPosition);
            Assert.AreEqual(new Vector2Int(4, 1), _controller.BattlePosition);
            Assert.IsNotNull(_controller.Grid);
        }

        [Test]
        public void StageController_MovePlayer_ShouldMoveToAdjacentCell()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            Vector2Int targetPos = new Vector2Int(1, 1);

            // Act
            bool moved = _controller.MovePlayer(targetPos);

            // Assert
            Assert.IsTrue(moved);
            Assert.AreEqual(targetPos, _controller.PlayerPosition);
            Assert.AreEqual(1, _controller.TotalMovesInStage);
        }

        [Test]
        public void StageController_MovePlayer_ShouldFailForNonAdjacentCell()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            Vector2Int targetPos = new Vector2Int(3, 1); // 2칸 떨어진 위치

            // Act
            bool moved = _controller.MovePlayer(targetPos);

            // Assert
            Assert.IsFalse(moved);
            Assert.AreEqual(new Vector2Int(0, 1), _controller.PlayerPosition); // 이동하지 않음
            Assert.AreEqual(0, _controller.TotalMovesInStage);
        }

        [Test]
        public void StageController_MovePlayer_ShouldFailForEmptyCell()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            Vector2Int emptyPos = new Vector2Int(0, 0); // Empty cell

            // Act
            bool moved = _controller.MovePlayer(emptyPos);

            // Assert
            Assert.IsFalse(moved);
            Assert.AreEqual(new Vector2Int(0, 1), _controller.PlayerPosition);
        }

        [Test]
        public void StageController_MovePlayerToBattlePosition_ShouldChangeState()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            bool stateChanged = false;

            _controller.OnStateChanged += (oldState, newState) =>
            {
                if (newState == StageState.ReadyForBattle)
                    stateChanged = true;
            };

            // Act - 전투 위치까지 이동
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1)); // 전투 위치 도달

            // Assert
            Assert.IsTrue(stateChanged);
            Assert.AreEqual(StageState.ReadyForBattle, _controller.CurrentState);
            Assert.AreEqual(_controller.BattlePosition, _controller.PlayerPosition);
        }

        [Test]
        public void StageController_OnBattleReached_ShouldTriggerEvent()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            bool eventTriggered = false;

            _controller.OnBattleReached += () => eventTriggered = true;

            // Act - 전투 위치까지 이동
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1));

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void StageController_StartBattle_ShouldChangeToInBattleState()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);

            // 전투 위치까지 이동
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1));

            // Act
            _controller.StartBattle();

            // Assert
            Assert.AreEqual(StageState.InBattle, _controller.CurrentState);
        }

        [Test]
        public void StageController_StartBattle_ShouldFailIfNotReady()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            // 전투 위치에 도달하지 않은 상태

            // Act
            _controller.StartBattle();

            // Assert
            Assert.AreNotEqual(StageState.InBattle, _controller.CurrentState);
        }

        [Test]
        public void StageController_CompleteBattle_ShouldChangeState()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);

            // 전투 시작까지
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1));
            _controller.StartBattle();

            // Act
            _controller.CompleteBattle(victory: true);

            // Assert
            Assert.AreEqual(StageState.BattleComplete, _controller.CurrentState);
        }

        [Test]
        public void StageController_ClearStage_ShouldChangeToStageCleared()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);

            // 전투 완료까지
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1));
            _controller.StartBattle();
            _controller.CompleteBattle(victory: true);

            // Act
            _controller.ClearStage();

            // Assert
            Assert.AreEqual(StageState.StageCleared, _controller.CurrentState);
        }

        [Test]
        public void StageController_OnStageCleared_ShouldTriggerEvent()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);
            bool eventTriggered = false;

            _controller.OnStageCleared += () => eventTriggered = true;

            // 전투 완료까지
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1));
            _controller.StartBattle();
            _controller.CompleteBattle(victory: true);

            // Act
            _controller.ClearStage();

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void StageController_GetPathToBattle_ShouldReturnValidPath()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);

            // Act
            List<Vector2Int> path = _controller.GetPathToBattle();

            // Assert
            Assert.IsNotEmpty(path);
            Assert.AreEqual(new Vector2Int(4, 1), path[path.Count - 1]); // 마지막이 전투 위치
        }

        [Test]
        public void StageController_FullStageFlow_ShouldWorkCorrectly()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);

            // Track state changes
            List<StageState> stateChanges = new List<StageState>();
            _controller.OnStateChanged += (oldState, newState) => stateChanges.Add(newState);

            // Act - 전체 스테이지 진행
            // 1. 이동
            _controller.MovePlayer(new Vector2Int(1, 1));
            _controller.MovePlayer(new Vector2Int(2, 1));
            _controller.MovePlayer(new Vector2Int(3, 1));
            _controller.MovePlayer(new Vector2Int(4, 1));

            // 2. 전투 시작
            _controller.StartBattle();

            // 3. 전투 완료
            _controller.CompleteBattle(victory: true);

            // 4. 스테이지 클리어
            _controller.ClearStage();

            // Assert
            Assert.AreEqual(StageState.StageCleared, _controller.CurrentState);
            Assert.Contains(StageState.MovingToBattle, stateChanges);
            Assert.Contains(StageState.ReadyForBattle, stateChanges);
            Assert.Contains(StageState.InBattle, stateChanges);
            Assert.Contains(StageState.BattleComplete, stateChanges);
            Assert.Contains(StageState.StageCleared, stateChanges);
            Assert.AreEqual(4, _controller.TotalMovesInStage);
        }

        [Test]
        public void StageController_GetStageInfo_ShouldReturnValidString()
        {
            // Arrange
            _controller.InitializeStage(_testStageData);

            // Act
            string info = _controller.GetStageInfo();

            // Assert
            Assert.IsNotEmpty(info);
            Assert.IsTrue(info.Contains("Test Stage"));
            Assert.IsTrue(info.Contains("MovingToBattle"));
        }
    }
}
