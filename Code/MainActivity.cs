using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.Button;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace RadioPlayer.Code
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : AppCompatActivity
    {
        private int notificationId = 0;
        private string notificationChannel;
        private int controlId = 0;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private SeekBar audioSlider;
        private AudioManager audioManager;
        private Intent radioService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            audioSlider = FindViewById<SeekBar>(Resource.Id.audio_slider);
            audioManager = (AudioManager)GetSystemService(AudioService);

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            radioService = new Intent(this, typeof(RadioService));

            CreateNotificationChannel();
            LoadButtons();
        }

        protected override void OnResume()
        {
            base.OnResume();

            audioSlider.Max = audioManager.GetStreamMaxVolume(Stream.Music);
            audioSlider.Progress = audioManager.GetStreamVolume(Stream.Music);
            audioSlider.ProgressChanged += AudioSlider_ProgressChanged;
        }

        protected override void OnPause()
        {
            base.OnPause();

            audioSlider.ProgressChanged -= AudioSlider_ProgressChanged;
        }

        private void AudioSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            audioManager.SetStreamVolume(Stream.Music, e.Progress, 0);
        }

        private void CreateRadioLayout(string text, Func<Task<string>> urlCallback)
        {
            var content = FindViewById<LinearLayout>(Resource.Id.layout_content);
            var layout = (LinearLayout)LayoutInflater.From(this).Inflate(Resource.Layout.content_radio, null);

            var button = (RadioMaterialButton)layout.GetChildAt(0);
            button.Id = ++controlId;
            button.Icon = GetPlayerIcon(RadioService.Id == button.Id ? false : true);
            button.GetRadioUrlAsync = urlCallback;
            button.Click += (s, e) => ButtonRadioClick(button, text);

            var tv = (TextView)layout.GetChildAt(1);
            tv.Id = ++controlId;
            tv.Text = text;

            content.AddView(layout);
        }

        private void LoadButtons()
        {
            CreateRadioLayout("Радио Дача Ростов", () => Radio.GetRadioDachaUrl(602, 128));
            CreateRadioLayout("Радио Дача Краснодар", () => Radio.GetRadioDachaUrl(32711, 64));
            CreateRadioLayout("Радио Дача СПб", () => Radio.GetRadioDachaUrl(3375, 128));
            CreateRadioLayout("Радио Дача Москва", () => Radio.GetRadioDachaUrl(1093, 64));
            CreateRadioLayout("Радио Дача Екатеринбург", () => Radio.GetRadioDachaUrl(311, 128));
            CreateRadioLayout("Радио Дача Красноярск", () => Radio.GetRadioDachaUrl(315, 128));
            CreateRadioLayout("Радио Дача Новосибирск", () => Radio.GetRadioDachaUrl(316, 128));
            CreateRadioLayout("Радио Дача Самара", () => Radio.GetRadioDachaUrl(319, 128));
            CreateRadioLayout("Радио Дача Нижний Новгород", () => Radio.GetRadioDachaUrl(412, 128));
            CreateRadioLayout("Радио Дача Уфа", () => Radio.GetRadioDachaUrl(17211, 128));
            CreateRadioLayout("Радио Дача Казань", () => Radio.GetRadioDachaUrl(19495, 128));
        }
        private void ResetButtons()
        {
            var content = FindViewById<LinearLayout>(Resource.Id.layout_content);
            for (int i = 0; i < content.ChildCount; i++)
            {
                var layout = (LinearLayout)content.GetChildAt(i);
                var button = (RadioMaterialButton)layout.GetChildAt(0);
                button.Icon = GetPlayerIcon(true);
            }
        }

        private void CreateNotificationChannel()
        {
            notificationChannel = Resources.GetString(Resource.String.app_name);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(notificationChannel, notificationChannel, NotificationImportance.Default);
                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }

        private void ShowNotification(string text)
        {
            var intent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 0, intent, PendingIntentFlags.UpdateCurrent);

            var builder = new NotificationCompat.Builder(this, notificationChannel)
                .SetContentText(text)
                .SetContentIntent(pendingIntent)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetSmallIcon(Resource.Mipmap.ic_notification)
                .SetAutoCancel(false)
                .SetOngoing(true)
                .SetShowWhen(false);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(notificationId, builder.Build());
        }

        private void HideNotification()
        {
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Cancel(notificationId);
        }

        private void ShowDialog(string title, string message)
        {
            var alert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            alert.SetTitle(title);
            alert.SetMessage(message);
            alert.SetPositiveButton("OK", (s, e) => { });
            alert.Create().Show();
        }

        private void DisplayText(string text)
        {
            Toast.MakeText(this, text, ToastLength.Long).Show();
        }

        private Drawable GetPlayerIcon(bool play)
        {
            var id = play ? Resource.Drawable.icon_play : Resource.Drawable.icon_pause;
            return ContextCompat.GetDrawable(this, id);
        }

#pragma warning disable 612, 618
        private ProgressDialog ShowProgress(string text)
        {
            var span = new SpannableString(text);
            span.SetSpan(new RelativeSizeSpan(1.5f), 0, text.Length, 0);

            var progress = new ProgressDialog(this);
            progress.SetMessage(span);
            progress.Show();

            return progress;            
        }
#pragma warning restore 612, 618

        private async Task StartRadioAsync(string url, MaterialButton button, string notification)
        {
            var player = new RadioPlayer(this, url);
            await player.StartAsync();

            RadioService.SetPlayer(player, button.Id);
            StartService(radioService);

            button.Icon = GetPlayerIcon(false);
            ShowNotification(notification);
        }

        private void StopRadio()
        {
            RadioService.SetPlayer(null, -1);
            StopService(radioService);

            ResetButtons();
            HideNotification();
        }

        private async void ButtonRadioClick(RadioMaterialButton button, string name)
        {
            await semaphore.WaitAsync();
            var progress = ShowProgress("Working on it...");

            try
            {
                var radioId = RadioService.Id;
                if (radioId >= 0)
                    StopRadio();

                if (radioId < 0 || radioId != button.Id)
                {
                    var url = await button.GetRadioUrlAsync();
                    await StartRadioAsync(url, button, name);
                }
            }
            catch (Exception ex)
            {
                StopRadio();
                DisplayText(ex.Message);
            }
            finally
            {
                progress.Hide();
                semaphore.Release();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_about:
                    {
                        ShowDialog("About", "RadioPlayer v1.1 by depler.\nAll rights is not reserved.\nCopyleft ® 2022.");
                        return true;
                    }
                default: return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
