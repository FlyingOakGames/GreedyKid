
namespace GreedyKid.Helper
{
    public enum Achievement : int
    {
        GD_ACHIEVEMENT_1,
        GD_ACHIEVEMENT_2,

        COUNT,
    };

    public sealed class AchievementHelper
    {
        private static AchievementHelper _instance;

        public static AchievementHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AchievementHelper();
                return _instance;
            }
        }

        private AchievementHelper()
        {

        }

        public static void UnlockAchievement(Achievement id)
        {
#if DESKTOP
            SteamworksHelper.Instance.SetAchievement(id);
#elif XBOXONE
            PlatformHelper.XboxOne.UnlockAchievement((int)id);
#elif PLAYSTATION4
            PlatformHelper.PlayStation4.UnlockAchievement((int)id);
#elif PSVITA
            PlatformHelper.PSVita.UnlockAchievement((int)id - 3);
#endif
        }
    }
}
