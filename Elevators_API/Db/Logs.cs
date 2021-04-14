using System;

namespace Elevators_API.Model.DbModel
{
    public partial class Logs
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
    }
}
