using System;

namespace TradeLib
{
    /// <summary>
    /// Specify an order to buy or sell a quantity of stock.
    /// </summary>
    [Serializable]
    public class Order : Trade
    {
        public Order() { }
        public override bool isValid { get { return (symbol != null) && (size != 0); } }
        public bool isMarket { get { return (price == 0) && (stopp == 0); } }
        public bool isLimit { get { return (price != 0); } }
        public bool isStop { get { return (stopp != 0); } }
        public int SignedSize { get { return Math.Abs(size) * (side ? 1 : -1); } }
        public int UnSignedSize { get { return Math.Abs(size); } }
        public Order(string sym, bool side, int size, decimal p, decimal s, string c, int time, int date)
        {
            this.symbol = sym.ToUpper();
            this.side = side;
            this.size = System.Math.Abs(size);
            this.price = p;
            this.stopp = s;
            this.comment = c;
            this.time = time;
            this.date = date;
            this.isFilled = false;
        }
        public Order(string sym, bool side, int size)
        {
            this.symbol = sym.ToUpper();
            this.side = side;
            this.price = 0;
            this.stopp = 0;
            this.comment = "";
            this.time = 0;
            this.date = 0;
            this.size = System.Math.Abs(size);
            this.isFilled = false;
        }
        public Order(string sym, bool side, int size, string c)
        {
            this.symbol = sym.ToUpper();
            this.side = side;
            this.price = 0;
            this.stopp = 0;
            this.comment = c;
            this.time = 0;
            this.date = 0;
            this.size = System.Math.Abs(size);
            this.isFilled = false;
        }
        public Order(string sym, int size)
        {
            this.symbol = sym.ToUpper();
            this.side = size > 0;
            this.price = 0;
            this.stopp = 0;
            this.comment = "";
            this.time = 0;
            this.date = 0;
            this.size = System.Math.Abs(size);
            this.isFilled = false;
        }
        public override string ToString()
        {
            if (this.isFilled) return base.ToString();
            return this.date + ":" + this.time + (side ? " BUY" : " SELL") + Math.Abs(size) + " " + this.symbol + "@" + ((this.price == 0) ? "Market" : this.price.ToString());
        }

        /// <summary>
        /// Serialize order as a string
        /// </summary>
        /// <returns></returns>
        public override string Serialize()
        {
            const char d = ',';
            return symbol + d + (side ? "B" : "S") + d + Math.Abs(size) + d + price + d + stopp + d + comment + d + ex + d + accountid + d + this.Security.ToString() + d + this.Currency.ToString();
        }
        /// <summary>
        /// Deserialize string to Order
        /// </summary>
        /// <returns></returns>
        public new static Order Deserialize(string message)
        {
            message = message.Remove(message.Length - 1); // remove last comma
            string[] rec = message.Split(','); // get the record
            bool side = Convert.ToBoolean(rec[(int)OrderField.Side]);
            int size = Convert.ToInt32(rec[(int)OrderField.Size]);
            decimal xprice = Convert.ToDecimal(rec[(int)OrderField.Price]);
            string sym = rec[(int)OrderField.Symbol];
            Order o = new Order(sym, side, size);
            o.comment = rec[(int)OrderField.Comment];
            o.Account = rec[(int)OrderField.Account];
            o.Exchange = rec[(int)OrderField.Exchange];
            o.Currency = (Currency)Enum.Parse(typeof(Currency), rec[(int)OrderField.Currency]);
            o.Security = (Security)Enum.Parse(typeof(Security), rec[(int)OrderField.Security]);
            return o;
        }
    }

    public enum OrderField
    {
        Symbol = 0,
        Side,
        Size,
        Price,
        Stop,
        Comment,
        Exchange,
        Account,
        Security,
        Currency,
    }

}
