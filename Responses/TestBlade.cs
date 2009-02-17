using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TradeLink.Common;
using TradeLink.API;

namespace Responses
{
    [TestFixture]
    public class TestBlade
    {
        public TestBlade() { }


        [Test]
        public void Basics()
        {
            const string sym = "TST";
            const int d = 20080509;
            const int t = 935;
            const string x = "NYSE";
            TickImpl[] ticklist = new TickImpl[] { 
                TickImpl.NewTrade(sym,d,t,0,10,100,x),
                TickImpl.NewTrade(sym,d,t+1,0,10,100,x),
                TickImpl.NewTrade(sym,d,t+2,0,10,100,x),
                TickImpl.NewTrade(sym,d,t+3,0,10,100,x),
                TickImpl.NewTrade(sym,d,t+4,0,15,100,x), // blade up
                TickImpl.NewTrade(sym,d,t+5,0,16,100,x), // new bar (blades reset)
                TickImpl.NewTrade(sym,d,t+6,0,16,100,x),
                TickImpl.NewTrade(sym,d,t+7,0,10,100,x), // blade down
                TickImpl.NewTrade(sym,d,t+7,10,10,100,x), // still a blade down (same bar)
                TickImpl.NewTrade(sym,d,t+8,0,15,100,x), 
                TickImpl.NewTrade(sym,d,t+15,0,15,800,x), // volume spike
                TickImpl.NewTrade(sym,d,t+20,0,15,100,x), 
                TickImpl.NewTrade(sym,d,t+25,0,15,100,x), 
            };

            BarListImpl bl = new BarListImpl(BarInterval.FiveMin,sym);
            Blade b = new Blade();
            Assert.That(b.BladePercentage != 0);
            b = new Blade(.2m); // 20 percent move is a blade
            int up=0,down=0,newbar=0,bigvol=0;

            foreach (TickImpl k in ticklist)
            {
                bl.AddTick(k);
                b.newBar(bl);
                if (bl.NewBar) newbar++;
                if (b.isBladeUP) up++;
                if (b.isBladeDOWN) down++;
                if (b.isBigVolume) bigvol++;
            }

            Assert.That(up == 1,up.ToString());
            Assert.That(down == 2,down.ToString());
            Assert.That(newbar == 5,newbar.ToString());
            Assert.That(bigvol == 1,bigvol.ToString());

        }

        [Test]
        public void QuoteOnlyTest()
        {
            TickImpl[] timesales = new TickImpl[] {
                TickImpl.NewBid("TST",100m,100),
                TickImpl.NewAsk("TST",100.1m,200),
            };

            Blade b = new Blade();
            BarListImpl bl = new BarListImpl(BarInterval.FiveMin,"TST");

            foreach (TickImpl k in timesales)
            {
                bl.newTick(k);
                b.newBar(bl);
            }

            // average volume should be zero bc
            // with only quotes we should have no bars to process
            Assert.That(b.AvgVol(bl) == 0, b.AvgVol(bl).ToString());
            Assert.That(!bl.Has(1), bl.ToString());
        }
    }
}
