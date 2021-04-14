using Elevators_API.Controllers;
using Elevators_API.Model.DbModel;
using Moq;
using System;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Elevators_API.Models;

namespace Elevators_API.Tests
{       
    public class DbTest
    {
        protected DbTest(DbContextOptions<ElevatorsDBContext> contextOptions)
        {
            ContextOptions = contextOptions;

            Seed();
        }

        protected DbContextOptions<ElevatorsDBContext> ContextOptions { get; }

        private void Seed()
        {
            using (var context = new ElevatorsDBContext(ContextOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var elevators = new List<Elevators>()
                {
                    new Elevators
                    {
                        Id = 1,
                        StatusId = 1,
                        Floor = 1
                    },
                    new Elevators
                    {
                        Id = 2,
                        StatusId = 1,
                        Floor = 1
                    }
                };

                var statuses = new List<Statuses>()
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
                };

                var logs = new List<Logs>();

                context.Elevators.AddRange(elevators);
                context.Statuses.AddRange(statuses);

                context.SaveChanges();
            }
        }
    }

    public class API_Tests : DbTest
    {
        private static T GetObjectResultContent<T>(ActionResult<T> result)
        {
            return (T)((ObjectResult)result.Result).Value;
        }

        public API_Tests() : base(new DbContextOptionsBuilder<ElevatorsDBContext>()
                .UseSqlite("Filename=Test.db").UseLazyLoadingProxies()
                .Options)
        {

        }

        [Fact]
        public void GetElevatorById()
        {
            using (var dbContext = new ElevatorsDBContext(ContextOptions))
            {
                var controller = new ElevatorsController(dbContext);

                var controllerResult = GetObjectResultContent<IEnumerable<ElevatorResult>>(controller.GetElevatorInfo(2).Result).ToList();

                //Fail if controller call returned list with NOT one object
                Assert.Single(controllerResult);

                //Fail if retrieved elevators id is not 2
                Assert.Equal(2, controllerResult[0].ID);
            }
        }

        [Fact]
        public async void CallElevator()
        {
            using (var dbContext = new ElevatorsDBContext(ContextOptions))
            {
                var elevatorToCall = 1;
                var floorToCallTo = 5;

                var controller = new ElevatorsController(dbContext);

                await controller.CallElevator(elevatorToCall, floorToCallTo);

                var result = GetObjectResultContent<IEnumerable<ElevatorResult>>(controller.GetElevatorInfo(1).Result).ToList();

                // Fail if elevator is not in 5th floor
                Assert.Equal(floorToCallTo, result[0].Floor);

                // Check logging (should contain 9 log records when calling to 5th floor)
                var logs = controller.GetLogs();

                // Fail if log records count is not 9
                Assert.Equal(9, logs.ToList().Count);
            }
        }
    }
}
