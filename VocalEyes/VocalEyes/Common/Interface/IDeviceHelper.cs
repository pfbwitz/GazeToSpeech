using System.Collections.Generic;
using VocalEyes.Model;

namespace VocalEyes.Common.Interface
{
    public interface IDeviceHelper
    {
        string GetVersion();

        List<Language> GetAvailableLanguages();
    }
}
