namespace PlaSample
{
    /// <summary>
    /// a mirror of the underlying PLA enum describing, predictably, the format of a perf counter log file
    /// </summary>
    public enum LogFileFormat
    {
        CommaSeparated = 0,
        TabSeparated = 1,
        Sql = 2,
        Binary = 3,
    }
}
