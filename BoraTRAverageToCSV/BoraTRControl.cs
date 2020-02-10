using System;
using System.Collections.Generic;
using System.Text;
using SQLServerStockBunkatu;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace BoraTRAverageToCSV
{
    static public class BoraTRControl
    {
        //TR・ボラ計算(コード別に入れる)
        static public IEnumerable<BoraTR> BoraTRCalc(IReadOnlyCollection<Price> prices)
        {
            double? lastClosePrice = null;
            foreach(var price in prices.OrderBy(x => x.DateTime))
            {
                var item = new BoraTR {
                    DateTime = price.DateTime,
                    Code = price.Code,
                    Bora = 0,
                    BoraPercent = 0,
                    TR = 0,
                    TRPercent = 0,
                    TRPattern = "null",
                    LastClosePrice = lastClosePrice,
                };
                //前日終値が計算できない間はnullを返す
                if(lastClosePrice == null)
                {
                    //終値を入れる
                    lastClosePrice = price.closePrice;
                    yield return item;
                    continue;
                }
                //当日値がついてない場合はnullを返す
                if(price.highPrice == null)
                {
                    yield return item;
                    continue;
                }
                //TR・ボラ計算
                var trData = TRCalc(price, lastClosePrice);
                item.TR = trData.TR;
                item.TRPercent = trData.TRPercent;
                item.TRPattern = trData.Pattern;
                item.Bora = price.highPrice - price.lowPrice;
                item.BoraPercent = (price.highPrice - price.lowPrice)/ price.lowPrice*100;

                yield return item;
                //終値がある場合は入れ替える
                if(price.closePrice != null)
                {
                    lastClosePrice = price.closePrice;
                }
            }
        }
        //TR計算
        static private TRData TRCalc(Price price, double? lastClosePrice)
        {
            var resultList = new List<TRData>
            {
                new TRData{TR = price.highPrice - price.lowPrice, TRPercent = (price.highPrice - price.lowPrice) /  price.lowPrice*100, Pattern = "高-安"}, //高-安
                new TRData{TR = price.highPrice - lastClosePrice, TRPercent = (price.highPrice - lastClosePrice) /  lastClosePrice*100, Pattern = "高-前終"}, //高-前日終値
                new TRData{TR = lastClosePrice - price.lowPrice, TRPercent = (lastClosePrice - price.lowPrice) /  price.lowPrice*100, Pattern = "前終-安"}, //前日終値-安                
            };
            return resultList.OrderByDescending(x => x.TR).FirstOrDefault();
        }


    }

    public class TRData
    {
        public double? TR { get; set; }
        public double? TRPercent { get; set; }
        public string Pattern { get; set; }
    }

    public class Price
    {
        public DateTime DateTime { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public double? highPrice { get; set; }
        public double? lowPrice { get; set; }
        public double? closePrice { get; set; }
        public long Cap { get; set; }
        public long Volume { get; set; }
    }

    public class BoraTR
    {
        public DateTime DateTime { get; set; }
        public int Code { get; set; }
        public double? TR { get; set; }
        public double? TRPercent { get; set; }
        public string TRPattern { get; set; }
        public double? Bora { get; set; }
        public double? BoraPercent { get; set; }
        public double? LastClosePrice { get; set; }
    }
}
