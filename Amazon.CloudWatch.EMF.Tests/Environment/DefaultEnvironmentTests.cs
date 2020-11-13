using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class DefaultEnvironmentTests
    {
        private readonly IFixture _fixture;
        private IConfiguration _configuration;
        private DefaultEnvironment _environment;
        public DefaultEnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _configuration = _fixture.Create<IConfiguration>();
            _environment = new DefaultEnvironment(_configuration);
        }
        
        [Fact]
        public void Test_Name_Configuration_NotSet()
        {
            Assert.False(string.IsNullOrWhiteSpace(_environment.Name));
        }
        

        [Fact]
        public void Test_Type_Configuration_NotSet()
        {
            Assert.False(string.IsNullOrWhiteSpace( _environment.Type));
        }

        [Fact]
        public void Test_LogStreamName_Configuration_Set()
        {
            var logStreamName = "TestServiceType";
            _configuration.LogStreamName.Returns(logStreamName);

            Assert.Equal(logStreamName, _environment.LogStreamName);
        }

        [Fact]
        public void Test_LogStreamName_Configuration_NotSet()
        {
            Assert.Equal(string.Empty, _environment.LogStreamName);
        }

        [Fact]
        public void Test_LogGroupName_Configuration_NotSet()
        {
            Assert.False(string.IsNullOrWhiteSpace(_environment.LogGroupName));
        }

        [Fact]
        public void Test_Probe()
        {
            var result = _environment.Probe();
            Assert.True(result);
        }
    }
}