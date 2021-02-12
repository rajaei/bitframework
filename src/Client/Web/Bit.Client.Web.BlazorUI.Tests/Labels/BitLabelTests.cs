﻿using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bit.Client.Web.BlazorUI.Tests;

namespace Bit.Client.Web.BlazorUI.Tests.Labels
{
    [TestClass]
    public class BitLabelTests: BunitTestContext
    {
        [DataTestMethod, DataRow(true,true), DataRow(false,false)]
        public void BitLabelShouldRespectIsRequired(bool isRequired, bool expectedResult)
        {
            var component = RenderComponent<BitLabelTest>(parameters => parameters.Add(p => p.IsRequired, isRequired));
            var bitLabel = component.Find(".bit-label");
            Assert.AreEqual(expectedResult , bitLabel.ClassList.Contains("required"));
        }
    }
}