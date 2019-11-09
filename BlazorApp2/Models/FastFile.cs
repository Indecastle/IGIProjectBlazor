using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Models
{
    public class FastFile
    {
        public int Id { get; set; }
        public string Name { get; set; } // название файла
        public string KeyName { get; set; } // Путь к файлу на amazon
        public DateTime DateTime { get; set; } // Время загрузки
    }
}
