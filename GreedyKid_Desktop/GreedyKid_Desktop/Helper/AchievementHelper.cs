
namespace GreedyKid.Helper
{
    public enum Achievement : int
    {
        GD_ACHIEVEMENT_1, // first blood                         OK
        GD_ACHIEVEMENT_2, // boo 200x                            OK
        GD_ACHIEVEMENT_3, // finish level                        OK
        GD_ACHIEVEMENT_4, // finish 1/3 levels                   OK
        GD_ACHIEVEMENT_5, // finish 2/3 levels                   OK
        GD_ACHIEVEMENT_6, // finish 3/3 levels                   OK
        GD_ACHIEVEMENT_7, // get 1/3 stars                       OK
        GD_ACHIEVEMENT_8, // get 2/3 stars                       OK
        GD_ACHIEVEMENT_9, // get 3/3 stars                       OK
        GD_ACHIEVEMENT_10, // hide 100x                          OK
        GD_ACHIEVEMENT_11, // roll 100x                          OK
        GD_ACHIEVEMENT_12, // KO nurse                           OK
        GD_ACHIEVEMENT_13, // show your butt and be seen         OK
        GD_ACHIEVEMENT_14, // get tased                          OK
        GD_ACHIEVEMENT_15, // get robot-copped                   OK
        GD_ACHIEVEMENT_16, // slam door on someone               OK
        GD_ACHIEVEMENT_17, // roll slam door                     OK
        GD_ACHIEVEMENT_18, // get $5000                          OK
        GD_ACHIEVEMENT_19, // Finished level with 1HP            OK
        GD_ACHIEVEMENT_20, // boo a SWAT                         OK

        COUNT,
    };

    public sealed class AchievementHelper
    {
        public const uint MoneyThreshold = 5000;
        public const uint BooThreshold = 200;
        public const uint HideThreshold = 100;
        public const uint RollThreshold = 100;

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

        public void UnlockAchievement(Achievement id)
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
