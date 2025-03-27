using R3;

namespace EdgeStudio.UI
{
    public static class UIEvent
    {
        public static Subject<(float value,string title)> UpdateLoadingProgress = new();
        public static Subject<Unit> CloseLoadingUI = new();
        public static Subject<int> EventMoveHouse = new();
        public static Subject<Unit> RefreshShelfUI = new();
        public static Subject<(int employeeId,int pos)> RefreshEmployeePosition = new();
        public static Subject<Unit> RefreshEmployeePositionFinished = new();
        public static Subject<Unit> RefreshEmployeeHead = new();
        public static Subject<Unit> IAPStatusChanged = new();
        public static Subject<Unit> DailyShopItemChanged = new();
        public static Subject<Unit> RefreshGrowthPage = new();
        public static Subject<Unit> ChangeDecPlayAnimationFinished = new();
        public static Subject<Unit> DecorationEditorUIState = new();
    }
}