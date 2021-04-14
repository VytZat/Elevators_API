using Elevators_API.Model.DbModel;
using System.Collections.Generic;
using System.Linq;

namespace Elevators_API.Controllers.NonApiControllers
{
    public class InitializationController
    {
        private readonly ElevatorsDBContext _dbContext;

        private readonly int _elevatorsCount;

        public InitializationController(ElevatorsDBContext dbContext, int elevatorsCount)
        {
            _dbContext = dbContext;
            _elevatorsCount = elevatorsCount;

            Initialize();
        }

        public void Initialize()
        {
            if (Program.dbInitialized)
                return;

            if (_dbContext.Database.EnsureCreated())
            {
                _dbContext.Statuses.AddRange(new Statuses[]
                {
                new Statuses()
                {
                    Id = 1,
                    Status = "Idle"
                },
                new Statuses()
                {
                    Id = 2,
                    Status = "Doors closing"
                },
                new Statuses()
                {
                    Id = 3,
                    Status = "Doors opening"
                },
                new Statuses()
                {
                    Id = 4,
                    Status = "Moving up"
                },
                new Statuses()
                {
                    Id = 5,
                    Status = "Moving down"
                }
                });
            }

            List<Elevators> elevators = _dbContext.Elevators.ToList();
            
            if (elevators.Count == _elevatorsCount)
            {
                return;
            }

            if (elevators.Count < _elevatorsCount)
            {
                int elevatorsToAdd = _elevatorsCount - elevators.Count;
                for (int i = 0; i < elevatorsToAdd; i++)
                {
                    _dbContext.Elevators.Add(new Elevators
                    {
                        StatusId = 1,
                        Floor = 1
                    });
                }
            }

            if (elevators.Count > _elevatorsCount)
            {

            }

            _dbContext.SaveChanges();

            Program.dbInitialized = true;
        }
    }
}
