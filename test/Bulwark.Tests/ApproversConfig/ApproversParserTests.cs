using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bulwark.Strategy.ApproversConfig;
using Bulwark.Strategy.ApproversConfig.Impl;
using Xunit;

#pragma warning disable xUnit2000

namespace Bulwark.Tests.ApproversConfig
{
    public class ApproversParserTests
    {
        IApproversParser _approversParser;

        public ApproversParserTests()
        {
            _approversParser = new ApproversParser();
        }

        [Fact]
        public async Task Can_parse_config()
        {
            var content = new StringBuilder();
            content.AppendLine("Approvers: pknopf, mreents");
            content.AppendLine("AppendedApprovers: sk");

            var approvers = await _approversParser.ParseConfig(content.ToString());

            Assert.Equal(2, approvers.Approvers.Count);
            Assert.Equal(approvers.Approvers.ElementAt(0), "pknopf");
            Assert.Equal(approvers.Approvers.ElementAt(1), "mreents");
            Assert.Equal(approvers.AppendedApprovers.Count, 1);
            Assert.Equal(approvers.AppendedApprovers.ElementAt(0), "sk");
        }

        [Fact]
        public async Task Can_parse_config_with_null_values()
        {
            var content = new StringBuilder();
            content.AppendLine("AppendedApprovers: sk");

            var approvers = await _approversParser.ParseConfig(content.ToString());

            Assert.Null(approvers.Approvers);
            Assert.Equal(approvers.AppendedApprovers.Count, 1);
            Assert.Equal(approvers.AppendedApprovers.ElementAt(0), "sk");

            content = new StringBuilder();
            content.AppendLine("Approvers: pknopf, mreents");

            approvers = await _approversParser.ParseConfig(content.ToString());

            Assert.Equal(approvers.Approvers.Count, 2);
            Assert.Equal(approvers.Approvers.ElementAt(0), "pknopf");
            Assert.Equal(approvers.Approvers.ElementAt(1), "mreents");
            Assert.Null(approvers.AppendedApprovers);
        }
    }
}