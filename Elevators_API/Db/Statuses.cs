using System.Collections.Generic;

namespace Elevators_API.Model.DbModel
{
    public partial class Statuses
    {
        public Statuses()
        {
            Elevators = new HashSet<Elevators>();
        }

        public int Id { get; set; }
        public string Status { get; set; }

        public virtual ICollection<Elevators> Elevators { get; set; }
    }
}
