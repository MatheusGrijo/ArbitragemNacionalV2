using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class BackTestBalance
    {
        public decimal totalUsdt = 1000;
        public decimal totalOrderUsdt = 100;
        public System.Data.DataTable balances;

        public void createInicialBalances()
        {
            balances = new System.Data.DataTable();
            balances.Columns.Add("pair");
            balances.Columns.Add("total");

            //balances.Rows.Add("BTC", "0,0491");
            //balances.Rows.Add("ETH", "0,87737");
            //balances.Rows.Add("DASH", "1,1158");
            //balances.Rows.Add("LTC", "2,7893");
            //balances.Rows.Add("XMR", "1,956259");
            balances.Rows.Add("BTC", "990,0491");
            balances.Rows.Add("ETH", "9990,87737");
            balances.Rows.Add("DASH", "9991,1158");
            balances.Rows.Add("LTC", "9992,7893");
            balances.Rows.Add("XMR", "9991,956259");
        }

       

        public void changeBalance(String pair,decimal total)
        {
            for (int i = 0; i < this.balances.Rows.Count; i++)
            {
                if (pair.IndexOf(this.balances.Rows[i]["pair"].ToString()) >= 0)
                {
                    this.balances.Rows[i]["total"] = decimal.Parse(this.balances.Rows[i]["total"].ToString().Replace(".", ",")) + total;                    
                }
            }
        }

    }

    public class ExchangeBase
    {
        public string urlTicker = "";
        public bool lockQuantity = false;
        public Object dataSource;
        public string key;
        public string secret;
        public decimal fee = 0.25m;
        public BackTestBalance backTestBalance = new BackTestBalance();
        public System.Data.DataSet dsBalances = new System.Data.DataSet();

        public decimal getBalance(String exchange, String coin)
        {
            try
            {
                coin = coin.Replace("USDT-", "");
            return 0;
            }
            catch
            {
                return 0;
            }
        }

        public decimal fixAmount(decimal amount)
        {
            try
            {
                String amountAsString = amount.ToString();
                String amountAsStringPartA = amountAsString.Split(',')[0];
                String amountAsStringPartB = amountAsString.Split(',')[1];
                if (amountAsStringPartB.Length > 8)
                    amountAsStringPartB = amountAsStringPartB.Substring(0, 8);
                amountAsString = amountAsStringPartA + "," + amountAsStringPartB;
                return decimal.Parse(amountAsString);
            }
            catch
            {
                return amount;
            }
        }

        public decimal getQuantity(String coin)
        {
            if (coin.IndexOf("BCH") >= 0)
                return decimal.Parse("0,01");
            if (coin.IndexOf("XMR") >= 0)
                return decimal.Parse("0,001");
            if (coin.IndexOf("LTC") >= 0)
                return decimal.Parse("0,1");
            if (coin.IndexOf("ETH") >= 0)
                return decimal.Parse("0,001");
            if (coin.IndexOf("BTC") >= 0)
                return decimal.Parse("0,01");
            if (coin.IndexOf("DASH") >= 0)
                return decimal.Parse("0,001");
            if (coin.IndexOf("XRP") >= 0)
                return decimal.Parse("1");
            else
                return 0;
        }
    }
