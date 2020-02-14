using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace poks41.Models
{
    public class TrackClass: TableEntity
    {
        //public String Album_ID { get; set; }   PartitionKey
        //public String Track_No { get; set; }   RowKey

        public String Album { get; set; }
        public String Name { get; set; }
        public String Artist { get; set; }

        public int LikeUp { get; set; }
        public int LikeDown { get; set; }

        public TrackClass(){
            LikeUp = 0;
            LikeDown = 0;

            Artist = "Unknown";
            Album = "Unknown";
            Name = "Unknown";
        }
    }


}
