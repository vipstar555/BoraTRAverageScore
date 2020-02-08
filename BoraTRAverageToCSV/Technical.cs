using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoraTRAverageToCSV
{
    static public class Technical
    {
        //移動平均線作成
        static public IEnumerable<double?> MovingAverage(IReadOnlyCollection<double?> stockData ,int span)
        {
            var queue = new Queue<double?>();
            //MAの数が足りない分はnullを返す
            int i = 0;       
            foreach (var price in stockData)
            {
                queue.Enqueue(price);
                if(queue.Count() < span)
                {
                    yield return null;
                    continue;
                }
                yield return queue.Sum() / span;
                queue.Dequeue();
            }          


        }
    }
}
