using UnityEngine;
using UnityEngine.InputSystem;
using NexonGame.Core;

namespace NexonGame.Managers
{
    /// <summary>
    /// 입력 시스템 관리자 (DI 패턴)
    /// Unity의 새로운 Input System을 사용합니다
    /// </summary>
    public class InputManager : IInputManager, IService
    {
        private InputSystem_Actions _inputActions;
        private bool _isEnabled = true;

        public void Initialize()
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Enable();

            Debug.Log("InputManager 초기화 완료");
        }

        public void Cleanup()
        {
            _inputActions?.Disable();
            _inputActions?.Dispose();
        }

        public Vector2 GetMovementInput()
        {
            if (!_isEnabled) return Vector2.zero;
            return _inputActions.Player.Move.ReadValue<Vector2>();
        }

        public Vector2 GetLookInput()
        {
            if (!_isEnabled) return Vector2.zero;
            return _inputActions.Player.Look.ReadValue<Vector2>();
        }

        public bool GetAttackInput()
        {
            if (!_isEnabled) return false;
            return _inputActions.Player.Attack.IsPressed();
        }

        public bool GetAttackInputDown()
        {
            if (!_isEnabled) return false;
            return _inputActions.Player.Attack.WasPressedThisFrame();
        }

        public bool GetInteractInput()
        {
            if (!_isEnabled) return false;
            return _inputActions.Player.Interact.IsPressed();
        }

        public bool GetInteractInputDown()
        {
            if (!_isEnabled) return false;
            return _inputActions.Player.Interact.WasPressedThisFrame();
        }

        public bool GetCrouchInput()
        {
            if (!_isEnabled) return false;
            return _inputActions.Player.Crouch.IsPressed();
        }

        public bool GetCrouchInputDown()
        {
            if (!_isEnabled) return false;
            return _inputActions.Player.Crouch.WasPressedThisFrame();
        }

        public void EnableInput()
        {
            _isEnabled = true;
            _inputActions?.Enable();
        }

        public void DisableInput()
        {
            _isEnabled = false;
            _inputActions?.Disable();
        }
    }
}
