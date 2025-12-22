using System;

namespace NexonGame.Managers
{
    /// <summary>
    /// UI 관리 인터페이스
    /// </summary>
    public interface IUIManager
    {
        void ShowPanel(string panelName);
        void HidePanel(string panelName);
        void HideAllPanels();
        void ShowLoadingScreen(bool show);
        void ShowMessage(string message, float duration = 3f);
    }
}
