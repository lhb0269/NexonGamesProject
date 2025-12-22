using UnityEngine;
using NexonGame.Core;
using NexonGame.Managers;

namespace NexonGame.Player
{
    /// <summary>
    /// 플레이어 컨트롤러 예제 - DI 패턴 사용
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("오디오")]
        [SerializeField] private AudioClip _jumpSound;
        [SerializeField] private AudioClip _footstepSound;

        // DI를 통해 주입받을 서비스들
        private IInputManager _inputManager;
        private IAudioManager _audioManager;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private float _gravity = -9.81f;

        private void Start()
        {
            // ServiceLocator를 통해 서비스 가져오기
            _inputManager = ServiceLocator.Instance.Get<IInputManager>();
            _audioManager = ServiceLocator.Instance.Get<IAudioManager>();

            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleMovement();
            HandleInput();
        }

        private void HandleMovement()
        {
            // InputManager 서비스를 통해 입력 받기
            Vector2 moveInput = _inputManager.GetMovementInput();

            // 이동 처리
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= _moveSpeed;

            // 중력 적용
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            _velocity.y += _gravity * Time.deltaTime;
            moveDirection.y = _velocity.y;

            _characterController.Move(moveDirection * Time.deltaTime);

            // 이동 중 발소리 재생 (예제)
            if (moveInput.magnitude > 0.1f && _characterController.isGrounded)
            {
                // 실제로는 발걸음 타이밍에 맞춰서 재생해야 함
            }
        }

        private void HandleInput()
        {
            // 공격 입력
            if (_inputManager.GetAttackInputDown())
            {
                PerformAttack();
            }

            // 상호작용 입력
            if (_inputManager.GetInteractInputDown())
            {
                TryInteract();
            }

            // 웅크리기 입력
            if (_inputManager.GetCrouchInputDown())
            {
                ToggleCrouch();
            }
        }

        private void PerformAttack()
        {
            Debug.Log("공격!");

            // AudioManager 서비스를 통해 사운드 재생
            if (_jumpSound != null)
            {
                _audioManager.PlaySFX(_jumpSound);
            }
        }

        private void TryInteract()
        {
            Debug.Log("상호작용 시도");
            // 상호작용 로직 구현
        }

        private void ToggleCrouch()
        {
            Debug.Log("웅크리기 토글");
            // 웅크리기 로직 구현
        }
    }
}
