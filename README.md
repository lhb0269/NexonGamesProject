# 블루 아카이브 테스트 자동화 프로젝트

[![Unity](https://img.shields.io/badge/Unity-6000.2.9f1-black.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

블루 아카이브 Normal 1-4 스테이지의 전투 시스템을 재현하고, 5개의 핵심 체크포인트를 자동으로 검증하는 테스트 자동화 프로젝트입니다.

## 📋 목차

- [프로젝트 개요](#프로젝트-개요)
- [주요 기능](#주요-기능)
- [기술 스택](#기술-스택)
- [시작하기](#시작하기)
- [테스트 실행](#테스트-실행)
- [프로젝트 구조](#프로젝트-구조)
- [문서](#문서)

---

## 프로젝트 개요

### 목표
블루 아카이브 스타일의 전투 시스템을 Unity로 구현하고, 다음 항목을 자동으로 검증합니다:

1. ✅ **플랫폼 이동 검증** - 8방향 인접 이동 시스템
2. ✅ **전투 진입 검증** - 스테이지에서 전투로의 전환
3. ✅ **EX 스킬 사용 검증** - 스킬 실행 및 코스트 소모
4. ✅ **데미지 추적 검증** - 학생별 데미지 통계
5. ✅ **보상 획득 검증** - 스테이지 클리어 보상

### 테스트 범위
- **스테이지**: Normal 1-4
- **학생**: 4명 (아리스, 호시노, 이로하, 시로코)
- **적**: 3명 (일반병)
- **테스트 방식**: 자동화된 통합 테스트

---

## 주요 기능

### 1. 플랫폼 이동 시스템
- 8방향 인접 이동 (상하좌우 + 대각선)
- 클릭 기반 이동 메커니즘
- 인접 검증 및 이동 실패 처리

### 2. 전투 시스템
- 턴 기반 전투 (블루 아카이브 스타일)
- 코스트 시스템 (자동 회복)
- EX 스킬 실행 및 쿨타임 관리
- 실시간 전투 로그

### 3. 스킬 시스템
- 단일/다중/광역 타겟팅
- 데미지 계산 및 적용
- 학생별 고유 스킬
- 코스트 기반 스킬 사용 제한

### 4. 통계 및 로깅
- 전투 이벤트 실시간 기록
- 학생별 데미지 통계
- 스킬 사용 횟수 추적
- 코스트 소모 내역

### 5. UI 시스템
- 테스트 진행 상황 표시
- 코스트 바 및 스킬 버튼
- 전투 로그 패널
- 보상 결과 화면

---

## 기술 스택

### Unity 환경
- **Unity 버전**: 6000.2.9f1
- **렌더 파이프라인**: Universal Render Pipeline (URP) 17.2.0
- **입력 시스템**: New Input System 1.14.2
- **테스트 프레임워크**: Unity Test Framework 1.6.0

### 아키텍처 패턴
- **의존성 주입 (DI)**: Service Locator 패턴
- **Pure C# 로직**: MonoBehaviour와 로직 분리
- **이벤트 기반 통신**: C# Events
- **AAA 테스트 패턴**: Arrange-Act-Assert

### 주요 패키지
```
com.unity.render-pipelines.universal@17.2.0
com.unity.inputsystem@1.14.2
com.unity.test-framework@1.6.0
com.unity.ai.navigation@2.0.9
```

---

## 시작하기

### 필수 요구사항
- Unity Hub 설치
- Unity 6000.2.9f1 이상
- Windows 10/11, macOS 10.15+, 또는 Linux
- 최소 4GB RAM
- 2GB 여유 저장공간

### 프로젝트 설치

1. **저장소 클론**
```bash
git clone https://github.com/yourusername/NexonGamesProject.git
cd NexonGamesProject
```

2. **Unity Hub에서 프로젝트 열기**
```
Unity Hub → 프로젝트 추가 → NexonGamesProject 폴더 선택
```

3. **Unity 버전 확인**
   - Unity 6000.2.9f1 또는 호환 버전 사용
   - URP가 자동으로 설정됨

---

## 테스트 실행

### 방법 1: Unity Editor에서 실행 (권장)

1. Unity Editor에서 프로젝트 열기
2. 상단 메뉴: `Window` → `General` → `Test Runner`
3. `PlayMode` 탭 선택
4. `BlueArchiveIntegrationTests` 확장
5. `FullIntegration_AllFiveCheckpoints_ShouldPass` 우클릭
6. `Run Selected` 클릭

**예상 실행 시간**: 약 30-40초

### 방법 2: 배치 파일 실행 (Windows)

```batch
# 프로젝트 루트에서 실행
RunTests.bat
```

**주의**: `RunTests.bat` 파일 내 Unity 설치 경로를 확인하세요.

### 방법 3: 명령줄 실행

```bash
# Windows
Unity.exe -runTests -batchmode -projectPath "경로" -testResults "TestResults.xml" -testPlatform PlayMode

# macOS
/Applications/Unity/Hub/Editor/6000.2.9f1/Unity.app/Contents/MacOS/Unity -runTests -batchmode -projectPath "경로" -testResults "TestResults.xml" -testPlatform PlayMode
```

### 테스트 결과 확인

테스트 실행 후 다음 파일이 생성됩니다:
- `TestResults.xml` - 테스트 결과 (NUnit 형식)
- `TestLog.txt` - 상세 로그

---

## 프로젝트 구조

```
NexonGamesProject/
├── Assets/
│   └── _Project/
│       ├── Scripts/
│       │   ├── Core/                    # ServiceLocator, GameBootstrapper
│       │   ├── Managers/                # 서비스 구현체
│       │   ├── BlueArchive/             # 게임 로직
│       │   │   ├── Stage/               # 스테이지 시스템
│       │   │   ├── Combat/              # 전투 시스템
│       │   │   ├── Character/           # 학생 및 적
│       │   │   ├── Skill/               # 스킬 및 코스트
│       │   │   ├── Reward/              # 보상 시스템
│       │   │   ├── Data/                # ScriptableObject
│       │   │   └── UI/                  # 게임 UI
│       │   └── Tests/
│       │       ├── EditMode/            # 단위 테스트
│       │       └── PlayMode/            # 통합 테스트
│       ├── Prefabs/
│       ├── Scenes/
│       │   ├── Development/
│       │   ├── Production/
│       │   └── Testing/
│       └── Settings/
├── CLAUDE.md                            # 프로젝트 개요
├── CODE_GUIDE.md                        # 코드 작성 가이드
├── AUTOMATION_GUIDE.md                  # 자동화 사용 가이드
├── README.md                            # 프로젝트 소개 (이 파일)
├── CHANGELOG.md                         # 변경 이력
└── RunTests.bat                         # 테스트 실행 스크립트
```

---

## 문서

### 주요 문서
- **[CLAUDE.md](CLAUDE.md)** - 프로젝트 아키텍처 및 개발 가이드
- **[CODE_GUIDE.md](CODE_GUIDE.md)** - 코드 작성 규칙 및 패턴
- **[AUTOMATION_GUIDE.md](AUTOMATION_GUIDE.md)** - 테스트 자동화 사용 가이드
- **[CHANGELOG.md](CHANGELOG.md)** - 버전별 변경 이력

### 코드 규칙
- **네이밍**: PascalCase (클래스/메서드), _camelCase (private 필드)
- **네임스페이스**: `NexonGame.*`
- **패턴**: 의존성 주입 (싱글톤 사용 금지)
- **테스트**: AAA 패턴 (Arrange-Act-Assert)

### 아키텍처 개요

#### 의존성 주입
```csharp
// 서비스 등록
ServiceLocator.Instance.Register<IAudioManager>(audioManager);

// 서비스 사용
private IAudioManager _audioManager;
_audioManager = ServiceLocator.Instance.Get<IAudioManager>();
```

#### MonoBehaviour + Pure C# 분리
```csharp
// StageManager.cs (MonoBehaviour - GameObject 관리)
public class StageManager : MonoBehaviour
{
    private StageController _stageController; // Pure C# 로직
}

// StageController.cs (Pure C# - 로직만)
public class StageController
{
    // 순수 로직 처리
}
```

---

## 테스트 체크포인트 설명

### 체크포인트 #1: 플랫폼 이동
- **목적**: 8방향 인접 이동 검증
- **경로**: (0,0) → (1,1) → (0,2) → (1,1) → (2,1) → (3,1)
- **검증**: 이동 횟수, 최종 위치, 스테이지 상태

### 체크포인트 #2: 전투 진입
- **목적**: 전투 초기화 검증
- **검증**: 오브젝트 생성, UI 패널, 코스트 시스템

### 체크포인트 #3: EX 스킬 사용
- **목적**: 스킬 실행 및 코스트 소모 검증
- **검증**: 스킬 사용 횟수, 데미지 발생, 코스트 소모

### 체크포인트 #4: 데미지 추적
- **목적**: 전투 로그 및 통계 검증
- **검증**: 총 데미지, 학생별 데미지 통계

### 체크포인트 #5: 보상 획득
- **목적**: 스테이지 클리어 및 보상 검증
- **검증**: 스테이지 상태, 보상 생성

---

## 개발 가이드

### 새로운 기능 추가

1. **인터페이스 정의** (필요시)
```csharp
public interface IYourService : IService
{
    void DoSomething();
}
```

2. **Pure C# 로직 작성**
```csharp
public class YourService : IYourService
{
    public void Initialize() { }
    public void DoSomething() { }
}
```

3. **MonoBehaviour 래퍼 작성** (필요시)
```csharp
public class YourManager : MonoBehaviour
{
    private IYourService _service;
}
```

4. **ServiceLocator 등록**
```csharp
// GameBootstrapper.cs
var yourService = new YourService();
ServiceLocator.Instance.Register<IYourService>(yourService);
yourService.Initialize();
```

5. **테스트 작성**
```csharp
[UnityTest]
public IEnumerator YourFeature_Condition_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

### 코드 리뷰 체크리스트
- [ ] 싱글톤 패턴 사용하지 않음
- [ ] 인터페이스를 통한 의존성 주입
- [ ] XML 문서 주석 작성
- [ ] AAA 패턴 준수 (테스트)
- [ ] 네이밍 규칙 준수
- [ ] MonoBehaviour와 Pure C# 분리

---

## 빌드 및 배포

### 빌드 설정
1. `File` → `Build Settings`
2. 플랫폼 선택 (Windows, macOS, Linux)
3. `Build` 클릭

### 테스트 포함 빌드
테스트 시뮬레이션 씬을 포함하여 빌드하면 독립 실행 파일로 테스트를 실행할 수 있습니다.

---

## 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

---

## 기여

기여는 언제나 환영합니다! [CONTRIBUTING.md](CONTRIBUTING.md)를 참조하세요.

### 기여 방법
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 연락처

프로젝트 관련 문의: [이메일 주소]

프로젝트 링크: [https://github.com/yourusername/NexonGamesProject](https://github.com/yourusername/NexonGamesProject)

---

## 감사의 말

- Unity Technologies - Unity 엔진
- Nexon Games - 블루 아카이브 게임 디자인 영감

---

## 변경 이력

최신 변경 사항은 [CHANGELOG.md](CHANGELOG.md)를 참조하세요.

### 주요 업데이트
- **2025-12-25**: 플랫폼 클릭 이동 시스템 구현
- **2025-12-25**: 코드 가이드 및 자동화 가이드 추가
- **2025-12-24**: AAA 패턴 적용하여 테스트 리팩토링
- **2025-12-23**: 스킬 버튼 시스템 구현
- **2025-12-22**: 보상 시스템 및 RewardResultPanel 완료

---

**Made with ❤️ using Unity**
