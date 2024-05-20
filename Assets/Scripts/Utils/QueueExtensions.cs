using System.Collections.Generic;
using UnityEngine;

namespace BeatWalker.Utils
{
    public static class QueueExtensions
    {
        public static void Remove<T>(this Queue<T> queue, T itemToRemove) where T : class
        {
            var items = queue.ToArray();
            queue.Clear();

            foreach (var item in items)
            {
                if (item == itemToRemove)
                    continue;
                
                queue.Enqueue(item);
            }
        }
            
    }
}