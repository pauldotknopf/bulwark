using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bulwark.Strategy.CodeOwners;
using Bulwark.Strategy.CodeOwners.Impl;
using Xunit;

namespace Bulwark.Tests.CodeOwners
{
    public class CodeOwnersParserTests
    {
        ICodeOwnersParser _codeOwnersParser;
        
        public CodeOwnersParserTests()
        {
            _codeOwnersParser = new CodeOwnersParser();    
        }
        
        [Fact]
        public async Task Can_parse()
        {
            var content = new StringBuilder();
            content.AppendLine("test/* @user1 @user2");

            var result = await _codeOwnersParser.ParserConfig(content.ToString());

            result.Entries.Count.ShouldBeEqualTo(1);
            result.Entries[0].Pattern.ShouldBeEqualTo("test/*");
            result.Entries[0].Users.ShouldBeEqualTo(new List<string> {"user1", "user2"});
        }

        [Fact]
        public async Task Can_ignore_comments()
        {
            var content = new StringBuilder();
            content.AppendLine("# comment");
            content.AppendLine("test/* @user1 @user2");
            content.AppendLine("# another");

            var result = await _codeOwnersParser.ParserConfig(content.ToString());

            result.Entries.Count.ShouldBeEqualTo(1);
            result.Entries[0].Pattern.ShouldBeEqualTo("test/*");
            result.Entries[0].Users.ShouldBeEqualTo(new List<string> {"user1", "user2"});
        }
        
        [Fact]
        public async Task Users_can_be_sep_by_comma()
        {
            var content = new StringBuilder();
            content.AppendLine("# comment");
            content.AppendLine("test/* @user1,@user2");
            content.AppendLine("# another");

            var result = await _codeOwnersParser.ParserConfig(content.ToString());

            result.Entries.Count.ShouldBeEqualTo(1);
            result.Entries[0].Pattern.ShouldBeEqualTo("test/*");
            result.Entries[0].Users.ShouldBeEqualTo(new List<string> {"user1", "user2"});
        }
    }
}