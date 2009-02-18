
using System;
using TradeLink.Common;
using TradeLink.API;

namespace Responses
{
    /// <summary>
    /// An example of a strategy that buys and sells Asymetries around the open.
    /// 
    /// If the open and the low are close but the high is far away, sell the open when the market reapproaches it.
    /// If the open and high and close but the low is far away, buy the open when the market reapproaches it.
    /// </summary>
	public class A : MarketResponse
	{
        public A() : base() 
        { 
            const string version = "$Rev$"; 
            Name = "A"+Util.CleanVer(version); 
        }
        BarList bars;

        // we need high-low and open for this response
        decimal h{ get { return BarMath.HH(bars); } }
        decimal l { get { return BarMath.LL(bars); } }
        decimal o { get { return bars.RecentBar.Open; } }

        // here are the parameters that define how close we need to be to the open
        const decimal a = .06m; // minimum asymmetry allowed for entry
        const decimal r = .37m; // minimum range required for asymmetry test
        const decimal e = .1m; // how close market must be to asymmetry before entry
        const decimal p = .2m; // profit target
        const decimal s = -.11m; // stop loss
        
        // here are the trading rules that implement our strategy's intention
        protected override int Read(Tick tick, BarList bl)
        {
            // indicator setup
            bars = bl;
            if (!bl.Has(2,bl.Int)) return 0; // must have at least one one bar

            // asymmetry tests
            if (((h - o) > a) && ((o - l) > a)) { Shutdown("Not enough Asymetry to trade."); return 0; }
            if ((h - l) < r) return 0; // must have the range
            if (((h-o)<=a) && ((h-tick.trade)>e)) return MaxSize;
            if (((o-l)<=a) && ((tick.trade-l)>e)) return MaxSize*-1;

            // profit and loss tests
            decimal PL = Calc.OpenPT(tick.trade, Pos.AvgPrice, Pos.Size);
            if (PL > p) return Pos.FlatSize;
            if (PL < s) return Pos.FlatSize;

            return 0;
        }
    }
}