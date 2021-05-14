using Android.App;
using Android.Content;
using Android.OS;

namespace RadioPlayer.Code
{
    [Service]
    public class RadioService : Service
    {
        public static int Id { get; private set; } = -1;
        public static RadioPlayer Player { get; private set; } = null;

        public static void SetPlayer(RadioPlayer player, int id)
        {
            if (Player != null)
            {
                try { Player.Dispose(); }
                catch { }
            }

            Player = player;
            Id = id;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.NotSticky; 
        }
    }
}