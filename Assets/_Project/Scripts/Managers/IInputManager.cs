using System;
using UnityEngine;

namespace NexonGame.Managers
{
    /// <summary>
    /// 입력 관리 인터페이스
    /// </summary>
    public interface IInputManager
    {
        Vector2 GetMovementInput();
        Vector2 GetLookInput();
        bool GetAttackInput();
        bool GetAttackInputDown();
        bool GetInteractInput();
        bool GetInteractInputDown();
        bool GetCrouchInput();
        bool GetCrouchInputDown();
        void EnableInput();
        void DisableInput();
    }
}
