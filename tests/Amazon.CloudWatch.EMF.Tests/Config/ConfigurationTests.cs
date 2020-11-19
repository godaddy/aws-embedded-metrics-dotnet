using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Config
{
    public class ConfigurationTests
    {
        private readonly IFixture _fixture;
        
        public ConfigurationTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }
        
        [Fact]
        public void Test_ReturnsEmpty_IfNotSet()
        {
            var config = new Configuration();
            Assert.Null(config.LogGroupName);
            Assert.Null(config.LogStreamName);
            Assert.Null(config.ServiceType);
            Assert.Null(config.ServiceName);
            Assert.Equal(Environments.Unknown, config.EnvironmentOverride);
        }
        
        [Fact]
        public void Test_ReturnsEmpty_IfStringValueIsBlank()
        {
            var config = new Configuration(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, Environments.Unknown);

            Assert.Equal("", config.AgentEndPoint);
            Assert.Equal("", config.LogGroupName);
            Assert.Equal("", config.LogStreamName);
            Assert.Equal("", config.ServiceType);
            Assert.Equal("", config.ServiceName);
            Assert.Equal(Environments.Unknown, config.EnvironmentOverride);
        }
        
        [Fact]
        public void Test_ReturnsCorrectValue_AfterSet()
        {
            string agentEndPoint = _fixture.Create<string>();
            string logGroupName = _fixture.Create<string>();
            string logStreamName = _fixture.Create<string>();
            string serviceType = _fixture.Create<string>();
            string serviceName = _fixture.Create<string>();
            Environments environment = Environments.Agent;
            var config = new Configuration(serviceName, serviceType, logGroupName, logStreamName, agentEndPoint,
                environment);

            Assert.Equal(agentEndPoint, config.AgentEndPoint);
            Assert.Equal(logGroupName, config.LogGroupName);
            Assert.Equal(logStreamName, config.LogStreamName);
            Assert.Equal(serviceType, config.ServiceType);
            Assert.Equal(serviceName, config.ServiceName);
            Assert.Equal(environment, config.EnvironmentOverride);
        }
    }
}