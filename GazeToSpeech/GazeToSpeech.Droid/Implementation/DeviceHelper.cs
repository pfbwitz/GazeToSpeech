using System.Collections.Generic;
using GazeToSpeech.Common.Interface;
using GazeToSpeech.Droid.Common.Helper;
using GazeToSpeech.Droid.Implementation;
using GazeToSpeech.Model;
using Xamarin.Forms;


[assembly: Dependency(typeof(DeviceHelper))]
namespace GazeToSpeech.Droid.Implementation
{
    public class DeviceHelper : IDeviceHelper
    {
        public string GetVersion()
        {
            try
            {
                var appVersion =
                    Forms.Context.ApplicationContext.PackageManager.GetPackageInfo(
                        Forms.Context.ApplicationContext.PackageName, 0).VersionName;

                return appVersion;
            }
            catch
            {
                return "0.0";
            }
        }

        public List<Language> GetAvailableLanguages()
        {
            return new TextToSpeechHelper(Forms.Context).GetAvailableLanguages();
        }
    }
}