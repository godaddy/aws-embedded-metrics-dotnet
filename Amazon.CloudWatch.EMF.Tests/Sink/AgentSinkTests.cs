using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class TestClient : ISocketClient 
    {
        private string _message;
        public void SendMessage(string message)
        {
            message = _message;
        }
            
        public string GetMessage() 
        {
            return _message;
        }
    }
    public class AgentSinkTests
    {
        private SocketClientFactory _socketClientFactory;
        private ISocketClient _client;
        private readonly IFixture _fixture;
        
        public AgentSinkTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _socketClientFactory = _fixture.Create<SocketClientFactory>();
            _client = new TestClient();
            _socketClientFactory.GetClient(Arg.Any<Endpoint>()).ReturnsForAnyArgs(_client);
        }
        
        [Fact]
        public void Test_Accept() 
        {
            string prop = "TestProp";
            string propValue = "TestPropValue";
            string logGroupName = "TestLogGroup";
            string logStreamName = "TestLogStream";

            var metricsContext = new MetricsContext();
            metricsContext.PutProperty(prop, propValue);
            metricsContext.PutMetric("Time", 10);

            var agentSink = new AgentSink(logGroupName, logStreamName, Endpoint.DEFAULT_TCP_ENDPOINT, _socketClientFactory);
            agentSink.Accept(metricsContext);

            TestClient testClient = (TestClient) _client;
            var emfMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(testClient.GetMessage());
            var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfMap["_aws"].ToString());

            Assert.False( metadata.ContainsKey("LogGroupName"));
            Assert.False( metadata.ContainsKey("LogStreamName"));
        }
    }
}