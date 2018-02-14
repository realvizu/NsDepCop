using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.Build.Framework;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    public class MsBuildLoggerGatewayTests
    {
        private readonly Mock<IBuildEngine> _buildEngineMock = new Mock<IBuildEngine>();

        [Fact]
        public void LogTraceMessage_Success()
        {
            CreateLogger().LogTraceMessage(new[] { "aaa", "bbb" });

            VerifyLogMessageEvent("aaa", MessageImportance.Low);
            VerifyLogMessageEvent("bbb", MessageImportance.Low);
        }

        [Fact]
        public void LogIssue_Error_Success()
        {
            var issue = new IssueDescriptor("MyCode", IssueKind.Error, "MyDescription");

            CreateLogger().LogIssue(issue);

            VerifyLogErrorEvent("MyCode", "MyDescription");
        }

        [Fact]
        public void LogIssue_Warning_Success()
        {
            var issue = new IssueDescriptor("MyCode", IssueKind.Warning, "MyDescription");

            CreateLogger().LogIssue(issue);

            VerifyLogWarningEvent("MyCode", "MyDescription");
        }

        [Fact]
        public void LogIssue_Info_Success()
        {
            var issue = new IssueDescriptor("MyCode", IssueKind.Info, "MyDescription");

            CreateLogger().LogIssue(issue);

            VerifyLogMessageEvent("MyDescription", MessageImportance.Normal);
        }

        [Fact]
        public void LogIssue_Info_HighImportance_Success()
        {
            var issue = new IssueDescriptor("MyCode", IssueKind.Info, "MyDescription");

            var logger = CreateLogger();
            logger.InfoImportance = MessageImportance.High;
            logger.LogIssue(issue);

            VerifyLogMessageEvent("MyDescription", MessageImportance.High);
        }

        [Fact]
        public void LogIssueT_Success()
        {
            var issue = new IssueDescriptor<string>("MyCode", IssueKind.Info, "MyDescription", i => i);

            CreateLogger().LogIssue(issue, "MyParameter");

            VerifyLogMessageEvent("MyParameter", MessageImportance.Normal);
        }

        [Fact]
        public void ExceedingMaxWarningErrorThresholdCausesIssueToBeLogged()
        {

            //Given a logger
            var msBuildLoggerGateway = CreateLogger();
            //And that logger has a maxWarningErrorThreshold
            msBuildLoggerGateway.SetMaxWarningErrorThreshold(0);
            //When there have been more warnings logged than the maxWarningErrorThreshold
            var issue = new IssueDescriptor<string>("MyCode", IssueKind.Warning, "MyDescription", i => i);
            msBuildLoggerGateway.LogIssue(issue, "MyParameter");
            //Then an error should be logged
            VerifyLogErrorEvent(IssueDefinitions.MaxWarningErrorThresholdExceededIssue.Id, "MaxWarningErrorThreshold has been exceeded");
        }


        private MsBuildLoggerGateway CreateLogger() => new MsBuildLoggerGateway(_buildEngineMock.Object);

        private void VerifyLogMessageEvent(string message, MessageImportance messageImportance) =>
            _buildEngineMock.Verify(
                i => i.LogMessageEvent(It.Is<BuildMessageEventArgs>(e => e.Message.Contains(message) && e.Importance == messageImportance)));

        private void VerifyLogWarningEvent(string code, string message) =>
            _buildEngineMock.Verify(i => i.LogWarningEvent(It.Is<BuildWarningEventArgs>(e => e.Code == code && e.Message.Contains(message))));

        private void VerifyLogErrorEvent(string code, string message) =>
            _buildEngineMock.Verify(i => i.LogErrorEvent(It.Is<BuildErrorEventArgs>(e => e.Code == code && e.Message.Contains(message))));
    }
}
