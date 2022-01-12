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
        private readonly string _sourceUrl;
        private readonly SimpleExoPlayer _exoPlayer;

        public bool IsPlaying => _exoPlayer.IsPlaying;

        public RadioPlayer(Context context, string url)
        {
            _sourceUrl = url;

#pragma warning disable 612, 618
            var agent = WebSettings.GetDefaultUserAgent(context);
            var factory = new DefaultDataSourceFactory(context, agent);
            var extractorMediaSource = new ExtractorMediaSource(Android.Net.Uri.Parse(url), factory, new DefaultExtractorsFactory(), null, null);

            _exoPlayer = ExoPlayerFactory.NewSimpleInstance(context);            
            _exoPlayer.Prepare(extractorMediaSource);
#pragma warning restore 612, 618
        }

        public async Task StartAsync()
        {
            _exoPlayer.PlayWhenReady = true;

            while (true)
            {
                if (_exoPlayer.IsPlaying)
                    return;

                if (_exoPlayer.PlaybackError != null)
                    throw new Exception($"Cannot play source url: {_sourceUrl}");

                await Task.Delay(100);
            }            
        }

        public void Dispose()
        {
            _exoPlayer.PlayWhenReady = false;
            _exoPlayer.Dispose();
        }
    }
}