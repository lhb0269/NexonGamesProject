# PlayMode 테스트 전환 및 실제 게임 구현 계획서

## 📋 개요

### 목적
- EditMode 테스트에서 PlayMode 테스트로 전환
- 실제 게임 오브젝트 및 UI 구현
- 시각적으로 테스트 진행 과정 확인 가능
- 자동화 실행 파일에서 테스트 실행 가능

### 현재 상태
- ✅ EditMode에서 6개 체크포인트 모두 구현 (178개 테스트)
- ✅ 로직 시스템 완성 (Stage, Combat, Skill, Cost, Reward)
- ✅ 데이터 모델 완성 (StudentData, EnemyData, StageData)
- ❌ 게임 오브젝트 미구현 (프리팹, 씬 설정 없음)
- ❌ UI 미구현 (테스트 진행 상황 표시 없음)

---

## 🎯 구현 목표

### 필수 구현 사항
1. **PlayMode 테스트 환경**
   - PlayMode에서 실행되는 통합 테스트
   - 게임 오브젝트 기반 테스트

2. **게임 오브젝트 시스템**
   - Student GameObject (학생 캐릭터)
   - Enemy GameObject (적 유닛)
   - Grid/Platform GameObject (발판 시스템)
   - Stage GameObject (스테이지 관리자)

3. **UI 시스템**
   - 테스트 진행 상황 표시 UI
   - 전투 로그 표시 UI
   - 코스트 게이지 표시 UI
   - 학생 상태 표시 UI
   - 보상 결과 표시 UI

4. **씬 구성**
   - TestScene (PlayMode 테스트용 씬)
   - Normal 1-4 스테이지 레이아웃

---

## 📐 시스템 아키텍처

### 현재 아키텍처 (EditMode)
```
[EditMode Tests]
    ↓
[Pure C# Logic Classes]
    - StageController (순수 로직)
    - CombatSystem (순수 로직)
    - CostSystem (순수 로직)
    - RewardSystem (순수 로직)
```

### 목표 아키텍처 (PlayMode)
```
[PlayMode Tests]
    ↓
[MonoBehaviour Controllers] ← 새로 구현
    - StageManager (MonoBehaviour)
    - CombatManager (MonoBehaviour)
    - UIManager (MonoBehaviour)
    ↓
[Pure C# Logic Classes] ← 기존 유지
    - StageController
    - CombatSystem
    - CostSystem
    - RewardSystem
    ↓
[GameObjects & UI] ← 새로 구현
    - StudentObject
    - EnemyObject
    - GridObject
    - UI Panels
```

### 설계 원칙
- **기존 로직 클래스는 건드리지 않음** (EditMode 테스트 유지)
- **MonoBehaviour 래퍼 클래스** 추가로 게임 오브젝트 연결
- **UI는 독립적으로** 구현 (로직과 분리)
- **테스트 자동화** 가능하도록 설계

---

## 🏗️ 구현 단계

### Phase 1: 게임 오브젝트 기반 구조 (Day 9-10)

#### 1.1 MonoBehaviour 매니저 클래스
**목적**: 기존 로직 클래스를 게임 오브젝트와 연결

**구현 파일**:
- `Assets/_Project/Scripts/BlueArchive/Stage/StageManager.cs` (MonoBehaviour)
  - 기존 `StageController` 래핑
  - 그리드 비주얼 생성/관리
  - 플랫폼 오브젝트 생성

- `Assets/_Project/Scripts/BlueArchive/Combat/CombatManager.cs` (MonoBehaviour)
  - 기존 `CombatSystem` 래핑
  - 학생/적 GameObject 생성
  - 전투 애니메이션 트리거

- `Assets/_Project/Scripts/BlueArchive/UI/TestUIManager.cs` (MonoBehaviour)
  - UI 패널 관리
  - 테스트 진행 상황 업데이트

**핵심 구조**:
```csharp
public class StageManager : MonoBehaviour
{
    private StageController _stageController; // 기존 로직
    private GridVisualizer _gridVisualizer;   // 새로운 비주얼
    private List<PlatformObject> _platforms;  // 새로운 오브젝트

    public void InitializeStage(StageData data)
    {
        _stageController = new StageController();
        _stageController.InitializeStage(data);

        // 비주얼 생성
        _gridVisualizer.CreateGrid(data.gridWidth, data.gridHeight);
        CreatePlatforms(data.platformPositions);
    }
}
```

