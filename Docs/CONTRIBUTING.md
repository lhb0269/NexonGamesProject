# 개발 가이드라인

## 코드 스타일

### C# 코딩 규칙

1. **네이밍 규칙**
   - 클래스, 메서드, 프로퍼티: `PascalCase`
   - 로컬 변수, 매개변수: `camelCase`
   - Private 필드: `_camelCase` (언더스코어 접두사)
   - 상수: `UPPER_CASE` 또는 `PascalCase`
   - 인터페이스: `I` 접두사 (예: `IService`, `IAudioManager`)

2. **네임스페이스**
   - 모든 스크립트는 적절한 네임스페이스를 사용해야 합니다
   - 형식: `NexonGame.{카테고리}`
   - 예시: `NexonGame.Core`, `NexonGame.Managers`, `NexonGame.Player`

3. **주석**
   - 모든 public 클래스와 메서드는 XML 주석을 작성합니다
   - 복잡한 로직에는 설명 주석을 추가합니다

```csharp
/// <summary>
/// 오디오를 재생하고 관리하는 매니저입니다
/// </summary>
public class AudioManager : IAudioManager
{
    /// <summary>
    /// 배경 음악을 재생합니다
    /// </summary>
    /// <param name="clip">재생할 오디오 클립</param>
    /// <param name="loop">반복 재생 여부</param>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        // 구현...
    }
}
```

## 의존성 주입 패턴

### 새로운 서비스 추가하기

1. **인터페이스 정의**

```csharp
namespace NexonGame.Managers
{
    public interface IMyService
    {
        void DoSomething();
    }
}
```

2. **서비스 구현**

```csharp
using NexonGame.Core;

namespace NexonGame.Managers
{
    public class MyService : IMyService, IService
    {
        public void Initialize()
        {
            // 초기화 로직
        }

        public void Cleanup()
        {
            // 정리 로직
        }

        public void DoSomething()
        {
            // 기능 구현
        }
    }
}
```

3. **GameBootstrapper에 등록**

```csharp
private void InitializeServices()
{
    // 기존 서비스들...

    var myService = new Managers.MyService();
    ServiceLocator.Instance.Register<Managers.IMyService>(myService);
    myService.Initialize();
}
```

4. **서비스 사용**

```csharp
public class MyGameClass : MonoBehaviour
{
    private IMyService _myService;

    private void Start()
    {
        _myService = ServiceLocator.Instance.Get<IMyService>();
    }

    private void Update()
    {
        _myService.DoSomething();
    }
}
```

## Git 워크플로우

### 브랜치 전략

- `main`: 프로덕션 배포용 브랜치
- `develop`: 개발 통합 브랜치
- `feature/*`: 새로운 기능 개발
- `bugfix/*`: 버그 수정
- `hotfix/*`: 긴급 수정

### 커밋 메시지 규칙

```
<타입>: <제목>

<본문> (선택)

<푸터> (선택)
```

**타입:**
- `feat`: 새로운 기능
- `fix`: 버그 수정
- `docs`: 문서 변경
- `style`: 코드 포맷팅, 세미콜론 누락 등
- `refactor`: 코드 리팩토링
- `test`: 테스트 코드 추가
- `chore`: 빌드 작업, 패키지 매니저 설정 등

**예시:**
```
feat: AudioManager에 3D 사운드 지원 추가

PlaySFXAtPosition 메서드를 추가하여
3D 공간에서 사운드를 재생할 수 있도록 함
```

## Unity 특정 가이드라인

### 씬 관리

- 개발용 씬: `Assets/_Project/Scenes/Development/`
- 프로덕션 씬: `Assets/_Project/Scenes/Production/`
- 테스트 씬: `Assets/_Project/Scenes/Testing/`

### 프리팹 구성

- 프리팹 네이밍: `{타입}_{이름}` (예: `Character_Player`, `UI_MainMenu`)
- 프리팹 변형(Variant)을 적극 활용
- 프리팹 오버라이드는 최소화

### 에셋 임포트 설정

**텍스처:**
- 최대 크기: 2048px (모바일: 1024px)
- 압축: 플랫폼별 자동 압축 사용
- Mipmap: UI 제외 활성화

**오디오:**
- 음악: Streaming, Vorbis/MP3
- SFX: Decompress on Load, PCM
- 배경음: Load in Background 활성화

**모델:**
- Import Cameras/Lights: 비활성화
- Mesh Compression: Medium
- 불필요한 애니메이션 제거

## 성능 최적화 가이드

### 코드 최적화

1. **Awake/Start에서의 초기화**
   - 무거운 초기화는 코루틴으로 분산
   - GetComponent는 Start에서 캐싱

2. **Update 최적화**
   - 매 프레임 실행이 필요 없는 코드는 코루틴이나 InvokeRepeating 사용
   - Update에서 new 키워드 사용 최소화

3. **메모리 관리**
   - Object Pooling 패턴 활용
   - 문자열 연산은 StringBuilder 사용
   - LINQ는 성능이 중요한 곳에서 사용 자제

### 프로파일링

- Unity Profiler를 정기적으로 사용
- 프레임 드롭이 발생하는 지점 확인
- 메모리 누수 체크

## 테스트 작성

### Unit Test

```csharp
using NUnit.Framework;

namespace NexonGame.Tests
{
    public class ServiceLocatorTests
    {
        [SetUp]
        public void Setup()
        {
            ServiceLocator.ResetInstance();
        }

        [Test]
        public void Register_And_Get_Service()
        {
            var service = new MyTestService();
            ServiceLocator.Instance.Register<IMyTestService>(service);

            var retrieved = ServiceLocator.Instance.Get<IMyTestService>();

            Assert.AreEqual(service, retrieved);
        }
    }
}
```

## 문서화

- 복잡한 시스템은 별도 마크다운 문서 작성
- API 변경사항은 CHANGELOG.md에 기록
- 새로운 패턴이나 구조는 README.md 업데이트

## 코드 리뷰 체크리스트

- [ ] 코드 스타일 가이드를 준수하는가?
- [ ] DI 패턴을 올바르게 사용하는가?
- [ ] 적절한 주석이 작성되어 있는가?
- [ ] 성능에 문제가 없는가?
- [ ] 메모리 누수 가능성은 없는가?
- [ ] 에러 처리가 적절한가?
- [ ] 테스트 코드가 작성되어 있는가?
