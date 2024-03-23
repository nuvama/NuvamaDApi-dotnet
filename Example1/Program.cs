using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NorenRestApiWrapper;

namespace NorenRestSample
{    
    public static class Program
    {
        #region dev  credentials

        public static string endPoint = "";
        public static string wsendpoint = "";
        public static string uid = "";
        
        public static string pwd = "";
        public static string imei = "";
        public static string vc = "";

        public static string appkey = "";
        public static string newpwd = "";
        #endregion

        //account id for placing order 
        public static string actid = "DEMO1";

        #region read credencials from app config
        static bool ReadConfig()
        {
            if (ConfigurationManager.AppSettings.Get("endpoint") == null)
            {
                return false;
            }
            if (ConfigurationManager.AppSettings.Get("wsendpoint") == null)
            {
                return false;
            }
            if (ConfigurationManager.AppSettings.Get("uid") == null)
            {
                return false;
            }
            if (ConfigurationManager.AppSettings.Get("pwd") == null)
            {
                return false;
            }
            
            if (ConfigurationManager.AppSettings.Get("vc") == null)
            {
                return false;
            }
            if (ConfigurationManager.AppSettings.Get("appkey") == null)
            {
                return false;
            }
            Program.endPoint = ConfigurationManager.AppSettings.Get("endpoint");
            Program.wsendpoint = ConfigurationManager.AppSettings.Get("wsendpoint");
            Program.uid = ConfigurationManager.AppSettings.Get("uid");
            Program.pwd = ConfigurationManager.AppSettings.Get("pwd");            
            Program.vc = ConfigurationManager.AppSettings.Get("vc");
            Program.appkey = ConfigurationManager.AppSettings.Get("appkey");
            return true;
        }
        #endregion
        public static bool loggedin = false;

        
        public static void OnStreamConnect(NorenStreamMessage msg)
        {
            Program.loggedin = true;
            nApi.SubscribeOrders(Handlers.OnOrderUpdate, uid);
            //nApi.SubscribeToken("NSE", "22");
            nApi.SubscribeTokenDepth("NSE", "22");
            
        }
        public static NorenRestApi nApi = new NorenRestApi();
        
        static void Main(string[] args)
        {
            if (ReadConfig() == false)
            {
                Console.WriteLine("config file values failed");
                return;
            }

            LoginMessage loginMessage = new LoginMessage();
            loginMessage.apkversion = "1.0.0";
            loginMessage.uid = uid;
            loginMessage.pwd = pwd;
            
            loginMessage.imei = imei;
            loginMessage.vc = vc;
            loginMessage.source = "API";
            loginMessage.appkey = appkey;
            nApi.SendLogin(Handlers.OnAppLoginResponse, endPoint, loginMessage);

            nApi.SessionCloseCallback = Handlers.OnAppLogout;
            nApi.onStreamConnectCallback = Program.OnStreamConnect;

            while (Program.loggedin == false)
            {
                //dont do anything till we get a login response         
                Thread.Sleep(5);
            }          
            
            bool dontexit = true;
            while(dontexit)
            {                
                var input = Console.ReadLine();
                var opts = input.Split(' ');
                foreach (string opt in opts)
                {
                    switch (opt.ToUpper())
                    {
                        case "B":
                            ActionPlaceBuyorder();
                            break;
                        case "O":
                            nApi.SendGetOrderBook(Handlers.OnOrderBookResponse, "");
                            break;
                        case "P":
                            nApi.SendGetPositionBook(Handlers.OnPositionsResponse);
                            break;
                        case "S":
                            string exch;
                            string token;
                            Console.WriteLine("Enter exch:");
                            exch = Console.ReadLine();
                            Console.WriteLine("Enter Token:");
                            token = Console.ReadLine();
                            nApi.SendGetSecurityInfo(Handlers.OnResponseNOP, exch, token);
                            break;
                        case "T":
                            nApi.SendGetTradeBook(Handlers.OnTradeBookResponse);
                            break;
                        case "Q":
                            nApi.SendLogout(Handlers.OnAppLogout);
                            dontexit = false;
                            return;
                        default:
                            // do other stuff...
                            ActionOptions();
                            break;
                    }
                }
                

                //var kp = Console.ReadKey();
                //if (kp.Key == ConsoleKey.Q)
                //    dontexit = false;
                //Console.WriteLine("Press q to exit.");
            }            
        }
        

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        #region actions
        
        public static void ActionPlaceBuyorder()
        {
            //sample cover order
            PlaceOrder order = new PlaceOrder();
            order.uid = uid;
            order.actid = actid;
            order.exch = "NSE";
            order.tsym = "M&M-EQ";
            order.qty = "10";
            order.dscqty = "0";
            order.prc = "100.5";
            
            order.prd = "I";
            order.trantype = "B";
            order.prctyp = "LMT";
            order.ret = "DAY";
            order.ordersource = "API";

            nApi.SendPlaceOrder(Handlers.OnResponseNOP, order);
        }

       
        public static void ActionOptions()
        {
            Console.WriteLine("Q: logout.");
            Console.WriteLine("O: get OrderBook");
            Console.WriteLine("T: get TradeBook");
            Console.WriteLine("P: get Positions");
            Console.WriteLine("B: place a buy order");
            Console.WriteLine("S: get security info");
        }
        #endregion
    }

}
