using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple.Tests
{
    [TestClass()]
    public class TradeProcessorTests
    {
        private int CountDbRecords()
        {
            using (var connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\ghrabik\Documents\tradedatabase.mdf;Integrated Security=True;Connect Timeout=30;"))
            //using (var connection = new System.Data.SqlClient.SqlConnection("Data Source=(local);Initial Catalog=TradeDatabase;Integrated Security=True;"))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string myScalarQuery = "SELECT COUNT(*) FROM trade";
                SqlCommand myCommand = new SqlCommand(myScalarQuery, connection);
                myCommand.Connection.Open();
                int count = (int)myCommand.ExecuteScalar();
                connection.Close();
                return count;
            }
        }



        [TestMethod]
        public void ProcessTrades_WithSingleValidTrade_AddsOneRecord()
        {
            // Arrange
            var tradeStream = CreateStreamFromText("GBPUSD,1000,1.51");
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            // Assert
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void ProcessTrades_WithMultipleValidTrades_AddsCorrectRecords()
        {
            var tradeStream = CreateStreamFromText("GBPUSD,1000,1.51\nEURUSD,2000,1.25");
            var tradeProcessor = new TradeProcessor();

            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            Assert.AreEqual(countBefore + 2, countAfter);
        }

        [TestMethod]
        public void ProcessTrades_WithEmptyFile_AddsNoRecords()
        {
            var tradeStream = CreateStreamFromText("");
            var tradeProcessor = new TradeProcessor();

            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ProcessTrades_WithNonExistentFile_ThrowsException()
        {
            // Simulate a non-existent file stream.
            using (var stream = new FileStream("nonexistent.txt", FileMode.Open))
            {
                var tradeProcessor = new TradeProcessor();
                tradeProcessor.ProcessTrades(stream);
            }
        }

        [TestMethod]
        public void ReadTradeData_WithMalformedTrade_ReturnsNoLines()
        {
            var tradeStream = CreateStreamFromText("GBPUSD,1000"); // Missing price
            var tradeProcessor = new TradeProcessor();

            var lines = tradeProcessor.ReadTradeData(tradeStream);

            Assert.AreEqual(0, lines.Count());
        }

        [TestMethod]
        public void ReadTradeData_WithNegativeLotSize_ReturnsNoLines()
        {
            var tradeStream = CreateStreamFromText("GBPUSD,-1000,1.51");
            var tradeProcessor = new TradeProcessor();

            var lines = tradeProcessor.ReadTradeData(tradeStream);

            Assert.AreEqual(0, lines.Count());
        }

        // Helper function to create a Stream from a text string.
        private Stream CreateStreamFromText(string text)
        {
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        }
    }
}

[TestMethod()]
        public void TestNormalFile()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.goodtrades.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 4, countAfter);
        }
    }

}