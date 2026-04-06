using SQLite;

namespace Fluence.Models
{
    class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, NotNull]
        public string Name { get; set; }
        
        public bool IsSystem { get; set; }
    }
}
