using System;
using Codartis.NsDepCop.Core.Util;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Util
{
    public class RetryHelperTests
    {
        [Theory]
        [InlineData(3, 0, 1)]
        [InlineData(3, 1, 2)]
        [InlineData(3, 3, 4)]
        public void Retry_Succeeds(int maxRetryCount, int presetFailureCount, int expectedTryCount)
        {
            var testSubject = new RetryTestSubject(presetFailureCount);

            var result = RetryHelper.Retry(
                () => testSubject.IncrementTryCountAndThrow(),
                maxRetryCount,
                e => testSubject.OnFailure(e));

            result.AsT0.Should().Be(expectedTryCount);
            testSubject.TryCount.Should().Be(expectedTryCount);
            testSubject.FailureCount.Should().Be(presetFailureCount);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(3, 4)]
        [InlineData(3, 10)]
        public void Retry_Fails(int maxRetryCount, int presetFailureCount)
        {
            var testSubject = new RetryTestSubject(presetFailureCount);

            var result = RetryHelper.Retry(
                () => testSubject.IncrementTryCountAndThrow(),
                maxRetryCount,
                e => testSubject.OnFailure(e));

            result.AsT1.Should().NotBeNull();
            testSubject.TryCount.Should().Be(maxRetryCount + 1);
            testSubject.FailureCount.Should().Be(maxRetryCount);
        }

        private class RetryTestSubject
        {
            public int PresetFailureCount { get; }
            public int TryCount { get; private set; }
            public int FailureCount { get; private set; }

            public RetryTestSubject(int presetFailureCount)
            {
                PresetFailureCount = presetFailureCount;
                TryCount = 0;
            }

            public int IncrementTryCountAndThrow()
            {
                TryCount++;

                if (TryCount < PresetFailureCount + 1)
                    throw new Exception($"Failed invocation #{TryCount}");

                return TryCount;
            }

            public void OnFailure(Exception e)
            {
                e.Should().NotBeNull();
                FailureCount++;
            }
        }
    }
}
