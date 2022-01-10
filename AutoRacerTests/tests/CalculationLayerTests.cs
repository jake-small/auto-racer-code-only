using System;
using NUnit.Framework;

namespace AutoRacerTests.Tests
{
  [TestFixture]
  public class CalculationLayerTests
  {
    private CalculationLayer _calcLayer = new CalculationLayer();

    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void ApplyLevelValues_All_Success()
    {
      var card = GetTestCard();
      Assert.That(card.GetRawName(), Is.EqualTo("TestCard"));
    }

    private Card GetTestCard()
    {
      return new Card
      {
        Name = "TestCard"
      };
    }
  }
}
