using GazeToSpeech.Common.Interface;
using GazeToSpeech.Droid.Implementation;
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
    }
}