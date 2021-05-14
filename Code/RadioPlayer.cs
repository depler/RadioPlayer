using Android.Content;
using Android.Webkit;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Upstream;
using System;
using System.Threading.Tasks;

namespace RadioPlayer.Code
{
    public class RadioPlayer: IDisposable
    {
        private string sourceUrl = null;
        private SimpleExoPlayer exoPlayer = null;

        public bool IsPlaying => exoPlayer.IsPlaying;

        public RadioPlayer(Context context, string url)
        {
            sourceUrl = url;

#pragma warning disable 612, 618
            var agent = WebSettings.GetDefaultUserAgent(context);
            var factory = new DefaultDataSourceFactory(context, agent);
            var extractorMediaSource = new ExtractorMediaSource(Android.Net.Uri.Parse(url), factory, new DefaultExtractorsFactory(), null, null);

            exoPlayer = ExoPlayerFactory.NewSimpleInstance(context);            
            exoPlayer.Prepare(extractorMediaSource);
#pragma warning restore 612, 618
        }

        public async Task StartAsync()
        {
            exoPlayer.PlayWhenReady = true;

            while (true)
            {
                if (exoPlayer.IsPlaying)
                    return;

                if (exoPlayer.PlaybackError != null)
                    throw new Exception($"Cannot play source url: {sourceUrl}");

                await Task.Delay(100);
            }            
        }

        public void Dispose()
        {
            exoPlayer.PlayWhenReady = false;
            exoPlayer.Dispose();
        }
    }
}