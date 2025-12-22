using UnityEngine;
using NexonGame.Core;

namespace NexonGame.Managers
{
    /// <summary>
    /// 입력 시스템 관리자 (DI 패턴)
    ///
    /// 주의: InputSystem_Actions.inputactions 파일에서
    /// "Generate C# Class" 버튼을 클릭하여 C# 클래스를 생성해야 합니다.
    /// 현재는 임시로 레거시 Input을 사용합니다.
    /// </summary>
    public class InputManager : IInputManager, IService
    {
        // TODO: InputSystem_Actions C# 클래스 생성 후 활성화
        // private InputSystem_Actions _inputActions;
        private bool _isEnabled = true;

        public void Initialize()
        {
            // TODO: InputSystem_Actions C# 클래스 생성 후 활성화
            // _inputActions = new InputSystem_Actions();
            // _inputActions.Enable();

            Debug.Log("InputManager 초기화 완료 (임시 레거시 Input 사용 중)");
        }

        public void Cleanup()
        {
            // TODO: InputSystem_Actions C# 클래스 생성 후 활성화
            // _inputActions?.Disable();
            // _inputActions?.Dispose();
        }

        public Vector2 GetMovementInput()
        {
            if (!_isEnabled) return Vector2.zero;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Move.ReadValue<Vector2>();

            // 임시 레거시 Input 사용
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }

        public Vector2 GetLookInput()
        {
            if (!_isEnabled) return Vector2.zero;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Look.ReadValue<Vector2>();

            // 임시 마우스 입력
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        public bool GetAttackInput()
        {
            if (!_isEnabled) return false;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Attack.IsPressed();

            return Input.GetMouseButton(0);
        }

        public bool GetAttackInputDown()
        {
            if (!_isEnabled) return false;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Attack.WasPressedThisFrame();

            return Input.GetMouseButtonDown(0);
        }

        public bool GetInteractInput()
        {
            if (!_isEnabled) return false;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Interact.IsPressed();

            return Input.GetKey(KeyCode.E);
        }

        public bool GetInteractInputDown()
        {
            if (!_isEnabled) return false;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Interact.WasPressedThisFrame();

            return Input.GetKeyDown(KeyCode.E);
        }

        public bool GetCrouchInput()
        {
            if (!_isEnabled) return false;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Crouch.IsPressed();

            return Input.GetKey(KeyCode.LeftControl);
        }

        public bool GetCrouchInputDown()
        {
            if (!_isEnabled) return false;

            // TODO: InputSystem_Actions 사용으로 변경
            // return _inputActions.Player.Crouch.WasPressedThisFrame();

            return Input.GetKeyDown(KeyCode.LeftControl);
        }

        public void EnableInput()
        {
            _isEnabled = true;
            // TODO: InputSystem_Actions 사용으로 변경
            // _inputActions?.Enable();
        }

        public void DisableInput()
        {
            _isEnabled = false;
            // TODO: InputSystem_Actions 사용으로 변경
            // _inputActions?.Disable();
        }
    }
}