#### 1.2 게임 오브젝트 클래스
**목적**: 실제 씬에 배치되는 게임 오브젝트

**구현 파일**:
- `Assets/_Project/Scripts/BlueArchive/Character/StudentObject.cs` (MonoBehaviour)
  - `Student` 클래스 래핑
  - 비주얼 표시 (Sprite/Model)
  - HP 바, 이름 표시
  - 스킬 이펙트 재생

- `Assets/_Project/Scripts/BlueArchive/Character/EnemyObject.cs` (MonoBehaviour)
  - `Enemy` 클래스 래핑
  - 비주얼 표시
  - HP 바 표시

- `Assets/_Project/Scripts/BlueArchive/Stage/PlatformObject.cs` (MonoBehaviour)
  - 발판 비주얼
  - 이동 가능 여부 표시
  - 플레이어 위치 표시

- `Assets/_Project/Scripts/BlueArchive/Stage/GridVisualizer.cs` (MonoBehaviour)
  - 그리드 라인 표시
  - 좌표 표시

**핵심 구조**:
```csharp
public class StudentObject : MonoBehaviour
{
    public Student Student { get; private set; } // 로직 클래스

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Slider _hpBar;
    [SerializeField] private TextMeshProUGUI _nameText;

    public void Initialize(Student student)
    {
        Student = student;
        _nameText.text = student.Data.studentName;
        UpdateHP();
    }

    public void UpdateHP()
    {
        _hpBar.value = (float)Student.CurrentHP / Student.Data.maxHP;
    }

    public void PlaySkillEffect()
    {
        // 스킬 이펙트 재생
    }
}
```

---

### Phase 2: UI 시스템 (Day 10-11)

#### 2.1 테스트 진행 상황 UI
**파일**: `Assets/_Project/Scripts/BlueArchive/UI/TestProgressPanel.cs`

**표시 내용**:
- 현재 진행 중인 체크포인트 (1/6)
- 체크포인트 이름 및 상태 (✅/⏳/❌)
- 진행률 바
- 현재 테스트 메시지

**UI 구조**:
```
TestProgressPanel (Canvas)
├── Background
├── Title Text: "블루 아카이브 자동화 테스트"
├── CheckpointList (Vertical Layout)
│   ├── Checkpoint1: ✅ 플랫폼 이동 검증
│   ├── Checkpoint2: ✅ 전투 진입 검증
│   ├── Checkpoint3: ⏳ EX 스킬 사용 로깅
│   ├── Checkpoint4: ⏳ 코스트 소모 검증
│   ├── Checkpoint5: ⏳ 전투별 데미지 추적
│   └── Checkpoint6: ⏳ 보상 획득 검증
├── ProgressBar (Slider)
└── CurrentMessage Text: "전투 진입 중..."
```

#### 2.2 전투 로그 UI
**파일**: `Assets/_Project/Scripts/BlueArchive/UI/CombatLogPanel.cs`

**표시 내용**:
- 전투 로그 (ScrollView)
- 스킬 사용 로그
- 데미지 로그
- 적 격파 로그

**UI 구조**:
```
CombatLogPanel (Canvas)
├── Background
├── Title: "전투 로그"
├── ScrollView
│   └── Content (Vertical Layout)
│       ├── LogEntry: "[00:05] Shiroko가 EX 스킬 사용!"
│       ├── LogEntry: "[00:05] 일반병 A에게 1250 데미지!"
│       └── LogEntry: "[00:06] 일반병 A 격파!"
└── ClearButton
```

#### 2.3 코스트 & 학생 상태 UI
**파일**: `Assets/_Project/Scripts/BlueArchive/UI/CombatStatusPanel.cs`

**표시 내용**:
- 코스트 게이지 (0/10)
- 학생 목록 (4명)
- 각 학생의 HP, 스킬 쿨다운

**UI 구조**:
```
CombatStatusPanel (Canvas)
├── CostGauge
│   ├── CostText: "코스트: 7/10"
│   └── CostBar (Slider)
└── StudentList (Horizontal Layout)
    ├── StudentCard (Shiroko)
    │   ├── Portrait
    │   ├── Name: "Shiroko"
    │   ├── HP: 2431/2431
    │   └── SkillReady: ✅
    ├── StudentCard (Hoshino)
    ├── StudentCard (Aru)
    └── StudentCard (Haruna)
```

