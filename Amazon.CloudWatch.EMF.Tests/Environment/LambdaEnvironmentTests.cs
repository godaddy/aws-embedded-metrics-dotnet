using System;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LambdaEnvironmentTests
    {
        private readonly IFixture _fixture;
        private LambdaEnvironment _environment;

        public LambdaEnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _environment = new LambdaEnvironment();
        }
        
        [Fact]
        public void Test_GetName_Returns_FunctionName()
        {
            string expectedName = _fixture.Create<string>();
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.LAMBDA_FUNCTION_NAME, expectedName);
            Assert.Equal(expectedName, _environment.Name );
        }
        
        [Fact]
        public void Test_GetType_Returns_CFNLambdaName()
        {
            Assert.Equal("AWS::Lambda::Function", _environment.Type );
        }
        
        [Fact]
        public void Test_GetLogGroupName_Returns_FunctionName() 
        {
            string expectedName = _fixture.Create<string>();
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.LAMBDA_FUNCTION_NAME, expectedName);
            Assert.Equal(expectedName, _environment.LogGroupName);
        }
        
        [Fact]
        public void Test_ConfigureContext_AddProperties() 
        {
            MetricsContext metricsContext = new MetricsContext();

            string expectedEnv = _fixture.Create<string>();
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.AWS_EXECUTION_ENV, expectedEnv);
   
            string expectedVersion = _fixture.Create<string>();
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.LAMBDA_FUNCTION_VERSION, expectedVersion);
            
            string expectedLogName = _fixture.Create<string>();
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.LAMBDA_LOG_STREAM, expectedLogName);
            
            _environment.ConfigureContext(metricsContext);
            
            Assert.Equal(expectedEnv, metricsContext.GetProperty("executionEnvironment"));
            Assert.Equal(expectedVersion, metricsContext.GetProperty("functionVersion"));
            Assert.Equal(expectedLogName, metricsContext.GetProperty("logStreamId"));
            Assert.Null(metricsContext.GetProperty("traceId"));
        }
        
       [Fact]
        public void Test_Context_With_TraceId() 
        {
            var metricsContext = new MetricsContext();

            string expectedTraceId = "Sampled=1;Count=1";
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.TRACE_ID, expectedTraceId);
            
            _environment.ConfigureContext(metricsContext);
            Assert.Equal(expectedTraceId, metricsContext.GetProperty("traceId"));
        }
        
        [Fact]
        public void Test_TraceId_With_Other_SampledValue()
        {
            var metricsContext = new MetricsContext();
            
            string expectedTraceId = "Sampled=0;Count=1";
            System.Environment.SetEnvironmentVariable(LambdaEnvironment.TRACE_ID, expectedTraceId);

            _environment.ConfigureContext(metricsContext);
            Assert.Null(metricsContext.GetProperty("traceId"));
        }
        
        
        [Fact]
        public void Test_GetCreateSink_Returns_LambdaSink()
        {
            var sinkType = _environment.Sink.GetType();
            Assert.Equal(typeof(ConsoleSink), sinkType);
        }
    }
}