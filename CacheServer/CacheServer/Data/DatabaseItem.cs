namespace CacheServer.Data
{
    public class DatabaseItem
    {   
        public string Key { get; set; }
        public string Value { get; set; }
        public DatabaseItem Next { get; set; }
        public DatabaseItem Previous { get; set; }
    }
}
