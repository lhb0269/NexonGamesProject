# 블루 아카이브 테스트 자동화 - 테스트 실행 가이드

## 목차
1. [Unity Test Framework 설정](#unity-test-framework-설정)
2. [테스트 실행 방법](#테스트-실행-방법)
3. [작성된 테스트 목록](#작성된-테스트-목록)
4. [테스트 체크포인트 확인](#테스트-체크포인트-확인)
5. [문제 해결](#문제-해결)

---

## Unity Test Framework 설정

### 1. Package Manager에서 Test Framework 확인

1. Unity 에디터 상단 메뉴: **Window > Package Manager**
2. 왼쪽 상단 드롭다운: **Unity Registry** 선택
3. 검색창에 "Test Framework" 입력
4. **Test Framework** 패키지 확인
   - 이미 설치되어 있어야 함 (Unity 2020 이상은 기본 포함)
   - 미설치 시 **Install** 버튼 클릭

### 2. Test Runner 창 열기

**Window > General > Test Runner**

두 개의 탭이 표시됩니다:
- **EditMode**: 에디터 모드 테스트 (Play 모드 없이 실행)
- **PlayMode**: 플레이 모드 테스트 (게임 실행 중 테스트)

---

## 테스트 실행 방법

### EditMode 테스트 실행 (권장)

현재 모든 테스트는 EditMode로 작성되어 있습니다.

#### 전체 테스트 실행

1. **Test Runner** 창 열기
2. **EditMode** 탭 선택
3. 상단의 **Run All** 버튼 클릭
4. 결과 확인:
   - ✅ 녹색 체크: 성공
   - ❌ 빨간 X: 실패
   - 실행 시간 및 통과/실패 개수 표시

#### 특정 테스트 파일만 실행

1. Test Runner에서 테스트 파일 트리 확장
2. 실행하려는 테스트 파일 또는 클래스 선택
3. 우클릭 > **Run Selected** 또는 선택 후 **Run Selected** 버튼 클릭

**예시:**
```
NexonGame.Tests.EditMode
├── StudentDataTests (14 tests)
├── CostSystemTests (17 tests)
├── CombatLogTests (16 tests)
├── GridManagerTests (20 tests)
├── StageControllerTests (18 tests)
└── CombatEntryTests (14 tests)
```

#### 개별 테스트 실행

1. 테스트 클래스를 더블클릭하여 펼치기
2. 실행하려는 개별 테스트 선택
3. 우클릭 > **Run Selected**

---

## 작성된 테스트 목록

### Day 1: 학생 데이터 시스템 (14 tests)

**파일:** `Assets/_Project/Scripts/Tests/EditMode/StudentDataTests.cs`

```
✅ Student_Creation_ShouldInitializeCorrectly
✅ Student_TakeDamage_ShouldReduceHP
✅ Student_TakeDamage_ShouldNotGoBelowZero
✅ Student_UseSkill_ShouldSucceedWhenReady
✅ Student_UseSkill_ShouldFailWhenOnCooldown
✅ Student_UpdateCooldown_ShouldReduceCooldownTime
✅ Student_UpdateCooldown_ShouldBecomeReadyAfterCooldown
✅ Student_RecordDamage_ShouldAccumulateDamage
✅ Student_Heal_ShouldIncreaseHP
✅ Student_Heal_ShouldNotExceedMaxHP
✅ Student_GetSkillCost_ShouldReturnCorrectCost
✅ Student_CanUseSkill_ShouldReturnTrueWhenReady
✅ Student_CanUseSkill_ShouldReturnFalseWhenOnCooldown
```

**테스트 내용:**
- 학생 생성 및 초기화
- 데미지 처리 및 HP 관리
- 스킬 사용 및 쿨다운
- 데미지 통계 추적
- 힐링 시스템

### Day 2: 코스트 시스템 (17 tests)

**파일:** `Assets/_Project/Scripts/Tests/EditMode/CostSystemTests.cs`

```
✅ CostSystem_Initialization_ShouldStartWithZeroCost
✅ CostSystem_AddCost_ShouldIncreaseCost
✅ CostSystem_AddCost_ShouldNotExceedMaxCost
✅ CostSystem_Update_ShouldRegenerateCost
✅ CostSystem_Update_ShouldNotExceedMaxCostDuringRegen
✅ CostSystem_TrySpendCost_ShouldReturnTrueWhenEnoughCost
✅ CostSystem_TrySpendCost_ShouldReturnFalseWhenNotEnoughCost
✅ CostSystem_HasEnoughCost_ShouldReturnCorrectValue
✅ CostSystem_FillCost_ShouldSetCostToMax
✅ CostSystem_Reset_ShouldClearAllStats
✅ CostSystem_MultipleSkillUsage_ShouldTrackCorrectly
✅ CostSystem_Update_WithFractionalTime_ShouldAccumulateCorrectly
✅ CostSystem_EventTrigger_OnCostChanged
✅ CostSystem_EventTrigger_OnCostSpent
```

**테스트 내용:**
- 코스트 초기화 및 증가
- 시간에 따른 자동 회복
- 코스트 소모 및 검증
- 이벤트 시스템

**관련 체크포인트:** ✅ #4 코스트 소모 검증

### Day 2: 전투 로그 시스템 (16 tests)

**파일:** `Assets/_Project/Scripts/Tests/EditMode/CombatLogTests.cs`

```
✅ CombatLog_Initialization_ShouldBeEmpty
✅ CombatLog_LogCombatStart_ShouldSetActiveState
✅ CombatLog_LogCombatEnd_ShouldSetInactiveState
✅ CombatLog_LogSkillUsed_ShouldIncrementSkillCount
✅ CombatLog_LogDamageDealt_ShouldAccumulateDamage
✅ CombatLog_LogDamageTaken_ShouldAccumulateDamage
✅ CombatLog_LogUnitDefeated_ShouldIncrementDefeatCount
✅ CombatLog_LogCostSpent_ShouldAccumulateCost
✅ CombatLog_GetLogsByType_ShouldFilterCorrectly
✅ CombatLog_GetLogsByActor_ShouldFilterCorrectly
✅ CombatLog_Clear_ShouldResetAllData
✅ CombatLog_FullCombatScenario_ShouldTrackAllEvents
✅ CombatLog_GetCombatSummary_ShouldReturnValidString
✅ CombatLog_GetFullLog_ShouldReturnAllLogs
✅ CombatLog_EventTrigger_OnLogAdded
```

**테스트 내용:**
- 전투 로그 기록 (시작, 종료, 스킬, 데미지, 격파)
- 로그 필터링 및 검색
- 전투 통계 집계
- 이벤트 시스템

**관련 체크포인트:** ✅ #5 전투별 데미지 추적

### Day 3-4: 그리드 관리 시스템 (20 tests)

**파일:** `Assets/_Project/Scripts/Tests/EditMode/GridManagerTests.cs`

```
✅ GridManager_Initialization_ShouldCreateCorrectSize
✅ GridManager_SetPlatform_ShouldChangeCellType
✅ GridManager_IsValidPosition_ShouldReturnTrueForValidPositions
✅ GridManager_IsValidPosition_ShouldReturnFalseForInvalidPositions
✅ GridManager_IsWalkable_ShouldReturnTrueForPlatforms
✅ GridManager_IsWalkable_ShouldReturnFalseForEmptyCells
✅ GridManager_IsWalkable_ShouldReturnFalseForOccupiedCells
✅ GridManager_IsAdjacent_ShouldReturnTrueForAdjacentCells
✅ GridManager_IsAdjacent_ShouldReturnFalseForDiagonalCells
✅ GridManager_GetAdjacentWalkableCells_ShouldReturnCorrectCells
✅ GridManager_GetManhattanDistance_ShouldCalculateCorrectly
✅ GridManager_GetCellsByType_ShouldReturnCorrectCells
✅ GridManager_RecordMove_ShouldIncrementCount
✅ GridManager_ResetStatistics_ShouldClearMoveHistory
✅ GridManager_SetPlatforms_ShouldSetMultiplePlatforms
✅ GridManager_Clear_ShouldResetAllCells
```

**테스트 내용:**
- 그리드 생성 및 초기화
- 플랫폼 설정
- 이동 가능 영역 판정
- 인접 셀 확인
- 이동 기록 추적

**관련 체크포인트:** ✅ #1 플랫폼 이동 검증 (일부)

### Day 3-4: 스테이지 컨트롤러 (18 tests)

**파일:** `Assets/_Project/Scripts/Tests/EditMode/StageControllerTests.cs`

```
✅ StageController_InitializeStage_ShouldSetCorrectState
✅ StageController_MovePlayer_ShouldMoveToAdjacentCell
✅ StageController_MovePlayer_ShouldFailForNonAdjacentCell
✅ StageController_MovePlayer_ShouldFailForEmptyCell
✅ StageController_MovePlayerToBattlePosition_ShouldChangeState
✅ StageController_OnBattleReached_ShouldTriggerEvent
✅ StageController_StartBattle_ShouldChangeToInBattleState
✅ StageController_StartBattle_ShouldFailIfNotReady
✅ StageController_CompleteBattle_ShouldChangeState
✅ StageController_ClearStage_ShouldChangeToStageCleared
✅ StageController_OnStageCleared_ShouldTriggerEvent
✅ StageController_GetPathToBattle_ShouldReturnValidPath
✅ StageController_FullStageFlow_ShouldWorkCorrectly
✅ StageController_GetStageInfo_ShouldReturnValidString
```

**테스트 내용:**
- 스테이지 초기화
- 플레이어 이동 (성공/실패)
- 상태 전환
- 전투 시작/완료
- 전체 스테이지 플로우

**관련 체크포인트:** ✅ #1 플랫폼 이동 검증 (일부), ✅ #2 전투 진입 검증 (일부)

### Day 3-4: 전투 진입 검증 (14 tests)

**파일:** `Assets/_Project/Scripts/Tests/EditMode/CombatEntryTests.cs`

```
✅ CombatEntry_ValidateEntry_ShouldFailWhenStageNotInitialized
✅ CombatEntry_ValidateEntry_ShouldFailWhenNotAtBattlePosition
✅ CombatEntry_ValidateEntry_ShouldSucceedWhenAtBattlePosition
✅ CombatEntry_TryEnterCombat_ShouldSucceedWhenReady
✅ CombatEntry_TryEnterCombat_ShouldFailWhenNotReady
✅ CombatEntry_MultipleAttempts_ShouldTrackStatistics
✅ CombatEntry_GetEntryRequirementsChecklist_ShouldReturnValidString
✅ CombatEntry_GetStatistics_ShouldReturnCorrectInfo
✅ CombatEntry_ResetStatistics_ShouldClearCounts
✅ CombatEntry_StateTransitionFlow_ShouldWorkCorrectly
✅ CombatEntry_ValidateEntry_ShouldFailWhenInWrongState
✅ CombatEntry_FullCombatFlow_WithValidation
```

**테스트 내용:**
- 전투 진입 조건 검증
- 진입 시도 성공/실패
- 통계 추적
- 상태 전환 플로우

**관련 체크포인트:** ✅ #2 전투 진입 검증

---

## 테스트 체크포인트 확인

### 체크포인트 #1: 플랫폼 이동 검증 ✅

**관련 테스트:**
- `GridManagerTests` (20개)
- `StageControllerTests` (18개)

**확인 방법:**
1. Test Runner > EditMode
2. `GridManagerTests` 전체 실행 → 모두 통과 확인
3. `StageControllerTests` 전체 실행 → 모두 통과 확인
4. 특히 확인할 테스트:
   - `GridManager_IsWalkable_*` (이동 가능 영역)
   - `StageController_MovePlayer_*` (플레이어 이동)
   - `StageController_FullStageFlow_*` (전체 플로우)

### 체크포인트 #2: 전투 진입 검증 ✅

**관련 테스트:**
- `StageControllerTests` (상태 전환 관련)
- `CombatEntryTests` (14개)

**확인 방법:**
1. Test Runner > EditMode
2. `CombatEntryTests` 전체 실행 → 모두 통과 확인
3. 특히 확인할 테스트:
   - `CombatEntry_ValidateEntry_*` (진입 조건 검증)
   - `CombatEntry_FullCombatFlow_WithValidation` (전체 플로우)

### 체크포인트 #3: EX 스킬 사용 로깅 🔲

**상태:** 미구현 (Day 5-6 예정)

### 체크포인트 #4: 코스트 소모 검증 ✅

**관련 테스트:**
- `CostSystemTests` (17개)
- `StudentDataTests` (코스트 관련 3개)

**확인 방법:**
1. Test Runner > EditMode
2. `CostSystemTests` 전체 실행 → 모두 통과 확인
3. 특히 확인할 테스트:
   - `CostSystem_TrySpendCost_*` (코스트 소모)
   - `CostSystem_MultipleSkillUsage_*` (여러 스킬 사용)

### 체크포인트 #5: 전투별 데미지 추적 ✅

**관련 테스트:**
- `CombatLogTests` (16개)
- `StudentDataTests` (데미지 관련)

**확인 방법:**
1. Test Runner > EditMode
2. `CombatLogTests` 전체 실행 → 모두 통과 확인
3. 특히 확인할 테스트:
   - `CombatLog_LogDamageDealt_*` (데미지 기록)
   - `CombatLog_FullCombatScenario_*` (전체 전투 시나리오)

### 체크포인트 #6: 보상 획득 검증 🔲

**상태:** 미구현 (Day 7 예정)

---

## 문제 해결

### 컴파일 에러 발생 시

#### 1. InputSystem_Actions 관련 에러

**에러 메시지:**
```
error CS0246: The type or namespace name 'InputSystem_Actions' could not be found
```

**해결 방법:**
1. Unity 에디터에서 `Assets/_Project/Settings/InputSystem_Actions.inputactions` 파일 선택
2. Inspector 창에서 **Generate C# Class** 체크박스 활성화
3. **Apply** 버튼 클릭
4. 스크립트 재컴파일 대기

**임시 우회:**
- 현재는 legacy Input 시스템으로 임시 구현되어 있어 테스트 실행에는 문제없음

#### 2. Assembly Definition 관련 에러

**증상:** 테스트가 Test Runner에 나타나지 않음

**해결 방법:**
1. `Assets/_Project/Scripts/Tests/EditMode` 폴더 확인
2. `NexonGame.Tests.EditMode.asmdef` 파일이 존재하는지 확인
3. 파일 내용 확인:
   ```json
   {
       "name": "NexonGame.Tests.EditMode",
       "references": [
           "NexonGame.Runtime",
           "UnityEngine.TestRunner",
           "UnityEditor.TestRunner"
       ],
       "includePlatforms": ["Editor"],
       "precompiledReferences": ["nunit.framework.dll"],
       "defineConstraints": ["UNITY_INCLUDE_TESTS"]
   }
   ```
4. Unity 에디터 재시작

#### 3. Test Runner에 테스트가 표시되지 않음

**해결 방법:**
1. Test Runner 창에서 **Refresh** 버튼 클릭 (우측 상단 새로고침 아이콘)
2. Unity 에디터 **재시작**
3. `Assets > Reimport All` 실행

### 테스트 실패 시

#### 1. ScriptableObject 관련 실패

**증상:** `StudentDataTests`, `StageControllerTests` 등에서 NullReferenceException

**원인:** Unity의 ScriptableObject는 에디터 모드에서만 생성 가능

**확인:**
- EditMode 테스트로 작성되어 있는지 확인
- `[SetUp]`에서 `ScriptableObject.CreateInstance<>()` 사용
- `[TearDown]`에서 `Object.DestroyImmediate()` 사용

#### 2. Time.time 관련 실패

**증상:** 시간 관련 테스트 실패

**해결:**
- EditMode 테스트에서는 `Time.time`이 항상 0일 수 있음
- 상대적인 시간 차이만 테스트하거나
- 실제 시간값보다는 로직 검증에 집중

### 성능 최적화

#### 테스트 실행 시간 단축

1. **병렬 실행 활성화:**
   - Edit > Preferences > Test Runner
   - "Run tests in parallel" 체크

2. **특정 테스트만 실행:**
   - 전체 실행 대신 수정한 부분 관련 테스트만 실행

3. **테스트 격리:**
   - 각 테스트는 독립적으로 실행 가능해야 함
   - `[SetUp]`과 `[TearDown]` 제대로 구현

---

## 테스트 결과 리포트

### Console 로그 확인

테스트 실행 중 Console 창에서 로그 확인 가능:
```
[GridManager] 그리드 생성: 10x5
[StageController] 스테이지 초기화: Test Stage
[CombatLogSystem] 전투 시작: Normal 1-4
```

### 테스트 커버리지

현재 작성된 테스트:
- **총 테스트 수:** 99개
- **EditMode:** 99개
- **PlayMode:** 0개 (추후 통합 테스트 시 추가)

### 다음 단계

Day 5-6 작업 후 추가될 테스트:
- 학생 프리셋 테스트
- 스킬 시스템 테스트
- 전투 시스템 테스트
- EX 스킬 로깅 테스트 (체크포인트 #3)

---

## 빠른 참조

### 필수 확인 사항

```bash
# 1. 모든 테스트 통과 확인
Test Runner > EditMode > Run All → 99/99 Passed

# 2. 체크포인트별 확인
✅ #1 플랫폼 이동: GridManagerTests + StageControllerTests
✅ #2 전투 진입: CombatEntryTests
✅ #4 코스트 소모: CostSystemTests
✅ #5 데미지 추적: CombatLogTests
```

### 자주 사용하는 단축키

- **Ctrl + Shift + T**: Test Runner 창 열기 (설정 필요)
- **Ctrl + R, T**: 테스트 재실행 (Visual Studio)
- **F5**: 선택한 테스트 실행 (Test Runner 포커스 시)

---

**작성일:** 2025-12-22
**버전:** Day 3-4 완료 시점
**다음 업데이트:** Day 5-6 완료 후
