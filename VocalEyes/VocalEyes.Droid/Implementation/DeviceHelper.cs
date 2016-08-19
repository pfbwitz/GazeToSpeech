using System.Collections.Generic;
using VocalEyes.Common.Interface;
using VocalEyes.Droid.Common.Helper;
using VocalEyes.Droid.Implementation;
using VocalEyes.Model;
using Xamarin.Forms;


[assembly: Dependency(typeof(DeviceHelper))]
namespace VocalEyes.Droid.Implementation
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