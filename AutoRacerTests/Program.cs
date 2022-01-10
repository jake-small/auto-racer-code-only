using System;
using NUnit.Framework;

namespace AutoRacerTests
{
  [TestFixture]
  public class PrimeService_IsPrimeShould
  {
    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void IsPrime_InputIs1_ReturnFalse()
    {
      Assert.Pass();
    }
  }
}
