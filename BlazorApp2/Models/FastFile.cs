using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Models
{
    public class FastFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } // название файла
        public string KeyName { get; set; } // Путь к файлу на amazon
        public DateTime DateTime { get; set; } // дата загрузки
        public DateTime EndTime { get; set; } // дата смерти

        public FastFile() { 
            Id = Guid.NewGuid(); 
        }

        public FastFile(string name, TimeSpan tspan)
        {
            Id = Guid.NewGuid();
            Name = name; DateTime = DateTime.Now;
            EndTime = DateTime.Add(tspan);
            KeyName = $"{Name}-{Id}";
        }

        public FastFile(Guid id, TimeSpan tspan)
        {
            Id = id;
            Name = id.ToString(); DateTime = DateTime.Now;
            EndTime = DateTime.Add(tspan);
            KeyName = $"folder-{Name}";
        }

        public void UpdateSpan(TimeSpan tspan)
        {
            this.DateTime = DateTime.Now;
            EndTime = DateTime.Add(tspan);
        }

        public void UpdateKeyName()
        {
            KeyName = $"{Name}-{Id}";
        }
    }
}
