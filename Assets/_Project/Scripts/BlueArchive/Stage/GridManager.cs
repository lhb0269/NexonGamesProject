using System.Collections.Generic;
using UnityEngine;

namespace NexonGame.BlueArchive.Stage
{
    /// <summary>
    /// 그리드 셀 타입
    /// </summary>
    public enum GridCellType
    {
        Empty,      // 빈 공간 (이동 불가)
        Platform,   // 플랫폼 (이동 가능)
        Start,      // 시작 위치
        Battle      // 전투 위치
    }

    /// <summary>
    /// 그리드 셀 정보
    /// </summary>
    public class GridCell
    {
        public Vector2Int Position { get; private set; }
        public GridCellType CellType { get; set; }
        public bool IsOccupied { get; set; }

        public GridCell(Vector2Int position, GridCellType cellType = GridCellType.Empty)
        {
            Position = position;
            CellType = cellType;
            IsOccupied = false;
        }

        public bool IsWalkable()
        {
            return (CellType == GridCellType.Platform ||
                    CellType == GridCellType.Start ||
                    CellType == GridCellType.Battle) &&
                   !IsOccupied;
        }
    }

    /// <summary>
    /// 그리드 관리 시스템
    /// - 2D 그리드 기반 맵 관리
    /// - 플랫폼 위치 설정 및 이동 가능 영역 판정
    /// </summary>
    public class GridManager
    {
        private GridCell[,] _grid;
        private int _width;
        private int _height;

        // 통계
        public int TotalMoveCount { get; private set; }
        public List<Vector2Int> MoveHistory { get; private set; }

        public int Width => _width;
        public int Height => _height;

        public GridManager(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new GridCell[width, height];
            MoveHistory = new List<Vector2Int>();
            TotalMoveCount = 0;

            // 모든 셀을 Empty로 초기화
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = new GridCell(new Vector2Int(x, y), GridCellType.Empty);
                }
            }

            Debug.Log($"[GridManager] 그리드 생성: {width}x{height}");
        }

        /// <summary>
        /// 특정 위치에 플랫폼 설정
        /// </summary>
        public void SetPlatform(Vector2Int position, GridCellType cellType = GridCellType.Platform)
        {
            if (!IsValidPosition(position))
            {
                Debug.LogWarning($"[GridManager] 유효하지 않은 위치: {position}");
                return;
            }

            _grid[position.x, position.y].CellType = cellType;
            Debug.Log($"[GridManager] 플랫폼 설정: {position} ({cellType})");
        }

        /// <summary>
        /// 여러 위치에 플랫폼 설정
        /// </summary>
        public void SetPlatforms(List<Vector2Int> positions, GridCellType cellType = GridCellType.Platform)
        {
            foreach (var pos in positions)
            {
                SetPlatform(pos, cellType);
            }
        }

        /// <summary>
        /// 특정 위치의 셀 가져오기
        /// </summary>
        public GridCell GetCell(Vector2Int position)
        {
            if (!IsValidPosition(position))
                return null;

            return _grid[position.x, position.y];
        }

        /// <summary>
        /// 위치가 그리드 내에 있는지 확인
        /// </summary>
        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.y >= 0 && position.y < _height;
        }

        /// <summary>
        /// 위치가 이동 가능한지 확인
        /// </summary>
        public bool IsWalkable(Vector2Int position)
        {
            if (!IsValidPosition(position))
                return false;

            return _grid[position.x, position.y].IsWalkable();
        }

        /// <summary>
        /// 두 위치가 인접한지 확인 (상하좌우)
        /// </summary>
        public bool IsAdjacent(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(from.x - to.x);
            int dy = Mathf.Abs(from.y - to.y);

            // 상하좌우만 인접으로 간주 (대각선 제외)
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// 인접한 이동 가능한 셀 목록 반환
        /// </summary>
        public List<Vector2Int> GetAdjacentWalkableCells(Vector2Int position)
        {
            List<Vector2Int> adjacent = new List<Vector2Int>();

            // 상하좌우 체크
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 위
                new Vector2Int(0, -1),  // 아래
                new Vector2Int(-1, 0),  // 왼쪽
                new Vector2Int(1, 0)    // 오른쪽
            };

            foreach (var dir in directions)
            {
                Vector2Int newPos = position + dir;
                if (IsWalkable(newPos))
                {
                    adjacent.Add(newPos);
                }
            }

            return adjacent;
        }

        /// <summary>
        /// 두 위치 사이의 맨하탄 거리 계산
        /// </summary>
        public int GetManhattanDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// 특정 타입의 모든 셀 위치 반환
        /// </summary>
        public List<Vector2Int> GetCellsByType(GridCellType cellType)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y].CellType == cellType)
                    {
                        cells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return cells;
        }

        /// <summary>
        /// 셀 점유 상태 설정
        /// </summary>
        public void SetOccupied(Vector2Int position, bool occupied)
        {
            if (!IsValidPosition(position))
                return;

            _grid[position.x, position.y].IsOccupied = occupied;
        }

        /// <summary>
        /// 이동 기록 추가
        /// </summary>
        public void RecordMove(Vector2Int position)
        {
            MoveHistory.Add(position);
            TotalMoveCount++;
            Debug.Log($"[GridManager] 이동 기록: {position} (총 {TotalMoveCount}회)");
        }

        /// <summary>
        /// 통계 초기화
        /// </summary>
        public void ResetStatistics()
        {
            MoveHistory.Clear();
            TotalMoveCount = 0;
            Debug.Log("[GridManager] 이동 통계 초기화");
        }

        /// <summary>
        /// 그리드 전체 초기화
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y].CellType = GridCellType.Empty;
                    _grid[x, y].IsOccupied = false;
                }
            }

            ResetStatistics();
            Debug.Log("[GridManager] 그리드 초기화");
        }

        /// <summary>
        /// 그리드 시각화 (디버그용)
        /// </summary>
        public string VisualizeGrid()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Grid Layout ===");

            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    GridCell cell = _grid[x, y];
                    char symbol = cell.CellType switch
                    {
                        GridCellType.Empty => '.',
                        GridCellType.Platform => '#',
                        GridCellType.Start => 'S',
                        GridCellType.Battle => 'B',
                        _ => '?'
                    };

                    if (cell.IsOccupied)
                        symbol = 'X';

                    sb.Append(symbol);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
