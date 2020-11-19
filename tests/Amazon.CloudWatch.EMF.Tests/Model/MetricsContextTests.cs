using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class MetricsContextTests
    {
        [Fact]
        public void Test_Serialize_WithLessThan100Metrics()
        {
            var metricsContext = new MetricsContext();
            AddMetrics(metricsContext, 10);
            var serializedList = metricsContext.Serialize();
            Assert.Single(serializedList);
            ParseMetrics(serializedList[0]);
        }
        
        [Fact]
        public void Test_Serialize_WithMoreThan100Metrics()
        {
            var metricsContext = new MetricsContext();
            AddMetrics(metricsContext, 220);
            var serializedList = metricsContext.Serialize();
            Assert.Equal(3, serializedList.Count);
            
        }

        private void AddMetrics(MetricsContext metricsContext, int numMetrics)
        {
            for (int i = 0; i < numMetrics; i++)
            {
                var key = "Metric-" + i;
                metricsContext.PutMetric(key, i);
            }
        }

        private void ParseMetrics(string serializedInput)
        {
            var rootNode = JsonConvert.DeserializeObject<RootNode>(serializedInput);
        }

    }
}