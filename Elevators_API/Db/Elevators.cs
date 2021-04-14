namespace Elevators_API.Model.DbModel
{
    public partial class Elevators
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public int Floor { get; set; }

        public virtual Statuses Status { get; set; }
    }
}
