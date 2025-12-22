# CLAUDE.md

이 파일은 이 저장소에서 작업할 때 Claude Code (claude.ai/code)에 지침을 제공합니다.

## 프로젝트 개요

Unity 6000.2.9f1을 사용하는 Universal Render Pipeline (URP) 템플릿 기반 프로젝트입니다. 새로운 Input System을 사용하며, 의존성 주입(DI) 패턴으로 설계되었습니다.

**현재 목표**: 블루 아카이브 Normal 1-4 스테이지 테스트 자동화 시스템 구현 (마감: 2025-12-30)

### 프로젝트 특성
- **테스트 대상**: 블루 아카이브 스타일의 전투 시스템
- **구현 방식**: 하이브리드 (최소 시각화 + 핵심 로직 + 자동화 테스트)
- **테스트 범위**: Normal 1-4 스테이지 클리어 과정 검증

## 빌드 및 개발 명령어

### 프로젝트 열기
이 프로젝트는 Unity Hub 또는 Unity Editor를 통해 열어야 합니다. 현재 명령줄 빌드 명령어는 구성되어 있지 않습니다.

### 프로젝트 파일
- 솔루션: `NexonGamesProject.slnx`
- 메인 어셈블리: `Assembly-CSharp.csproj` (런타임), `Assembly-CSharp-Editor.csproj` (에디터 스크립트)

## 프로젝트 아키텍처

### 렌더링
- **렌더 파이프라인**: Universal Render Pipeline (URP) 17.2.0
- **플랫폼별 렌더러**:
  - `PC_Renderer.asset` / `PC_RPAsset.asset` - 데스크톱 렌더링 설정
  - `Mobile_Renderer.asset` / `Mobile_RPAsset.asset` - 모바일 렌더링 설정
- **포스트 프로세싱**: `Assets/Settings/`의 Volume 프로필
  - `DefaultVolumeProfile.asset` - 전역 포스트 프로세싱
  - `SampleSceneProfile.asset` - 씬별 효과

### 입력 시스템
- **New Input System** (버전 1.14.2) - Unity의 새로운 입력 아키텍처 사용
- **입력 액션**: `Assets/InputSystem_Actions.inputactions`에 플레이어 액션 정의:
  - Move (Vector2) - 이동
  - Look (Vector2) - 시점
  - Attack (Button) - 공격
  - Interact (Button with Hold interaction) - 상호작용 (홀드)
  - Crouch (Button) - 웅크리기
- 입력 처리 스크립트 작성 시, Input을 직접 폴링하지 말고 InputSystem_Actions 에셋과 생성된 C# 클래스를 사용하세요

### 의존성 주입 아키텍처
- **패턴**: 의존성 주입을 위한 Service Locator 패턴
- **싱글톤 없음**: 모든 매니저가 싱글톤 패턴 대신 DI 사용
- **핵심 서비스**:
  - `IAudioManager` - 오디오 재생 및 관리
  - `ISceneLoader` - 씬 로딩 및 전환
  - `IInputManager` - 입력 처리 래퍼
  - `IUIManager` - UI 패널 관리
- **초기화**: 모든 서비스는 `GameBootstrapper.cs`에서 등록됨
- **서비스 접근**: `ServiceLocator.Instance.Get<IServiceType>()`

### 프로젝트 구조
```
Assets/_Project/
├── Scripts/
│   ├── Core/                       # ServiceLocator, GameBootstrapper, IService
│   ├── Managers/                   # 서비스 구현체 (Audio, Input, UI, Scene)
│   ├── BlueArchive/                # 블루 아카이브 게임 로직 구현
│   │   ├── Stage/                  # 스테이지 시스템 (발판, 전투 진입)
│   │   ├── Character/              # 학생(캐릭터) 시스템
│   │   ├── Combat/                 # 전투 시스템 (스킬, 데미지)
│   │   ├── Skill/                  # 스킬 및 코스트 시스템
│   │   └── Reward/                 # 보상 시스템
│   ├── Tests/                      # 자동화 테스트 코드
│   │   ├── EditMode/               # 단위 테스트
│   │   └── PlayMode/               # 통합 테스트
│   ├── Player/                     # 플레이어 관련 스크립트
│   ├── UI/                         # UI 컴포넌트
│   ├── Gameplay/                   # 게임 로직
│   └── Utilities/                  # 유틸리티 클래스
├── Art/                            # 텍스처, 머티리얼, 모델, 애니메이션
├── Audio/                          # 음악, SFX, 보이스
├── Prefabs/                        # 캐릭터, 환경, UI, 이펙트
├── Scenes/
│   ├── Development/                # 개발 테스트 씬
│   ├── Production/                 # 프로덕션 준비 씬
│   └── Testing/                    # 테스트 씬
├── Settings/                       # 입력, 렌더링, 오디오 설정
└── Resources/                      # 런타임 로드 에셋

Assets/Scenes/
└── SampleScene.unity               # 원본 템플릿 씬

Assets/Editor/
└── Scripts/                        # 에디터 도구 및 윈도우
```

## 주요 의존성

