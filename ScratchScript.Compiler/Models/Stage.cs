namespace ScratchScript.Compiler.Models;

public record Stage : Target
{
    public int Tempo = 60;
    public string TextToSpeechLanguage;
    public string VideoState = "on";
    public int VideoTransparency = 50;
}