#### 2.4 보상 결과 UI
**파일**: `Assets/_Project/Scripts/BlueArchive/UI/RewardResultPanel.cs`

**표시 내용**:
- 스테이지 클리어 메시지
- 획득한 보상 목록
- 테스트 통계 (이동 횟수, 스킬 사용, 데미지)

**UI 구조**:
```
RewardResultPanel (Canvas)
├── Background (반투명)
├── Panel
│   ├── Title: "🎉 스테이지 클리어!"
│   ├── StageName: "Normal 1-4"
│   ├── RewardList (Grid Layout)
│   │   ├── Reward: 크레딧 x1000
│   │   ├── Reward: 노트 x5
│   │   ├── Reward: T1 가방 x1
│   │   └── Reward: 전술 EXP x150
│   ├── Statistics
│   │   ├── "이동 횟수: 7회"
│   │   ├── "스킬 사용: 4회"
│   │   └── "총 데미지: 5840"
│   └── CloseButton
```

---

### Phase 3: PlayMode 테스트 작성 (Day 11-12)

#### 3.1 PlayMode 테스트 구조
**파일**: `Assets/_Project/Scripts/Tests/PlayMode/BlueArchivePlayModeTests.cs`

**테스트 방식**:
- `[UnityTest]` 어트리뷰트 사용 (IEnumerator)
- 실제 GameObject 생성 및 조작
- UI 업데이트 대기 (yield return)
- 비주얼 검증 (GameObject 존재, UI 텍스트 확인)

**핵심 구조**:
```csharp
[UnityTest]
public IEnumerator PlayMode_FullStageFlow_AllCheckpoints()
{
    // 씬 로드
    yield return SceneManager.LoadSceneAsync("TestScene");

    // 매니저 찾기
    var stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    var combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
    var uiManager = GameObject.Find("UIManager").GetComponent<TestUIManager>();

    // 체크포인트 #1: 플랫폼 이동
    uiManager.UpdateCheckpoint(1, CheckpointStatus.InProgress);

    StageData stageData = StagePresets.CreateNormal1_4();
    stageManager.InitializeStage(stageData);
    yield return new WaitForSeconds(0.5f); // 비주얼 생성 대기

    // 플랫폼 오브젝트 생성 확인
    var platforms = GameObject.FindGameObjectsWithTag("Platform");
    Assert.AreEqual(6, platforms.Length, "플랫폼 개수 불일치");

    // 이동 테스트
    List<Vector2Int> path = stageManager.GetPathToBattle();
    foreach (var pos in path)
    {
        bool moved = stageManager.MovePlayer(pos);
        Assert.IsTrue(moved);
        yield return new WaitForSeconds(0.3f); // 이동 애니메이션 대기
    }

    uiManager.UpdateCheckpoint(1, CheckpointStatus.Completed);

    // 체크포인트 #2: 전투 진입
    uiManager.UpdateCheckpoint(2, CheckpointStatus.InProgress);
    // ...

    // 체크포인트 #3-6 계속...
}
```

#### 3.2 개별 PlayMode 테스트
**파일들**:
- `Assets/_Project/Scripts/Tests/PlayMode/StagePlayModeTests.cs`
  - 플랫폼 생성 테스트
  - 이동 비주얼 테스트

- `Assets/_Project/Scripts/Tests/PlayMode/CombatPlayModeTests.cs`
  - 학생/적 오브젝트 생성 테스트
  - 스킬 이펙트 재생 테스트
  - HP 바 업데이트 테스트

- `Assets/_Project/Scripts/Tests/PlayMode/UIPlayModeTests.cs`
  - UI 패널 표시 테스트
  - 코스트 게이지 업데이트 테스트
  - 로그 추가 테스트

---

### Phase 4: 씬 및 프리팹 구성 (Day 12-13)

#### 4.1 테스트 씬
**파일**: `Assets/_Project/Scenes/Testing/BlueArchiveTestScene.unity`

