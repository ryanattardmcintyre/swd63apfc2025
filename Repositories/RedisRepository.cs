using Newtonsoft.Json;
using PFC2025SWD63A.Models;
using StackExchange.Redis;

namespace PFC2025SWD63A.Repositories
{
    public class RedisRepository
    {
        private IDatabase _database;
        public RedisRepository(string connectionString, string username, string password) {
            try
            {
                var muxer = ConnectionMultiplexer.Connect(
                        new ConfigurationOptions
                        {
                            EndPoints = { { connectionString } },
                            User = username,
                            Password = password
                        }
                    );

                _database = muxer.GetDatabase();
            }
            catch { }

        }

        public List<Menu> GetMenus()
        {
           
           string menusStr =  _database.StringGet("menus").IsNull? "": _database.StringGet("menus")!;
           if (menusStr == "") return new List<Menu>();
           var listOfMenus = JsonConvert.DeserializeObject<List<Menu>>(menusStr);
           return listOfMenus!;
        }

        public void AddMenu(Menu menu) {
            var list = GetMenus();
           // if (DoesMenuExist(menu.Title) == false)
            //{
                list.Add(menu);
                string menusUpToDate = JsonConvert.SerializeObject(list);
                _database.StringSet("menus", menusUpToDate, when: When.Always);
           // }
        }

        public bool DoesMenuExist(string menuName) {
            return GetMenus().Count(x => x.Title == menuName) > 0 ? true : false;
        }
    }
}
