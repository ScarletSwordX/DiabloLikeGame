namespace Gameplay.Input
{
    /// <summary>
    /// 与 GameInput.inputactions 中 Gameplay 动作名一致（改键只改 .inputactions）。
    /// </summary>
    public static class GameInputActions
    {
        public const string MapGameplay = "Gameplay";

        public const string Move = "Move";
        public const string Attack = "Attack";
        public const string Skill1 = "Skill1";
        public const string Skill2 = "Skill2";
        public const string Skill3 = "Skill3";
        public const string Item1 = "Item1";
        public const string Item2 = "Item2";
        public const string Item3 = "Item3";

        public static readonly string[] ButtonActions =
        {
            Attack, Skill1, Skill2, Skill3, Item1, Item2, Item3
        };
    }
}