**씬 구조**:
```
BlueArchiveTestScene
├── [Managers]
│   ├── ServiceLocator (DontDestroyOnLoad)
│   ├── GameBootstrapper
│   ├── StageManager
│   ├── CombatManager
│   └── TestUIManager
├── [Stage]
│   ├── GridVisualizer
│   └── PlatformContainer (빈 오브젝트, 런타임에 생성)
├── [Characters]
│   └── CharacterContainer (빈 오브젝트, 런타임에 생성)
├── [UI Canvas]
│   ├── TestProgressPanel
│   ├── CombatLogPanel
│   ├── CombatStatusPanel
│   └── RewardResultPanel
└── [Camera]
    └── Main Camera (Orthographic, 2D 뷰)
```

#### 4.2 프리팹 제작
**학생 프리팹**: `Assets/_Project/Prefabs/Characters/StudentPrefab.prefab`
```
StudentPrefab
├── Visual
│   └── Sprite (placeholder - 간단한 아이콘)
├── Canvas (World Space)
│   ├── NameText
│   └── HPBar (Slider)
└── Components
    └── StudentObject.cs
```

**적 프리팹**: `Assets/_Project/Prefabs/Characters/EnemyPrefab.prefab`
```
EnemyPrefab
├── Visual
│   └── Sprite (빨간 사각형)
├── Canvas (World Space)
│   ├── NameText
│   └── HPBar (Slider)
└── Components
    └── EnemyObject.cs
```

**플랫폼 프리팹**: `Assets/_Project/Prefabs/Stage/PlatformPrefab.prefab`
```
PlatformPrefab
├── Visual
│   └── Sprite (회색 타일)
├── Outline (선택 시 강조)
└── Components
    └── PlatformObject.cs
```

---

## 📊 구현 우선순위

### High Priority (필수)
1. ✅ **StageManager + GridVisualizer** - 그리드 및 플랫폼 비주얼
2. ✅ **CombatManager + StudentObject/EnemyObject** - 캐릭터 오브젝트
3. ✅ **TestProgressPanel** - 테스트 진행 상황 UI
4. ✅ **PlayMode 통합 테스트** - 6개 체크포인트 자동화
5. ✅ **BlueArchiveTestScene** - 테스트 씬 구성

### Medium Priority (권장)
6. **CombatLogPanel** - 전투 로그 UI
7. **CombatStatusPanel** - 코스트 & 학생 상태 UI
8. **RewardResultPanel** - 보상 결과 UI
9. **개별 PlayMode 테스트** - 시스템별 세부 테스트

### Low Priority (선택)
10. **스킬 이펙트** - 파티클 시스템
11. **이동 애니메이션** - 부드러운 이동
12. **사운드** - 스킬 사용 효과음

---

## 🎨 비주얼 구현 방식

### 최소 비주얼 (Placeholder)
**목적**: 기능 검증에 집중, 빠른 구현

- **학생**: 간단한 아이콘/스프라이트 (64x64)
- **적**: 빨간 사각형
- **플랫폼**: 회색 타일
- **그리드**: 흰색 라인
- **UI**: Unity 기본 UI 컴포넌트

### 향후 확장 가능
- 실제 캐릭터 스프라이트 교체
- 애니메이션 추가
- 파티클 이펙트 추가
- 사운드 추가

---

## 🧪 테스트 전략

### EditMode 테스트 (기존 유지)
- **목적**: 로직 검증, 빠른 실행
- **178개 테스트 유지**
- CI/CD에서 빠른 검증용

### PlayMode 테스트 (새로 추가)
- **목적**: 통합 검증, 비주얼 확인
- **주요 시나리오 테스트** (5-10개)
- 자동화 실행 파일에서 사용

### 이중 검증 전략
```
[로직 검증]
EditMode Tests (178개)
    ↓
[통합 검증]
PlayMode Tests (10개)
    ↓
[최종 확인]
자동화 실행 파일
```

---

## 📅 일정 계획

### Day 9 (12/30): GameObject 기반 구조
- [ ] 새 브랜치 생성: `day9-playmode-objects`
- [ ] StageManager.cs 구현
- [ ] GridVisualizer.cs 구현
- [ ] PlatformObject.cs 구현
- [ ] 프리팹 제작 (Platform)
- [ ] 간단한 PlayMode 테스트 (플랫폼 생성)

