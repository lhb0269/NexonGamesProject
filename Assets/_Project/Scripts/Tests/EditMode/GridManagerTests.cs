using NUnit.Framework;
using UnityEngine;
using NexonGame.BlueArchive.Stage;
using System.Collections.Generic;

namespace NexonGame.Tests.EditMode
{
    /// <summary>
    /// 그리드 관리자 테스트 (테스트 체크포인트 #1 관련)
    /// </summary>
    public class GridManagerTests
    {
        private GridManager _gridManager;

        [SetUp]
        public void Setup()
        {
            _gridManager = new GridManager(10, 5);
        }

        [Test]
        public void GridManager_Initialization_ShouldCreateCorrectSize()
        {
            // Assert
            Assert.AreEqual(10, _gridManager.Width);
            Assert.AreEqual(5, _gridManager.Height);
            Assert.AreEqual(0, _gridManager.TotalMoveCount);
        }

        [Test]
        public void GridManager_SetPlatform_ShouldChangeCellType()
        {
            // Arrange
            Vector2Int pos = new Vector2Int(3, 2);

            // Act
            _gridManager.SetPlatform(pos, GridCellType.Platform);

            // Assert
            GridCell cell = _gridManager.GetCell(pos);
            Assert.IsNotNull(cell);
            Assert.AreEqual(GridCellType.Platform, cell.CellType);
        }

        [Test]
        public void GridManager_IsValidPosition_ShouldReturnTrueForValidPositions()
        {
            // Assert
            Assert.IsTrue(_gridManager.IsValidPosition(new Vector2Int(0, 0)));
            Assert.IsTrue(_gridManager.IsValidPosition(new Vector2Int(9, 4)));
            Assert.IsTrue(_gridManager.IsValidPosition(new Vector2Int(5, 2)));
        }

        [Test]
        public void GridManager_IsValidPosition_ShouldReturnFalseForInvalidPositions()
        {
            // Assert
            Assert.IsFalse(_gridManager.IsValidPosition(new Vector2Int(-1, 0)));
            Assert.IsFalse(_gridManager.IsValidPosition(new Vector2Int(0, -1)));
            Assert.IsFalse(_gridManager.IsValidPosition(new Vector2Int(10, 0)));
            Assert.IsFalse(_gridManager.IsValidPosition(new Vector2Int(0, 5)));
        }

        [Test]
        public void GridManager_IsWalkable_ShouldReturnTrueForPlatforms()
        {
            // Arrange
            Vector2Int pos = new Vector2Int(3, 2);
            _gridManager.SetPlatform(pos, GridCellType.Platform);

            // Assert
            Assert.IsTrue(_gridManager.IsWalkable(pos));
        }

        [Test]
        public void GridManager_IsWalkable_ShouldReturnFalseForEmptyCells()
        {
            // Arrange
            Vector2Int pos = new Vector2Int(3, 2);
            // Empty cell by default

            // Assert
            Assert.IsFalse(_gridManager.IsWalkable(pos));
        }

        [Test]
        public void GridManager_IsWalkable_ShouldReturnFalseForOccupiedCells()
        {
            // Arrange
            Vector2Int pos = new Vector2Int(3, 2);
            _gridManager.SetPlatform(pos, GridCellType.Platform);
            _gridManager.SetOccupied(pos, true);

            // Assert
            Assert.IsFalse(_gridManager.IsWalkable(pos));
        }

        [Test]
        public void GridManager_IsAdjacent_ShouldReturnTrueForAdjacentCells()
        {
            // Arrange
            Vector2Int center = new Vector2Int(5, 2);

            // Assert - 상하좌우
            Assert.IsTrue(_gridManager.IsAdjacent(center, new Vector2Int(5, 3))); // 위
            Assert.IsTrue(_gridManager.IsAdjacent(center, new Vector2Int(5, 1))); // 아래
            Assert.IsTrue(_gridManager.IsAdjacent(center, new Vector2Int(4, 2))); // 왼쪽
            Assert.IsTrue(_gridManager.IsAdjacent(center, new Vector2Int(6, 2))); // 오른쪽
        }

        [Test]
        public void GridManager_IsAdjacent_ShouldReturnFalseForDiagonalCells()
        {
            // Arrange
            Vector2Int center = new Vector2Int(5, 2);

            // Assert - 대각선
            Assert.IsFalse(_gridManager.IsAdjacent(center, new Vector2Int(4, 3)));
            Assert.IsFalse(_gridManager.IsAdjacent(center, new Vector2Int(6, 3)));
            Assert.IsFalse(_gridManager.IsAdjacent(center, new Vector2Int(4, 1)));
            Assert.IsFalse(_gridManager.IsAdjacent(center, new Vector2Int(6, 1)));
        }

