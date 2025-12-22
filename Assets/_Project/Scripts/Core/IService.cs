namespace NexonGame.Core
{
    /// <summary>
    /// 모든 서비스가 구현해야 하는 기본 인터페이스
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 서비스 초기화
        /// </summary>
        void Initialize();

        /// <summary>
        /// 서비스 종료 및 정리
        /// </summary>
        void Cleanup();
    }
}
