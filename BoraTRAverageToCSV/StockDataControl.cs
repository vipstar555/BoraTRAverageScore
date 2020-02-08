using SQLServerStockBunkatu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoraTRAverageToCSV
{
    public class StockDataControl
    {
        BunkatuConText bunkatuCon;
        YahooFinanceDbContext yahooCon;

        //初期化
        public StockDataControl()
        {
            //分割・併合Context
            bunkatuCon = new GetBunkatuContext().GetContext();
            //株価Context
            yahooCon = new YahooFinanceDbContext();
        }
        //全銘柄分の指定した日付データ+MAに必要なデータ分渡す
        public IEnumerable<Stock> GetAllCodeAddMAStockData(DateTime fromDatetime, DateTime toDatetime, int MA)
        {
            //引数の日付範囲＋MA分必要なデータを計算する
            var DatetimeList = yahooCon.TradeIndexs.Select(x => x.date).Distinct().OrderBy(x => x).ToList();
            var fromDateIndex = DatetimeList.FindIndex(x => fromDatetime <= x);
            var toDateIndex = DatetimeList.FindLastIndex(x => x <= toDatetime);
            //指定した日付が無い場合は終了
            if (fromDateIndex == -1 || toDateIndex == -1)
            {
                Console.WriteLine("指定された日付はデータベースに存在しません。");
                yield break;
            }
            //MAの計算に必要な分も取得する
            fromDateIndex -= MA;
            //toDateIndexが範囲外の場合は0に修正する
            if (fromDateIndex < 0)
            {
                fromDateIndex = 0;
            }
            //Stockクラスを返す
            var start = DatetimeList[fromDateIndex];
            var end = DatetimeList[toDateIndex];
            var stockSQL = yahooCon.TradeIndexs.Where(x => start <= x.date && x.date <= end).Select(
                x => new Stock {
                    Date = x.date,
                    Code = x.code,
                    ClosePrice = x.price.closePrice,
                    HighPrice = x.price.highPrice,
                    LowPrice = x.price.lowPrice,
                    OpenPrice = x.price.openPrice,
                }
                );
            //分割・併合データ
            var bunkatus = bunkatuCon.Bunkatus.Where(x => start <= x.KenriDate && x.KenriDate <= end).Select(
                x => new BunkatuHeigouData
                {
                    Code = x.Code,
                    KenriDate = x.KenriDate,
                    bairitu = x.BunkatuMae / x.BunkatuAto,
                }
                );
            var heigous = bunkatuCon.Heigous.Where(x => start <= x.KenriDate && x.KenriDate <= end).Select(
                x => new BunkatuHeigouData
                {
                    Code = x.Code,
                    KenriDate = x.KenriDate,
                    bairitu = x.HeigouMae / x.HeigouAto,
                }
                );
            var bunkatuList = bunkatus.ToList();
            foreach(var heigou in heigous)
            {
                bunkatuList.Add(heigou);
            }
            //分割・併合の倍率を適用させながら株価データを返す
            foreach (var stock in stockSQL)
            {
                var tmpStock = stock;
                var tmpBunkatus = bunkatuList.Where(x => x.Code == stock.Code);
                foreach(var bunkatu in tmpBunkatus)
                {
                    //権利日が日付以前の場合は補正する
                    if(stock.Date <= bunkatu.KenriDate)
                    {
                        tmpStock.HighPrice = tmpStock.HighPrice*bunkatu.bairitu;
                        tmpStock.LowPrice = tmpStock.LowPrice * bunkatu.bairitu;
                        tmpStock.OpenPrice = tmpStock.OpenPrice * bunkatu.bairitu;
                        tmpStock.ClosePrice = tmpStock.ClosePrice * bunkatu.bairitu;           
                    }
                }

                yield return tmpStock;
            }

        }
        
    }

    public class BunkatuHeigouData
    {
        public int Code { get; set; }
        public DateTime KenriDate { get; set; }//これを含む前の日付全てが補正条件
        public double bairitu { get; set; }//分割：　併合：併合後/併合前
    }

    public class Stock
    {
        public DateTime Date { get; set; }
        public int Code { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public double? OpenPrice { get; set; }
        public double? ClosePrice { get; set; }
    }
}