        [Test]
        public void GridManager_GetAdjacentWalkableCells_ShouldReturnCorrectCells()
        {
            // Arrange
            Vector2Int center = new Vector2Int(5, 2);
            _gridManager.SetPlatform(new Vector2Int(5, 3), GridCellType.Platform);
            _gridManager.SetPlatform(new Vector2Int(4, 2), GridCellType.Platform);

            // Act
            List<Vector2Int> adjacent = _gridManager.GetAdjacentWalkableCells(center);

            // Assert
            Assert.AreEqual(2, adjacent.Count);
            Assert.Contains(new Vector2Int(5, 3), adjacent);
            Assert.Contains(new Vector2Int(4, 2), adjacent);
        }

        [Test]
        public void GridManager_GetManhattanDistance_ShouldCalculateCorrectly()
        {
            // Arrange
            Vector2Int from = new Vector2Int(0, 0);
            Vector2Int to = new Vector2Int(3, 4);

            // Act
            int distance = _gridManager.GetManhattanDistance(from, to);

            // Assert
            Assert.AreEqual(7, distance); // 3 + 4 = 7
        }

        [Test]
        public void GridManager_GetCellsByType_ShouldReturnCorrectCells()
        {
            // Arrange
            _gridManager.SetPlatform(new Vector2Int(1, 1), GridCellType.Platform);
            _gridManager.SetPlatform(new Vector2Int(2, 2), GridCellType.Platform);
            _gridManager.SetPlatform(new Vector2Int(3, 3), GridCellType.Start);

            // Act
            List<Vector2Int> platforms = _gridManager.GetCellsByType(GridCellType.Platform);
            List<Vector2Int> starts = _gridManager.GetCellsByType(GridCellType.Start);

            // Assert
            Assert.AreEqual(2, platforms.Count);
            Assert.AreEqual(1, starts.Count);
        }

        [Test]
        public void GridManager_RecordMove_ShouldIncrementCount()
        {
            // Act
            _gridManager.RecordMove(new Vector2Int(1, 1));
            _gridManager.RecordMove(new Vector2Int(2, 1));
            _gridManager.RecordMove(new Vector2Int(3, 1));

            // Assert
            Assert.AreEqual(3, _gridManager.TotalMoveCount);
            Assert.AreEqual(3, _gridManager.MoveHistory.Count);
        }

        [Test]
        public void GridManager_ResetStatistics_ShouldClearMoveHistory()
        {
            // Arrange
            _gridManager.RecordMove(new Vector2Int(1, 1));
            _gridManager.RecordMove(new Vector2Int(2, 1));

            // Act
            _gridManager.ResetStatistics();

            // Assert
            Assert.AreEqual(0, _gridManager.TotalMoveCount);
            Assert.AreEqual(0, _gridManager.MoveHistory.Count);
        }

        [Test]
        public void GridManager_SetPlatforms_ShouldSetMultiplePlatforms()
        {
            // Arrange
            List<Vector2Int> platforms = new List<Vector2Int>
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1)
            };

            // Act
            _gridManager.SetPlatforms(platforms);

            // Assert
            foreach (var pos in platforms)
            {
                GridCell cell = _gridManager.GetCell(pos);
                Assert.AreEqual(GridCellType.Platform, cell.CellType);
            }
        }

        [Test]
        public void GridManager_Clear_ShouldResetAllCells()
        {
            // Arrange
            _gridManager.SetPlatform(new Vector2Int(1, 1), GridCellType.Platform);
            _gridManager.SetPlatform(new Vector2Int(2, 2), GridCellType.Start);
            _gridManager.RecordMove(new Vector2Int(1, 1));

            // Act
            _gridManager.Clear();

            // Assert
            GridCell cell1 = _gridManager.GetCell(new Vector2Int(1, 1));
            GridCell cell2 = _gridManager.GetCell(new Vector2Int(2, 2));

            Assert.AreEqual(GridCellType.Empty, cell1.CellType);
            Assert.AreEqual(GridCellType.Empty, cell2.CellType);
            Assert.AreEqual(0, _gridManager.TotalMoveCount);
        }
    }
}
