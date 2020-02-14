using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace poks41.Models
{
    public class ProjectClass: TableEntity
    {
        public String Name { get; set; }
        public String Email { get; set; }
        public String Description { get; set; }

        public ProjectClass()
        {
        }
    }
}
