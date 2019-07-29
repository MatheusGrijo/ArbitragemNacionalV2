using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class ExchangeBitrecife : ExchangeBase, IExchange
{
    public decimal balance_brl = 0;
    public decimal balance_btc = 0;

    public ExchangeBitrecife()
    {
        this.urlTicker = "https://bitrecife.com.br/";
        this.key = Program.jConfig["bitrecife_key"].ToString();
        this.secret = Program.jConfig["bitrecife_secret"].ToString();
        this.lockQuantity = false;
        this.fee = decimal.Parse(Program.jConfig["bitrecife_fee"].ToString());
    }

    public decimal getFee()
    {
        return this.fee;
    }


    public string getName()
    {
        return "BITRECIFE";

    }


    public decimal calculateAmount(decimal amount, string pair)
    {
        return amount;
    }


    public string getBalances()
    {
        String json = post("https://exchange.bitrecife.com.br/api/v3/private/getbalances", "apikey=" + this.key, this.key, this.secret);
        JContainer jContainer = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

        balance_brl = decimal.Parse(jContainer["result"][0]["Balance"].ToString().Replace(".", ","));
        balance_btc = decimal.Parse(jContainer["result"][1]["Balance"].ToString().Replace(".", ","));

        return json;
    }

    public decimal getLastValue(string pair)
    {

        try
        {
            String json = Http.get("https://exchange.bitrecife.com.br/api/v3/public/getticker?market=BTC_BRL");

            JContainer j = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

            return decimal.Parse(j["result"][0]["Last"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
        }
        catch
        {
        }
        return 0;

    }

    public decimal[] getLowestAsk(string pair, decimal amount)
    {
        try
        {
            String json = Http.get("https://exchange.bitrecife.com.br/api/v3/public/getorderbook?market=BTC_BRL&type=SELL&depth=2000");
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal orderPrice = 0;
            decimal orderAmount = 0;
            decimal totalCost = 0;
            decimal totalAmount = 0;
            decimal remaining = amount;
            decimal cost = 0;

            foreach (var item in jCointaner["result"]["sell"])
            {
                orderPrice = decimal.Parse(item["Rate"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
                orderAmount = decimal.Parse(item["Quantity"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
                cost = orderPrice * orderAmount;
                if (cost < remaining)
                {
                    remaining -= cost;
                    totalCost += cost;
                    totalAmount += orderAmount;
                }
                else
                {
                    //finished
                    remaining -= amount;
                    totalCost += amount * orderPrice;
                    totalAmount += amount;
                    arrayValue[0] = Math.Round(amount / (totalCost / totalAmount), 8);
                    arrayValue[1] = Math.Round(amount / orderPrice, 8);
                    return arrayValue;
                }
            }
        }
        catch
        {
        }
        return new decimal[2];
    }

    public decimal[] getHighestBid(string pair, decimal amount)
    {
        try
        {

            pair = "btc_brl";
            String json = Http.get("https://exchange.bitrecife.com.br/api/v3/public/getorderbook?market=BTC_BRL&type=BUY&depth=2000");
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));


            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal orderPrice = 0;
            decimal orderAmount = 0;
            decimal totalCost = 0;
            decimal totalAmount = 0;
            decimal remaining = amount;
            decimal cost = 0;

            foreach (var item in jCointaner["result"]["buy"])
            {

                orderPrice = decimal.Parse(item["Rate"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
                orderAmount = decimal.Parse(item["Quantity"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
                cost = orderPrice * orderAmount;
                if (cost < remaining)
                {
                    remaining -= cost;
                    totalCost += cost;
                    totalAmount += orderAmount;
                }
                else
                {
                    //finished
                    remaining -= amount;
                    totalCost += amount * orderPrice;
                    totalAmount += amount;
                    arrayValue[0] = Math.Round(amount / (totalCost / totalAmount), 8);
                    arrayValue[1] = Math.Round(amount / orderPrice, 8);
                    return arrayValue;
                }
            }
        }
        catch
        {
        }
        return new decimal[2];


    }


    public void getMarket()
    {
        String json = Http.get(this.urlTicker);
        this.dataSource = (Newtonsoft.Json.Linq.JContainer)JsonConvert.DeserializeObject(json);
    }

    public Operation order(string type, string pair, decimal amount, decimal price)
    {
        pair = "BTC_BRL";
        String json = post("https://exchange.bitrecife.com.br/api/v3/private/" + type.Trim().ToLower()+"limit", "apikey=" + this.key +  "&market=" + pair + "&quantity=" + amount.ToString().Replace(",", ".") + "&rate=" + price.ToString().Replace(",", "."), this.key, this.secret);
        Logger.log(json);
        return null;
    }

    public string getKey()
    {
        return this.key;
    }

    public string getSecret()
    {
        return this.secret;
    }



    public string post(String url, String parameters, String key, String secret)
    {
        try
        {
            // lock (objLock)
            {

                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                parameters = "?nonce=" + unixTimestamp + "&" + parameters;
                Logger.log(url + parameters);
                var request = (HttpWebRequest)WebRequest.Create(url + parameters);                

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var data = Encoding.ASCII.GetBytes(url + parameters);

                HMACSHA512 encryptor = new HMACSHA512();
                encryptor.Key = Encoding.ASCII.GetBytes(secret);
                String sign = Utils.ByteToString(encryptor.ComputeHash(data)).ToLower();

                request.Headers["apisign"] = sign;
                request.Method = "POST";
                var response = (HttpWebResponse)request.GetResponse();
                String result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Logger.log(result);
                return result;
            }
        }
        catch (Exception ex)
        {
            Logger.log("ERROR POST " + ex.Message + ex.StackTrace);
            return null;
        }
        finally
        {
        }
    }

    public decimal getBalance(string pair)
    {
        if (pair == "BRL")
            return balance_brl;
        if (pair == "BTC")
            return balance_btc;

        return 0m;
    }


    public OrderStatus getOrder(string idOrder)
    {
        throw new NotImplementedException();
    }

    public bool isLockQuantity()
    {
        return this.lockQuantity;
    }
}