프로젝트가 사용하는 주요 Unity 패키지:
- `com.unity.render-pipelines.universal` 17.2.0 - URP 렌더링
- `com.unity.inputsystem` 1.14.2 - 새로운 Input System
- `com.unity.ai.navigation` 2.0.9 - NavMesh 및 경로 탐색
- `com.unity.visualscripting` 1.9.8 - 비주얼 스크립팅 그래프
- `com.unity.timeline` 1.8.9 - 컷신 및 애니메이션
- `com.unity.test-framework` 1.6.0 - Unity 테스트 프레임워크

## 의존성 주입 사용법

### 서비스 가져오기
```csharp
public class PlayerController : MonoBehaviour
{
    private IAudioManager _audioManager;
    private IInputManager _inputManager;

    private void Start()
    {
        _audioManager = ServiceLocator.Instance.Get<IAudioManager>();
        _inputManager = ServiceLocator.Instance.Get<IInputManager>();
    }

    private void Update()
    {
        Vector2 movement = _inputManager.GetMovementInput();
        if (_inputManager.GetAttackInputDown())
        {
            _audioManager.PlaySFX(attackSound);
        }
    }
}
```

### 새로운 서비스 추가하기
1. `Assets/_Project/Scripts/Managers/IYourService.cs`에 인터페이스 생성
2. `IService` 인터페이스와 함께 구현: `public class YourService : IYourService, IService`
3. `GameBootstrapper.InitializeServices()`에 추가:
   ```csharp
   var yourService = new Managers.YourService();
   ServiceLocator.Instance.Register<Managers.IYourService>(yourService);
   yourService.Initialize();
   ```

## 블루 아카이브 테스트 자동화 과제

### 테스트 체크 항목
1. **발판 이동 정상 여부** - 그리드 기반 이동 시스템 검증
2. **전투 정상 진입 여부** - 스테이지 상태 전환 검증
3. **각 학생별 EX 스킬 사용 여부** - 스킬 실행 로그 검증
4. **코스트 소모량 정상 여부** - 스킬 사용 시 코스트 계산 검증
5. **각 전투별 학생 데미지 기록** - 전투 로그 수집 및 검증
6. **보상 정상 획득 여부** - 스테이지 클리어 보상 검증

### 구현 범위
- **스테이지**: Normal 1-4 (간단한 2D 그리드 기반)
- **학생**: 4명의 캐릭터 (체력, 스킬, 코스트 시스템)
- **전투**: 타임라인 기반 전투 시스템
- **스킬**: EX 스킬 + 코스트 소모 메커니즘
- **테스트**: Unity Test Framework (EditMode + PlayMode)

### 제출물
1. 자동화 실행 파일 (테스트 실행 가능한 빌드)
2. 전체 소스 코드
3. 자동화 사용 가이드 (README 형식)
4. 테스트 결과 리포트 (체크 항목별 결과)

## 중요 사항

### 입력 시스템
- 프로젝트는 **새로운 Input System**을 사용하며, 레거시 Input Manager를 사용하지 않습니다
- 입력 액션은 `InputSystem_Actions.inputactions`에 정의되어 있습니다
- .inputactions 파일 수정 후에는 반드시 C# 클래스를 재생성해야 합니다
- **`IInputManager` 서비스를 통해 입력에 접근**하세요. 직접적인 Input System이나 레거시 Input 호출은 사용하지 마세요
- IInputManager 메서드: `GetMovementInput()`, `GetLookInput()`, `GetAttackInput()` 등

### URP 특성
- 셰이더는 반드시 URP 호환이어야 합니다 (URP/Lit, URP/Unlit, 또는 커스텀 Shader Graph 사용)
- 포스트 프로세싱은 Volume 컴포넌트를 사용하며, 구형 Post-processing Stack을 사용하지 않습니다
- 라이팅은 URP Renderer Features 시스템을 사용합니다

### 코드 스타일 및 아키텍처
- **의존성 주입 사용**: ServiceLocator를 통해 매니저에 접근하며, 절대 싱글톤을 사용하지 않습니다
- **인터페이스 우선**: 모든 매니저는 인터페이스를 통해 기능을 노출합니다
- **네임스페이스**: `NexonGame.Core`, `NexonGame.Managers`, `NexonGame.Player` 등
- **네이밍 규칙**:
  - 클래스, 메서드, 프로퍼티: `PascalCase`
  - Private 필드: `_camelCase` (언더스코어 접두사)
  - 인터페이스: `IServiceName` (I 접두사)
- **에디터 스크립트**: `Assets/Editor/Scripts/`에 위치하며 `NexonGame.Editor` 네임스페이스 사용
- **Assembly Definitions**: 프로젝트는 `NexonGame.Runtime.asmdef`와 `NexonGame.Editor.asmdef` 사용

### 에디터 도구
Unity 메뉴 `NexonGame`을 통해 접근:
- **프로젝트 설정**: GameBootstrapper 추가 등을 위한 설정 윈도우
- **씬 빠른 전환**: 씬 빠른 전환 단축키
- **도구 > 에셋 검증**: 프로젝트 에셋 검증
- **도구 > 누락된 스크립트 찾기**: 누락된 스크립트 참조 찾기
