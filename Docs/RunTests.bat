@echo off
echo =====================================
echo 블루 아카이브 테스트 자동화 실행
echo =====================================
echo.

REM Unity 설치 경로 (사용자 환경에 맞게 수정)
set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.9f1\Editor\Unity.exe

REM 프로젝트 경로 (현재 디렉토리)
set PROJECT_PATH=%~dp0

REM 결과 파일 경로
set RESULTS_FILE=%PROJECT_PATH%TestResults.xml
set LOG_FILE=%PROJECT_PATH%TestLog.txt

echo Unity 경로: %UNITY_PATH%
echo 프로젝트 경로: %PROJECT_PATH%
echo.

REM Unity 설치 확인
if not exist "%UNITY_PATH%" (
    echo [오류] Unity를 찾을 수 없습니다.
    echo Unity 설치 경로를 확인하세요: %UNITY_PATH%
    echo.
    pause
    exit /b 1
)

echo [1/3] Unity Test Runner 실행 중...
echo.

REM Unity 테스트 실행
"%UNITY_PATH%" -runTests -batchmode -projectPath "%PROJECT_PATH%" -testResults "%RESULTS_FILE%" -testPlatform PlayMode -logFile "%LOG_FILE%"

REM 결과 확인
if %ERRORLEVEL% EQU 0 (
    echo.
    echo =====================================
    echo ✅ 테스트 완료!
    echo =====================================
) else (
    echo.
    echo =====================================
    echo ❌ 테스트 실패 또는 오류 발생
    echo =====================================
)

echo.
echo [2/3] 결과 파일 생성 확인...

if exist "%RESULTS_FILE%" (
    echo ✅ TestResults.xml 생성됨
) else (
    echo ❌ TestResults.xml 생성 실패
)

if exist "%LOG_FILE%" (
    echo ✅ TestLog.txt 생성됨
) else (
    echo ❌ TestLog.txt 생성 실패
)

echo.
echo [3/3] 완료
echo.
echo 결과 파일:
echo - TestResults.xml: 테스트 결과 (XML 형식)
echo - TestLog.txt: 상세 로그
echo.
echo =====================================

pause
