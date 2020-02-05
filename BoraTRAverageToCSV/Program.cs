﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Console.WriteLine("Enterを押してください。");
            Console.ReadLine();
            //BoraTR
        }
    }
}
