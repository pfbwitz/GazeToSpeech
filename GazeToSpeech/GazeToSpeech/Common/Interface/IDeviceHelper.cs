using System.Collections.Generic;
using GazeToSpeech.Model;

namespace GazeToSpeech.Common.Interface
{
    public interface IDeviceHelper
    {
        string GetVersion();

        List<Language> GetAvailableLanguages();
    }
}
