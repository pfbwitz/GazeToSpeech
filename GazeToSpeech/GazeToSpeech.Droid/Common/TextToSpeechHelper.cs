using Android.Speech.Tts;
using Xamarin.Forms;
using System.Collections.Generic;
using Android.Content;

namespace GazeToSpeech.Droid.Common
{
    public class TextToSpeechHelper : Java.Lang.Object, TextToSpeech.IOnInitListener
    {
        TextToSpeech _speaker;
        private readonly Context _context;
        string _toSpeak;

        public TextToSpeechHelper(Context context)
        {
            _context = context;
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
            }
        }

        #region IOnInitListener implementation
        public void OnInit(OperationResult status)
        {
            if (status.Equals(OperationResult.Success))
            {
                var p = new Dictionary<string, string>();
                _speaker.Speak(_toSpeak, QueueMode.Flush, p);
            }
        }
        #endregion
    }
}