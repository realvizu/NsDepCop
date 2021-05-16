using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.SourceTest
{
    public struct SourceLineSegment
    {
        public int Line { get; }
        public int StartColumn { get; }
        public int EndColumn { get; }

        public SourceLineSegment(int line, int startColumn, int endColumn)
        {
            Line = line;
            StartColumn = startColumn;
            EndColumn = endColumn;
        }

        public bool Equals(SourceSegment sourceSegment)
        {
            return Line == sourceSegment.StartLine
                && Line == sourceSegment.EndLine
                && StartColumn == sourceSegment.StartColumn
                && EndColumn == sourceSegment.EndColumn;
        }

        public override string ToString() => $"({Line},{StartColumn}-{EndColumn})";
    }
}
