using System.Collections.Generic;
using Android.Media;
using Android.Speech.Tts;
using Java.Util;
using Xamarin.Forms;

namespace VocalEyes.Droid.Common.Helper
{
    public class TextToSpeechHelper: Java.Lang.Object, TextToSpeech.IOnInitListener
    {
        TextToSpeech _speaker;
        string _toSpeak;

        public void Speak(string text)
        {
            var ctx = Forms.Context;
            _toSpeak = text;
            if (_speaker == null)
            {

                _speaker = new TextToSpeech(ctx, this);
                _speaker.SetLanguage(App.User.Language == "en" ? Locale.English : new Locale("nl"));
            }
            else
            {
                var p = new Dictionary<string, string>();
                _speaker.Speak(_toSpeak, QueueMode.Flush, p);
            }
        }

        public void CancelSpeak()
        {
            if (_speaker.IsSpeaking)
                _speaker.Stop();
        }

        public void PlayBeep()
        {
            new ToneGenerator(Stream.Notification, 100).StartTone(Tone.PropBeep, 200);
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