namespace GazeToSpeech.Common.Enumeration
{
    /// <summary>
    /// Current configuration of pupil tracking. Are we setting the character-subset, the current letter or 
    /// are we ending entering an entire word
    /// </summary>
    public enum CaptureMethod
    {
        Subset,
        Character,
        Word
    }
}