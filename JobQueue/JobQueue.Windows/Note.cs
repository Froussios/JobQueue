using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobQueue
{
    public class Note
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public string Content { get; set; }
    }
}
