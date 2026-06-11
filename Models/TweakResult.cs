namespace NitroOptimizer.Models
{
    public class TweakResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long FreedSpaceBytes { get; set; }
    }
}
