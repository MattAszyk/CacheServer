using System;
using System.Collections;

namespace CacheServer.Data
{
    public class Database : IDatabase
    {
        private readonly Hashtable collection;
        private DatabaseItem head;
        private DatabaseItem tail;
        private int approximatelyCurrentSize = 0;
        public string GetSize() => approximatelyCurrentSize.ToString();
        private readonly int maximumSize = 128 * 1024 * 1024;
        
        private static readonly object locker = new object();
        public Database()
        {
            /*
             * Database is using Least recently used cache algorithm. 
             * */
            collection = new Hashtable();
            LinkedListBuilder();          
        }

        private void LinkedListBuilder()
        {
            head = new DatabaseItem { Key = "", Value = "" };
            tail = new DatabaseItem { Key = "", Value = "" };
            head.Previous = tail;
            tail.Next = head;
            approximatelyCurrentSize = 0;
        }
        public string Get(string key)
        {
            lock (locker)
                return GetFromDatabase(key);

        }
        public void Set(string key, string value)
        {
            lock (locker)
                AddToDatabase(key, value);
        }



        private void MoveToHead(DatabaseItem item)
        {
            if (item == tail)
            {
                //End node from LinkedList
                item.Next.Previous = null;
                tail = item.Next;
                head.Next = item;
                head = item;
            }
            else if (item != head)
            {
                //Inside node from LinkedList
                item.Previous.Next = item.Next;
                item.Next.Previous = item.Previous;
                head.Next = item;
                item.Previous = head;
                head = item;
            }
        }

        private string GetFromDatabase(string key) //O(1) in most cases
        {
            var item = (DatabaseItem)collection[key];
            if (item is not null)
            {
                MoveToHead(item);
            }
            return item?.Value;
        }
       

        private void AddToDatabase(string key, string value) //O(1) in most cases
        {
            //I'm assuming that Hashtables key is hashed into int value
            int size = sizeof(char) * (value.Length + key.Length) + sizeof(int);
            if (size >= maximumSize) return;
            approximatelyCurrentSize += size;

            //Evicting space until new value could be stored
            while (approximatelyCurrentSize > maximumSize)
                Cleaner();


            //Key exist, updating value
            if (collection.Contains(key))
            {
                var item = (DatabaseItem)collection[key];
                item.Value = value;
                MoveToHead(item);
            }
            else //New key.
            {
                var item = new DatabaseItem { Next = null, Previous = head, Key = key, Value = value };
                collection.Add(key, item);
                head.Next = item;
                head = item;
            }

        }


        private void Cleaner()
        {
            //always trimming database by 10% of items;
            int itemToRemove = collection.Count;
            if (itemToRemove == 2)
            {
                //Special case: rebuild whole database to beginning.
                collection.Remove(head.Key);
                collection.Remove(tail.Key);
                LinkedListBuilder();
            }
            else
            {
                itemToRemove /= 10;
                while (itemToRemove >= 0)
                {
                    approximatelyCurrentSize -= sizeof(char) * (head.Key.Length + tail.Value.Length) + sizeof(int);
                    collection.Remove(tail.Key);
                    tail = tail.Next;
                    itemToRemove--;
                }
            }
            //Run GC after cleaning
            GC.Collect();
        }
    }
}
