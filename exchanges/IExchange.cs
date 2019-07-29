using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class Operation
    {
        public bool success;
        public string description;
        public string json;
        public string amount;
    }


    public enum OrderStatus
    {
        open,
        error,
        close
    }


    public interface IExchange
    {
        void getMarket();
        decimal getLastValue(String pair);
        string getName();        
        Operation order(string type, string pair, decimal amount, decimal price);
        decimal getBalance(String pair);
        string getBalances();        
        OrderStatus getOrder(String idOrder);

        decimal[] getLowestAsk(string pair,decimal amount);
        decimal[] getHighestBid(string pair, decimal amount);

        
        decimal getFee();        
        decimal calculateAmount(decimal amount,string pair);
        bool isLockQuantity();
        string getKey();
        string getSecret();        
    }
