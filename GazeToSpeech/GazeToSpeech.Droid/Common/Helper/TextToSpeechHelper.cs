using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Speech.Tts;
using GazeToSpeech.Model;

namespace GazeToSpeech.Droid.Common.Helper
{
    public class TextToSpeechHelper : Java.Lang.Object, TextToSpeech.IOnInitListener
    {
        TextToSpeech _speaker;
        private readonly CaptureActivity _context;
        string _toSpeak;

        public TextToSpeechHelper(CaptureActivity context)
        {
            _context = context;
        }

        public bool IsSpeaking
        {
            get
            {
                return _speaker.IsSpeaking;
            }
        }

        public TextToSpeechHelper(Context context)
        {
            _speaker = new TextToSpeech(context, this);
        }

        public List<Language> GetAvailableLanguages()
        {
           var locales =  Java.Text.BreakIterator.GetAvailableLocales().Select(l => new Language{Code = l.ISO3Language, Name = l.DisplayLanguage});
        return locales.ToList();
        }

        public void Speak(string text)
        {

            _toSpeak = text;
            if (_speaker == null)
            {
                 _speaker = new TextToSpeech(_context, this);
                 
            }
            else
            {
                var p = new Dictionary<string, string>();
                _speaker.Speak(_toSpeak, QueueMode.Flush, p);
                Task.Run(() =>
                {
                    while (true)
                    {
                        _context.Speaking = _speaker.IsSpeaking;
                        if (!_speaker.IsSpeaking)
                            break;
                    }
                });
            }
        }

        #region IOnInitListener implementation
        public void OnInit(OperationResult status)
        {
            if (status.Equals(OperationResult.Success))
            {
                //_speaker.SetOnUtteranceProgressListener(new MyUtteranceProgressListener(_context));
            }
        }
        #endregion
    }

    //public class MyUtteranceProgressListener : UtteranceProgressListener
    //{
    //    private readonly CaptureActivity _activity;

    //    public MyUtteranceProgressListener(CaptureActivity activity)
    //    {
    //        _activity = activity;
    //    }

    //    public override void OnDone(string utteranceId)
    //    {
    //        _activity.Speaking = false;
    //    }

    //    public override void OnError(string utteranceId)
    //    {
    //        _activity.Speaking = false;
    //    }

    //    public override void OnStart(string utteranceId)
    //    {
    //        _activity.Speaking = true;
    //    }
    //}
}