/*
 * Created by SharpDevelop.
 * User: mifus_000
 * Date: 20/05/2017
 * Time: 09:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Globalization;

using System.Threading.Tasks;

class Program
{
    
    public static string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    

    static decimal morePercent(decimal value, decimal perc)
    {
        return ((value * perc) / 100) + value;
    }

    static decimal lessPercent(decimal value, decimal perc)
    {
        return value - ((value * perc) / 100);
    }


    static decimal calcPerc(decimal more, decimal less)
    {
        return ((more * 100) / less) - 100;
    }

    public static void arbitrageHFT()
    {

        IExchange bitrecife = new ExchangeBitrecife();
        IExchange braziliex = new ExchangeBraziliex();

        Task.Run(() =>
        {
            bitrecife.getBalances();
        });
        Task.Run(() =>
        {
            braziliex.getBalances();
        });

       
       
        decimal hand = decimal.Parse( jConfig["arbitrage_amount"].ToString());
        decimal percentProfit = decimal.Parse(jConfig["arbitrage_percent"].ToString());
        int count = 0;
        while (true)
        {
            try
            {

                decimal lastBraziliex = 0;
                decimal lastBitrecife = 0;
                int i = 0;

                Task.Run(() =>
                {
                    lastBraziliex = braziliex.getLastValue("BTC_BRL");
                });
                Task.Run(() =>
                {
                    lastBitrecife = bitrecife.getLastValue("BTC_BRL");
                });

                i = 0;
                while (true)
                {
                    if (lastBraziliex != 0 && lastBitrecife != 0)
                        break;
                    System.Threading.Thread.Sleep(5);
                    i++;
                    if (i > 3000)
                        throw new Exception("Timeout while");
                }

                Logger.log("lastBraziliex " + lastBraziliex);
                Logger.log("lastBitrecife " + lastBitrecife);


                decimal[] buy = null;
                decimal[] sell = null;

                Task.Run(() =>
                {
                    buy = braziliex.getLowestAsk("BTC_BRL", lessPercent(hand, braziliex.getFee()));
                });
                Task.Run(() =>
                {
                    sell = bitrecife.getHighestBid("BTC_BRL", morePercent(hand, bitrecife.getFee()) + 0.01m);
                });


                i = 0;
                while (true)
                {
                    if (buy != null && sell != null)
                        break;
                    System.Threading.Thread.Sleep(5);
                    if (i > 3000)
                        throw new Exception("Timeout while");
                }


                decimal perc = calcPerc(buy[0], sell[0]);
                Logger.log("Compra BRAZILIEX(" + buy[0] + ") | Venda BITRECIFE(" + sell[0] + ") | perc " + perc + "%");

                if (buy[0] > sell[0])
                {
                    if (braziliex.getBalance("BRL") > morePercent(hand, 1))
                    {
                        if (bitrecife.getBalance("BTC") > morePercent(sell[0], 1))
                        {

                            if (perc >= percentProfit)
                            {
                                Task.Run(() =>
                                {
                                    braziliex.order("buy", "BTC_BRL", Math.Round(buy[0] - ((buy[0] - sell[0])/2),8) , morePercent(lastBraziliex, 10));
                                });
                                Task.Run(() =>
                                {
                                    bitrecife.order("sell", "BTC_BRL", Math.Round(sell[0],8), lessPercent(lastBitrecife, 10));
                                });


                                Logger.log("EXECUTADO! - Compra BRAZILIEX | Venda BITRECIFE | PROFIT " + (buy[0] - sell[0]) + " | " + perc + "%");
                                Console.WriteLine("Wait...");
                                System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));
                                Task.Run(() =>
                                {
                                    bitrecife.getBalances();
                                });
                                Task.Run(() =>
                                {
                                    braziliex.getBalances();
                                });
                                Console.WriteLine("Wait...");
                                System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));
                            }
                        }
                    }
                }







                ///////

                buy = null;
                sell = null;

                Task.Run(() =>
                {
                    buy = bitrecife.getLowestAsk("BTC_BRL", lessPercent(hand, bitrecife.getFee()) + 0.01m);
                });
                Task.Run(() =>
                {
                    sell = braziliex.getHighestBid("BTC_BRL", lessPercent(hand, braziliex.getFee()));
                });


                i = 0;
                while (true)
                {
                    if (buy != null && sell != null)
                        break;
                    System.Threading.Thread.Sleep(5);
                    if (i > 3000)
                        throw new Exception("Timeout while");
                }

                perc = calcPerc(buy[0], sell[0]);
                Logger.log("Compra BITRECIFE(" + buy[0] + ") | Venda BRAZILIEX(" + sell[0] + ") | perc " + perc + "%");

                if (buy[0] > sell[0])
                {
                    if (bitrecife.getBalance("BRL") > morePercent(hand, 1))
                    {
                        if (braziliex.getBalance("BTC") > morePercent(sell[0], 1))
                        {

                            if (perc >= percentProfit)
                            {
                                Task.Run(() =>
                                {
                                    bitrecife.order("buy", "BTC_BRL", Math.Round( buy[0] - ((buy[0] - sell[0])/2),8), morePercent(lastBitrecife, 10));
                                });
                                Task.Run(() =>
                                {
                                    braziliex.order("sell", "BTC_BRL", Math.Round(sell[0],8), lessPercent(lastBraziliex, 10));
                                });


                                Logger.log("EXECUTADO! Compra BITRECIFE | Venda BRAZILIEX | PROFIT" + (buy[0] - sell[0]) + " | " + perc + "%");
                                Console.WriteLine("Wait...");
                                System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));
                                Task.Run(() =>
                                {
                                    bitrecife.getBalances();
                                });
                                Task.Run(() =>
                                {
                                    braziliex.getBalances();
                                });
                                Console.WriteLine("Wait 30s...");
                                System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.log("ERRO ::: " + ex.Message + " | " + ex.StackTrace);
                System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));
            }


            Console.WriteLine("Wait...");
            System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));


            Logger.log("TOTAL BRL: R$ " + (bitrecife.getBalance("BRL") + braziliex.getBalance("BRL")));
            Logger.log("TOTAL BTC:    " + (bitrecife.getBalance("BTC") + braziliex.getBalance("BTC")) + " BTC");

            Logger.log("TOTAL BRL(BITRECIFE): R$ " + (bitrecife.getBalance("BRL") ));
            Logger.log("TOTAL BTC(BITRECIFE):    " + (bitrecife.getBalance("BTC")) + " BTC");

            Logger.log("TOTAL BRL(BRAZILIEX): R$ " + (braziliex.getBalance("BRL")));
            Logger.log("TOTAL BTC(BRAZILIEX):    " + (braziliex.getBalance("BTC")) + " BTC");

            Logger.log("");
            Logger.log("");

            count++;

            if (count > 300)
            {
                Task.Run(() =>
                {
                    bitrecife.getBalances();
                });
                Task.Run(() =>
                {
                    braziliex.getBalances();
                });

                                
                count = 0;
                System.Threading.Thread.Sleep(int.Parse(jConfig["sleep_default"].ToString()));
            }

        }


    }




    public static JContainer jConfig = null;
    public static void Main(string[] args)
    {

        try
        {

            String jsonConfig = System.IO.File.ReadAllText(location + "key.txt");
            jConfig = (JContainer)JsonConvert.DeserializeObject(jsonConfig, (typeof(JContainer)));


            arbitrageHFT();

        }
        catch 
        {

            return;
        }

    }

}
