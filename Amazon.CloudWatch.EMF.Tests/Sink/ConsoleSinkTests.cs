using System;
using System.Collections.Generic;
using System.IO;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Newtonsoft.Json;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class ConsoleOutput : IDisposable
    {
        private StringWriter _stringWriter;
        private TextWriter _originalOutput;

        public ConsoleOutput()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }

        public string GetOutput()
        {
            return _stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }
    }
    
    public class ConsoleSinkTests
    {
        private readonly IFixture _fixture;
        
        public ConsoleSinkTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }
        
        [Fact]
        public void Test_Accept() 
        {
            string prop = "TestProp";
            string propValue = "TestPropValue";

            var metricsContext = new MetricsContext();
            metricsContext.PutProperty(prop, propValue);
            metricsContext.PutMetric("Time", 10);

            var message = string.Empty;
            using (var consoleOutput = new ConsoleOutput())
            {
                var consoleSink = new ConsoleSink();
                consoleSink.Accept(metricsContext);
                message = consoleOutput.GetOutput();
            }
            
            var emfMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(message); 
            
            Assert.Equal(propValue, emfMap[prop]);
            Assert.Equal(10.0, emfMap["Time"] );
        }
        
        [Fact]
        public void Test_Accept_Throws_Exception() 
        {
            var metricsContext = _fixture.Create<MetricsContext>();
            var consoleSink = _fixture.Create<ConsoleSink>();
            metricsContext.Serialize().Throws(new Exception());

            try
            {
                consoleSink.Accept(metricsContext);
            }
            catch (Exception e)
            {
            }
        }
    }
}