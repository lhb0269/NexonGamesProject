using UnityEngine;
using NexonGame.Managers;
using NexonGame.Utilities;

namespace NexonGame.Player
{
    /// <summary>
    /// ServiceInjector를 사용한 플레이어 컨트롤러 예제
    /// Awake에서 자동으로 서비스를 주입받습니다
    /// </summary>
    public class PlayerControllerWithInjector : ServiceInjector
    {
        [Header("이동 설정")]
        [SerializeField] private float _moveSpeed = 5f;

        // 서비스들은 InjectServices()에서 주입됨
        private IInputManager _inputManager;
        private IAudioManager _audioManager;

        protected override void InjectServices()
        {
            // ServiceInjector의 GetService() 헬퍼 메서드 사용
            _inputManager = GetService<IInputManager>();
            _audioManager = GetService<IAudioManager>();

            Debug.Log("서비스 주입 완료");
        }

        private void Update()
        {
            // 주입받은 서비스 사용
            Vector2 moveInput = _inputManager.GetMovementInput();

            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * _moveSpeed * Time.deltaTime;
                transform.Translate(movement, Space.World);
            }

            if (_inputManager.GetAttackInputDown())
            {
                Debug.Log("공격!");
                // _audioManager.PlaySFX(attackSound);
            }
        }
    }
}
