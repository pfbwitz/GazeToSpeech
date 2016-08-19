using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Speech.Tts;
using VocalEyes.Droid.Activities;
using VocalEyes.Model;

namespace VocalEyes.Droid.Common.Helper
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

        public bool IsSpeaking{get{ return _speaker.IsSpeaking;}}

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
                 _speaker = new TextToSpeech(_context, this);
            else
            {
                var p = new Dictionary<string, string>();
                _speaker.Speak(_toSpeak, QueueMode.Flush, p);
                Task.Run(() =>
                {
                    while (true)
                    {
                        _context.Speaking = IsSpeaking;
                        if (!IsSpeaking)
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
}