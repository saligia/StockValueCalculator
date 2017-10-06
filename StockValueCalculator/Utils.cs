﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace StockValueCalculator
{
    public class Utils
    {
        public static HtmlAgilityPack.HtmlDocument loadHtmlDocument(string url, Encoding encoding)
        {
            HttpWebResponse response = (HttpWebResponse)WebRequest.CreateHttp(url).GetResponse();
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), encoding))
            {
                htmlDocument.Load(sr);
            }

            response.Close();

            return htmlDocument;
        }

        public static StockMarketTypes checkMarketType(string stockID)
        {
            Regex regexForChinaSZ = new Regex("^00[0-9]{4}$");
            Regex regexForChinaSH = new Regex("^60[0-9]{4}$");
            Regex regexForHK = new Regex("^0[0-9]{4}$");

            if (regexForChinaSZ.IsMatch(stockID))
            {
                return StockMarketTypes.CHINA_SZ_EXCHANGE_MARKET;
            }
            else if (regexForChinaSH.IsMatch(stockID))
            {
                return StockMarketTypes.CHINA_SH_EXCHANGE_MARKET;
            }
            else if (regexForHK.IsMatch(stockID))
            {
                return StockMarketTypes.HK_EXCHANGE_MARKET;
            }
            else
            {
                return StockMarketTypes.UNKNOWN;
            }
        }

        public static string[] readPreferStockIDList()
        {
            List<string> resultList = new List<string>();
            if (File.Exists("PreferStockList.csv"))
            {
                foreach (string line in File.ReadAllLines("PreferStockList.csv"))
                {
                    resultList.Add(line.Split(',')[0]);
                }
            }
            return resultList.ToArray();
        }

        public static HtmlNode findNodeByText(HtmlNode parentNode, string xpath, string compareValue, int offset)
        {
            int count = 0;
            bool foundMatch = false;
            foreach (HtmlNode subNode in parentNode.SelectNodes(xpath))
            {
                if (subNode.InnerText.Equals(compareValue, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    foundMatch = true;
                }
                if (foundMatch)
                {
                    if(count == offset)
                    {
                        return subNode;
                    }
                    else
                    {
                        count += 1;
                    }
                }
            }

            return null;
        }

        public static void parseCompanyBasicInfo(string url, ref StockInfo stockInfo)
        {
            // The following implementation is based on Baidu Stock API.
            // It might need to be changed if we decide to use other API instead.
            HtmlAgilityPack.HtmlDocument StockInfoHtmlDocument = Utils.loadHtmlDocument(url, Encoding.GetEncoding("utf-8"));

            HtmlNode stockInfoTableNode = StockInfoHtmlDocument.DocumentNode.SelectSingleNode("//div[@class='stock-bets']");

            if (stockInfoTableNode != null)
            {
                HtmlNode nameNode = stockInfoTableNode.SelectSingleNode("//a[@class='bets-name']");
                HtmlNode dateNode = stockInfoTableNode.SelectSingleNode("//span[@class='state f-up']");
                HtmlNode closePriceNode = stockInfoTableNode.SelectSingleNode("//strong[@class='_close']");
                HtmlNode detailNode = stockInfoTableNode.SelectSingleNode("//div[@class='bets-content']//div");

                stockInfo.CompanyName = nameNode.InnerText.Trim();
                stockInfo.LastTradingPrice = closePriceNode.InnerText.Trim();
                stockInfo.DateOfInfo = dateNode.InnerText.Trim().Replace("&nbsp;", "");
                decimal companyProfitPerShare = decimal.Zero;
                decimal peRatio = decimal.Zero;
                decimal lastTradingPrice = decimal.Zero;
                decimal.TryParse(closePriceNode.InnerText.Trim(), out lastTradingPrice);
                foreach (HtmlNode subNode in detailNode.ChildNodes)
                {
                    if (subNode.HasChildNodes)
                    {
                        string firstChildText = subNode.FirstChild.InnerText.Trim();
                        string lastChildText = subNode.LastChild.InnerText.Trim();
                        if (firstChildText.Equals("每股收益", StringComparison.CurrentCultureIgnoreCase))
                        {
                            decimal.TryParse(lastChildText, out companyProfitPerShare);
                        }
                        else if (firstChildText.Contains("市盈率"))
                        {
                            stockInfo.PERatio = lastChildText;
                            decimal.TryParse(lastChildText, out peRatio);
                        }
                        // if failed to parse last trading price (most likely to happen during long holiday), then use previous closing price instead
                        else if (lastTradingPrice == decimal.Zero &&
                            firstChildText.Equals("昨收", StringComparison.CurrentCultureIgnoreCase))
                        {
                            decimal.TryParse(lastChildText, out lastTradingPrice);
                            stockInfo.DateOfInfo += "(顺延前一收盘价)";
                        }
                    }
                }

                // Recalculate the profit per share by the past days.
                // Always use PERatio for bellow calculation if available.
                if (peRatio > 0)
                {
                    companyProfitPerShare = decimal.Round(lastTradingPrice / peRatio * 365M / DateTime.Today.DayOfYear, 4);
                }
                else if (companyProfitPerShare > 0)
                {
                    companyProfitPerShare = decimal.Round(companyProfitPerShare * 365M / DateTime.Today.DayOfYear, 4);
                }

                stockInfo.CompanyProfitPerShare = Convert.ToString(companyProfitPerShare);
            }
         }

        public static void parseCompanyProfitSharing(string url, ref StockInfo stockInfo)
        {
            // The following implementation is based on IFeng Stock API.
            // It might need to be changed if we decide to use other API instead
            HtmlAgilityPack.HtmlDocument lastProfitSharingHtmlDocument = Utils.loadHtmlDocument(url, Encoding.GetEncoding("utf-8"));

            HtmlNodeCollection lastProfitSharingTableNodes = lastProfitSharingHtmlDocument.DocumentNode.SelectNodes("//table[@class='tab01']");

            foreach (HtmlNode profitSharingTableNode in lastProfitSharingTableNodes)
            {
                HtmlNode tdNode = Utils.findNodeByText(profitSharingTableNode, ".//tr/td", "公告日期", 1);
                DateTime publishDate = new DateTime();
                DateTime.TryParse(tdNode.InnerText.Trim(), out publishDate);
                HtmlNode profitSharingNode = Utils.findNodeByText(profitSharingTableNode, ".//tr/td", "每10股现金(含税)", 1);
                string profitSharingNodeText = profitSharingNode.InnerText.Substring(0, profitSharingNode.InnerText.Length - 1);
                if (publishDate.CompareTo(DateTime.Today.AddYears(-1)) > 0 &&
                    publishDate.CompareTo(DateTime.Today) < 0)
                {
                    stockInfo.FirstYearProfitSharing = profitSharingNodeText;
                }
                else if (publishDate.CompareTo(DateTime.Today.AddYears(-2)) > 0 &&
                    publishDate.CompareTo(DateTime.Today.AddYears(-1)) < 0)
                {
                    stockInfo.SecondYearProfitSharing = profitSharingNodeText;
                }
                else if (publishDate.CompareTo(DateTime.Today.AddYears(-3)) > 0 &&
                    publishDate.CompareTo(DateTime.Today.AddYears(-2)) < 0)
                {
                    stockInfo.ThirdYearProfitSharing = profitSharingNodeText;
                }
                else if (publishDate.CompareTo(DateTime.Today.AddYears(-4)) > 0 &&
                    publishDate.CompareTo(DateTime.Today.AddYears(-3)) < 0)
                {
                    stockInfo.FourthYearProfitSharing = profitSharingNodeText;
                }
                else if (publishDate.CompareTo(DateTime.Today.AddYears(-5)) > 0 &&
                    publishDate.CompareTo(DateTime.Today.AddYears(-4)) < 0)
                {
                    stockInfo.FifthhYearProfitSharing = profitSharingNodeText;
                }
            }
        }

        public static string getValueFromArray(string[] inputArray, int idx)
        {
            return inputArray.Length > idx ? inputArray[idx] : "";
        }
    }
}