**예상 소요 시간**: 4-6시간

---

### Day 10 (12/31): 캐릭터 오브젝트 & 전투
- [ ] CombatManager.cs 구현
- [ ] StudentObject.cs 구현
- [ ] EnemyObject.cs 구현
- [ ] 프리팹 제작 (Student, Enemy)
- [ ] PlayMode 전투 테스트

**예상 소요 시간**: 4-6시간

---

### Day 11 (01/01): UI 시스템
- [ ] TestUIManager.cs 구현
- [ ] TestProgressPanel.cs 구현
- [ ] CombatLogPanel.cs 구현
- [ ] CombatStatusPanel.cs 구현
- [ ] UI 프리팹 제작

**예상 소요 시간**: 4-6시간

---

### Day 12 (01/02): PlayMode 통합 테스트
- [ ] BlueArchiveTestScene.unity 구성
- [ ] BlueArchivePlayModeTests.cs 작성
- [ ] 6개 체크포인트 PlayMode 검증
- [ ] 버그 수정 및 개선

**예상 소요 시간**: 4-6시간

---

### Day 13 (01/03): 최종 통합 및 문서화
- [ ] RewardResultPanel.cs 구현
- [ ] 전체 PlayMode 테스트 실행
- [ ] USER_GUIDE.md 업데이트 (PlayMode 실행 방법)
- [ ] 스크린샷 및 데모 영상 제작
- [ ] main 브랜치 머지

**예상 소요 시간**: 3-5시간

---

## 🎯 성공 기준

### 필수 (Must Have)
- ✅ PlayMode에서 6개 체크포인트 모두 자동 실행
- ✅ 테스트 진행 상황을 UI에서 시각적으로 확인 가능
- ✅ 학생/적 오브젝트가 씬에 표시됨
- ✅ 플랫폼 이동이 비주얼로 표시됨
- ✅ 전투 로그가 UI에 표시됨

### 권장 (Should Have)
- 코스트 게이지 실시간 업데이트
- 학생 HP 바 실시간 업데이트
- 보상 결과 UI 표시

### 선택 (Nice to Have)
- 스킬 이펙트 파티클
- 부드러운 이동 애니메이션
- 효과음

---

## 🔧 기술적 고려사항

### 1. 기존 코드 보존
- **EditMode 테스트 유지**: 로직 검증용
- **Pure C# 클래스 유지**: MonoBehaviour와 분리
- **기존 테스트 통과**: 178개 테스트 모두 통과 유지

### 2. 성능 최적화
- **오브젝트 풀링**: 학생/적 오브젝트 재사용
- **UI 업데이트 최소화**: 변경 시에만 업데이트
- **비동기 로딩**: 씬 전환 시 로딩 화면

### 3. 테스트 안정성
- **대기 시간 조정**: yield return WaitForSeconds
- **NULL 체크**: GameObject.Find 결과 검증
- **에러 핸들링**: try-catch로 테스트 실패 방지

### 4. 확장성
- **인터페이스 활용**: IVisualizable, ITestable
- **이벤트 시스템**: UI 업데이트용 이벤트
- **데이터 기반**: ScriptableObject로 설정 관리

---

## 📦 최종 제출물 (업데이트)

### 1. 자동화 실행 파일
- [x] Windows 스탠드얼론 빌드 (.exe)
- [x] PlayMode 테스트 자동 실행
- [x] 테스트 결과 UI 표시

### 2. 소스 코드
- [x] 전체 Unity 프로젝트
- [x] EditMode + PlayMode 테스트
- [x] Git 커밋 히스토리

### 3. 자동화 사용 가이드
- [x] PlayMode 테스트 실행 방법
- [x] UI 해석 방법
- [x] 트러블슈팅 가이드

### 4. 테스트 결과 리포트
- [x] EditMode 결과 (178개 테스트)
- [x] PlayMode 결과 (10개 통합 테스트)
- [x] 스크린샷 및 데모 영상

---

## 📝 다음 단계

1. ✅ 계획서 검토 및 승인
2. ⏭️ Day 9 작업 시작: GameObject 기반 구조
3. ⏭️ 브랜치 생성 및 StageManager 구현

**시작 준비 완료! PlayMode 구현을 시작할까요?** 🎮
