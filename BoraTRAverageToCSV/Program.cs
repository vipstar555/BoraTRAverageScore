using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace BoraTRAverageToCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            //始まりの日付取得
            DateTime fromDatetime = DateTime.Now.Date;
            Console.WriteLine("指定したい始まりの日付を入力してください。\r\n" +
                "書式：2020/01/01(空白なら今日の日付)");            
            var fromDatetimeString = Console.ReadLine();
            //空白以外の日付じゃない書式が入れられたら終了する
            if (fromDatetimeString != "" && DateTime.TryParse(fromDatetimeString, out fromDatetime) == false)
            {
                Console.WriteLine("日付以外の書式で入力されています。");
                Console.ReadLine();
                return;
            }
            //終わりの日付取得
            DateTime toDatetime = DateTime.Now.Date;
            Console.WriteLine("指定したい終わりの日付を入力してください。\r\n" +
                "書式：2020/01/01(空白なら今日の日付)");
            var toDatetimeString = Console.ReadLine();
            //空白以外の日付じゃない書式が入れられたら終了する
            if (toDatetimeString != "" && DateTime.TryParse(toDatetimeString, out toDatetime) == false)
            {
                Console.WriteLine("日付以外の書式で入力されています。");
                Console.ReadLine();
                return;
            }
            //始まりの日付方が後の日付の場合は処理を終わる
            if(fromDatetime > toDatetime)
            {
                Console.WriteLine("始まりと終わりの日付の範囲を正しく入力してください。");
                Console.ReadLine();
                return;
            }
            //〇日分の平均を取得する日数を決定する
            int MA = 5;

            //株価データ取得
            var stockData = new StockDataControl();
            var stockList = stockData.GetAllCodeAddMAStockData(fromDatetime, toDatetime, MA).ToList();

            //コード一覧を取得
            var codeList = stockList.Select(x => x.Code).Distinct();
            //TR・ボラデータ作成
            //CSV作成準備
            using (var sw = new StreamWriter(@"D:\デスクトップ\ボラTR平均\ボラTR平均.csv", false, Encoding.UTF8))
            {
                var _csvData = new CSVData();
                PropertyInfo[] infoArray = _csvData.GetType().GetProperties();
                //タイトル書き込み
                foreach (var info in infoArray)
                {
                    sw.Write($"{info.Name},");
                }
                sw.WriteLine();
                foreach (var code in codeList)
                {
                    var codeStockList = stockList.Where(x => x.Code == code).OrderBy(x => x.Date).Select(x =>
                        new Price
                        {
                            Code = x.Code,
                            Name = x.Name,
                            DateTime = x.Date,
                            closePrice = x.ClosePrice,
                            highPrice = x.HighPrice,
                            lowPrice = x.LowPrice,
                            Cap = x.Cap,
                            Volume = x.Volume,
                        }
                        ).ToArray();
                    var TRData = BoraTRControl.BoraTRCalc(codeStockList);
                    //TR・ボラのMA作成
                    var TRAveArray = Technical.MovingAverage(TRData.OrderBy(x => x.DateTime).Select(x => x.TRPercent).ToArray(), 5).ToArray();
                    var BoraAveArray = Technical.MovingAverage(TRData.OrderBy(x => x.DateTime).Select(x => x.BoraPercent).ToArray(), 5).ToArray();

                    for (int i = MA; i < codeStockList.Count(); i++)
                    {
                        var csvData = new CSVData
                        {
                            Code = codeStockList[i].Code,
                            Name = codeStockList[i].Name,
                            DateTime = codeStockList[i].DateTime,
                            Cap = codeStockList[i].Cap,
                            Volume = codeStockList[i].Volume,
                            BoraAverage = BoraAveArray[i],
                            TRAverage = TRAveArray[i],
                        };
                        //データ書き込み
                        foreach(var info in infoArray)
                        {
                            sw.Write($"{info.GetValue(csvData, null)},");
                        }
                        sw.WriteLine();
                    }
                    //出力データの成形
                    //プロパティTypeをループ取得
                    continue;
                }
            }            

            Console.WriteLine("Enterを押してください。");
            Console.ReadLine();
        }
    }

    class CSVData
    {
        public DateTime DateTime { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }

        public long Volume { get; set; }
        public long Cap { get; set; }

        public double? TRAverage { get; set; }
        public double? BoraAverage { get; set; }
    }
}
