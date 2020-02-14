using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace poks41.Models
{
    public class ViewCntClass: TableEntity
    {
        public Int64 Count { get; set; }

        public ViewCntClass()
        {
            PartitionKey = "poks43";
            RowKey = "main";
        }
    }
}
