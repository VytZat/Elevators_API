using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Elevators_API.Controllers.NonApiControllers;
using System.Threading;
using Elevators_API.Model.DbModel;
using Elevators_API.Models;
using System.Threading.Tasks;

namespace Elevators_API.Controllers
{
    [ApiController]
    [Route("api/elevators")]
    public class ElevatorsController : ControllerBase
    {
        private readonly ElevatorsDBContext _dbContext;

        //From configuration - "appsettings.json"
        private readonly int _floorsCount;
        private readonly int _elevatorsCount;

        public ElevatorsController(ElevatorsDBContext dbContext, IConfiguration configuration = null)
        {
            _dbContext = dbContext;
            if (configuration != null)
            {
                _floorsCount = configuration.GetValue<int>("BuildingConfig:FloorsCount");
                if (_floorsCount < 2)
                {
                    _floorsCount = 2;
                }

                _elevatorsCount = configuration.GetValue<int>("BuildingConfig:ElevatorsCount");
                if (_elevatorsCount < 2)
                {
                    _elevatorsCount = 2;
                }
            }
            else
            {
                _floorsCount = 20;
                _elevatorsCount = 2;
            }

            InitializationController initController = new InitializationController(_dbContext, _elevatorsCount);
        }

        #region Publicly accessible methods
        //returns specified elevator's info (returns info of all elevators if id is not specified)
        // specify id example: .../api/elevators/1
        [HttpGet("{elevatorId=0}")]
        public async Task<ActionResult<IEnumerable<ElevatorResult>>> GetElevatorInfo(int elevatorId)
        {
            List<Elevators> elevators = new List<Elevators>();
            List<ElevatorResult> result = new List<ElevatorResult>();

            await Task.Run(() =>
            {
                if (elevatorId == 0)
                {
                    elevators = _dbContext.Elevators.ToList();
                }
                else if (elevatorId > 0)
                {
                    elevators = _dbContext.Elevators.Where(x => x.Id == elevatorId).ToList();
                }



                result = elevators.Select<Elevators, ElevatorResult>(x => new ElevatorResult
                {
                    ID = x.Id,
                    Status = x.Status.Status,
                    Floor = x.Floor
                }).ToList();
            });

            return Ok(result);
        }

        [Route("logs")]
        [HttpGet]
        public IEnumerable<Logs> GetLogs()
        {
            return _dbContext.Logs.ToList();
        }

        [HttpPost]
        public async Task<ActionResult<string>> CallElevator([FromHeader]int elevatorId, [FromHeader]int floor)
        {
            if (floor < 1 || floor > _floorsCount)
            {
                //floor is non existing
                return NotFound("Specified floor does not exist");
            }

            Elevators elevator = _dbContext.Elevators.FirstOrDefault(e => e.Id == elevatorId);

            if (elevator == null)
            {
                //elevator not found
                return NotFound("Specified elevator does not exist");
            }

            string logMessage = $"Elevator with id:{elevator.Id} was called to floor:{floor}";

            AddLog(logMessage);

            await GoToFloor(elevator, floor);

            return Ok($"Elevator (id:{elevatorId}) has arrived to floor:{floor}");
        }
        #endregion

        #region NonPublicMethods
        private void ChangeElevatorStatus(Elevators elevator, int statusID)
        {
            if (elevator.StatusId != statusID)
            {
                var statuses = _dbContext.Statuses.ToList();

                if (!statuses.Any(f => f.Id == statusID))
                {
                    // specified status does not exist
                    return;
                }

                string logMessage = $"Elevator's with id:{elevator.Id}, status has changed from '{elevator.Status.Status}' ";

                elevator.StatusId = statusID;

                logMessage += $" to '{statuses.First(f => f.Id == statusID).Status}'";

                AddLog(logMessage);

                _dbContext.SaveChanges();
            }
        }

        private void ChangeFloor(Elevators elevator, int floor)
        {
            if (elevator.Floor != floor)
            {
                string logMessage = $"Elevator with id:{elevator.Id} moved from floor:{elevator.Floor} to floor:{floor}";

                elevator.Floor = floor;

                AddLog(logMessage);
                // TODO: Log floor change
                _dbContext.SaveChanges();
            }
        }

        private void AddLog(string message)
        {
            _dbContext.Logs.Add(new Logs
            {
                Description = message,
                Time = DateTime.Now
            });

            _dbContext.SaveChanges();
        }

        private async Task GoToFloor(Elevators elevator, int floor)
        {
            //Elevator statuses: 1-"Idle" 2-"Doors closing" 3-"Doors opening" 4-"Moving up" 5-"Moving down"
            if (elevator.Floor != floor)
            {
                bool up = elevator.Floor < floor;
                while (elevator.Floor != floor)
                {
                    if (up)
                    {
                        ChangeElevatorStatus(elevator, 4);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        ChangeFloor(elevator, elevator.Floor + 1);
                    }
                    else
                    {
                        ChangeElevatorStatus(elevator, 5);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        ChangeFloor(elevator, elevator.Floor - 1);
                    }
                }
            }

            ChangeElevatorStatus(elevator, 3);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            ChangeElevatorStatus(elevator, 2);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            ChangeElevatorStatus(elevator, 1);
        }
        #endregion
    }
}
