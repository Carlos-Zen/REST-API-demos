using Huobi.Rest.CSharp.Demo;
using Huobi.Rest.CSharp.Demo.Model;
using NUnit.Framework;
using System;
using System.Linq;

namespace Huobi.Rest.CSharp.Demo.Tests
{
    [TestFixture]
    public class HuobiApiTests
    {
        static string privateKeyStr = "MIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQgh+agFMvSAHa37NmhRq8VduM18VtJ/un5aecroYIDzLqhRANCAAQDnfHLp1UAFDUGRwtMGYSIycwcaO+Dbfn+pFXi1dyb4Oz7/EC6EPTAAu2f4lB+6ujWkJgF6i+0p3OF8XLLJ2s4";
        HuobiApi api = new HuobiApi("40155c14-f2aeaae7-190e875f-3114e", "58727598-1878f193-22d79deb-7df00", privateKeyStr);
       
        [Test]
        public void GetAllAccountTest()
        {
            var result = api.GetAllAccount();

            Assert.IsNotNull(result);
        }

        [Test]
        public void OrderPlaceTest()
        {
            var accounts = api.GetAllAccount();
            var spotAccountId = accounts.FirstOrDefault(a => a.Type == "spot" && a.State == "working")?.Id;
            if (spotAccountId <= 0)
                throw new ArgumentException("spot account unavailable");
            OrderPlaceRequest req = new OrderPlaceRequest();
            req.account_id = spotAccountId.ToString();
            req.amount = "0.1";
            req.price = "0.8";
            req.source = "api";
            req.symbol = "ethusdt";
            req.type = "buy-limit";
            var result = api.OrderPlace(req);
            Assert.AreEqual("ok", result.Status);
        }
    }
}