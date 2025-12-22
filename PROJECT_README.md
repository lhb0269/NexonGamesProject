# NexonGamesProject - 블루 아카이브 테스트 자동화

Unity 6000.2.9f1 기반의 블루 아카이브 스테이지 테스트 자동화 프로젝트입니다.

## 프로젝트 개요

- **Unity 버전**: 6000.2.9f1
- **렌더 파이프라인**: Universal Render Pipeline (URP) 17.2.0
- **입력 시스템**: Unity Input System 1.14.2
- **아키텍처 패턴**: 의존성 주입(Dependency Injection) 패턴
- **테스트 프레임워크**: Unity Test Framework (NUnit)

## 과제 목표

블루 아카이브 Normal 1-4 스테이지를 클리어하는 과정을 자동화하고, 다음 항목들을 검증하는 테스트 시스템 구현:

### 체크 항목
✅ 발판 이동 정상 여부
✅ 전투 정상 진입 여부
✅ 각 학생(캐릭터)별 EX 스킬 사용 여부
✅ 스킬 사용 시 코스트 소모량 정상 여부
✅ 각 전투별 학생 데미지 기록
✅ 보상 정상 획득 여부

### 구현 방식
- **하이브리드 접근**: 최소한의 시각적 요소 + 핵심 전투 로직 + 자동화 테스트
- **코드 기반**: 외부 자동화 도구 없이 순수 C# 코드로만 구현
- **플랫폼**: PC (Windows) 스탠드얼론

## 프로젝트 구조

```
Assets/_Project/
├── Scripts/
│   ├── Core/              # 핵심 시스템 (ServiceLocator, GameBootstrapper)
│   ├── Managers/          # 게임 매니저들 (Audio, Input, UI, Scene)
│   ├── Player/            # 플레이어 관련 스크립트
│   ├── UI/                # UI 관련 스크립트
│   ├── Gameplay/          # 게임플레이 로직
│   └── Utilities/         # 유틸리티 클래스
├── Art/                   # 아트 에셋
├── Audio/                 # 오디오 에셋
├── Prefabs/               # 프리팹
├── Scenes/                # 씬 파일
│   ├── Development/       # 개발용 씬
│   ├── Production/        # 프로덕션 씬
│   └── Testing/           # 테스트 씬
├── Settings/              # 프로젝트 설정
└── Resources/             # 런타임 로드 리소스
```

## 의존성 주입 (DI) 패턴

이 프로젝트는 싱글톤 패턴 대신 의존성 주입 패턴을 사용합니다.

### ServiceLocator 사용법

```csharp
// 서비스 등록 (GameBootstrapper에서 자동으로 처리)
var audioManager = new AudioManager();
ServiceLocator.Instance.Register<IAudioManager>(audioManager);
audioManager.Initialize();

// 서비스 사용
public class PlayerController : MonoBehaviour
{
    private IAudioManager _audioManager;

    private void Start()
    {
        _audioManager = ServiceLocator.Instance.Get<IAudioManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _audioManager.PlaySFX(jumpSound);
        }
    }
}
```

### 사용 가능한 서비스

- `IAudioManager` - 오디오 재생 및 관리
- `ISceneLoader` - 씬 로딩 및 전환
- `IInputManager` - 입력 처리 (새로운 Input System 사용)
- `IUIManager` - UI 패널 관리

## 시작하기

1. Unity Hub에서 Unity 6000.2.9f1을 설치합니다
2. 프로젝트를 Unity Hub에 추가하고 엽니다
3. `Assets/Scenes/SampleScene.unity`를 엽니다
4. Unity 메뉴에서 `NexonGame > 프로젝트 설정`을 선택합니다
5. "GameBootstrapper 씬에 추가" 버튼을 클릭합니다
6. 플레이 모드로 실행합니다

## 입력 시스템

프로젝트는 Unity의 새로운 Input System을 사용합니다.

### 입력 액션

- **Move**: 캐릭터 이동 (WASD, 왼쪽 스틱)
- **Look**: 카메라 회전 (마우스, 오른쪽 스틱)
- **Attack**: 공격 (마우스 좌클릭, X 버튼)
- **Interact**: 상호작용 (E, Y 버튼) - 홀드 인터랙션
- **Crouch**: 웅크리기 (Ctrl, B 버튼)

### 입력 설정 수정

1. `Assets/InputSystem_Actions.inputactions` 파일을 선택합니다
2. Inspector에서 설정을 수정합니다
3. "Generate C# Class" 버튼을 클릭하여 C# 클래스를 재생성합니다

## 에디터 도구

Unity 메뉴의 `NexonGame` 항목에서 다음 도구들을 사용할 수 있습니다:

- **프로젝트 설정**: 프로젝트 초기 설정 도구
- **씬 빠른 전환**: 자주 사용하는 씬을 빠르게 엽니다
- **에셋 검증**: 프로젝트 에셋의 문제점을 확인합니다
- **누락된 스크립트 찾기**: 씬에서 누락된 스크립트를 검색합니다

## 코딩 규칙

- **네이밍**: PascalCase (클래스, 메서드, 프로퍼티), camelCase (로컬 변수)
- **Private 필드**: `_camelCase` (언더스코어 접두사)
- **인터페이스**: `I` 접두사 (예: `IAudioManager`)
- **네임스페이스**: `NexonGame.{카테고리}` (예: `NexonGame.Managers`)

## 빌드 설정

현재는 PC (Windows) 타겟으로 설정되어 있습니다.

추가 플랫폼을 위해서는:
1. `File > Build Settings`를 엽니다
2. 원하는 플랫폼을 선택하고 "Switch Platform"을 클릭합니다
3. 플랫폼별 최적화 설정을 조정합니다

## 라이선스

[라이선스 정보 추가 필요]

## 기여하기

자세한 내용은 [CONTRIBUTING.md](CONTRIBUTING.md)를 참조하세요.
