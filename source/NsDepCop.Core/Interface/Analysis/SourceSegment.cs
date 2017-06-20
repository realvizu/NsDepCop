namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Describes a certain segment of a source file.
    /// </summary>
    public class SourceSegment
    {
        /// <summary>
        /// The line number of the segment's start (1-based).
        /// </summary>
        public int StartLine { get; }

        /// <summary>
        /// The column number of the segment's start (1-based).
        /// </summary>
        public int StartColumn { get; }

        /// <summary>
        /// The line number of the segment's end (1-based).
        /// </summary>
        public int EndLine { get; }

        /// <summary>
        /// The column number of the segment's end (1-based).
        /// </summary>
        public int EndColumn { get; }

        /// <summary>
        /// The text of the segment.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The full path of the source file.
        /// </summary>
        public string Path { get; }

        public SourceSegment(int startLine, int startColumn, int endLine, int endColumn, string text, string path)
        {
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            Text = text;
            Path = path;
        }

        public override string ToString() => $"{Path} ({StartLine},{StartColumn},{EndLine},{EndColumn})";
    }
}
