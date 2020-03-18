namespace Codartis.NsDepCop.Core.Test.Interface.Remote
{
    using System;
    using System.Collections.Generic;
    using Codartis.NsDepCop.Core.Interface.Analysis.Remote.Commands;
    using Codartis.NsDepCop.Core.Util;
    using FluentAssertions;
    using Xunit;

    public class CommandTests
    {
        [Fact]
        public void Conversion_is_ok()
        {
            object x = new Command<Exception, IEnumerable<Exception>>("xx", new Exception());
            object y = x;
            bool isHandler  = y is ICommand<MessageHandler>;
            bool isObject  = y is ICommand<object>;
            bool isException  = y is ICommand<IEnumerable<object>>;

            isHandler.Should().BeFalse();
            isObject.Should().BeTrue();
            isException.Should().BeTrue();

        }

    }
}