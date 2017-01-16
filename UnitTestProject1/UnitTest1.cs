using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private MachineWorking _machineWorking;
        private Mock<MachineWorking> _machineWorkingMock;

        [TestInitialize]
        public void Init()
        {
            _machineWorkingMock = new Mock<MachineWorking>();
            _machineWorking = _machineWorkingMock.Object;

            _machineWorkingMock.Setup(m => m.EngineIsOk()).Returns(true);
            _machineWorkingMock.Setup(m => m.CheckSensor(It.IsAny<int>())).Returns(true);
        }

        [TestMethod]
        public void Elevator_Starts_At_Ground_Floor()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);

            // ACT
            int expectedFloor = elevator.GROUND_FLOOR;
            int actualFloor = elevator.CurrentFloor;

            // ASSERT
            Assert.AreEqual(expectedFloor, actualFloor);
        }

        [TestMethod]
        public void Elevator_Has_A_Correct_State()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);

            // ACT
            ElevatorState expectedState = ElevatorState.Waiting;
            ElevatorState actualState = elevator.State;

            // ASSERT
            Assert.AreEqual(expectedState, actualState);
        }

        [TestMethod]
        public void Elevator_Closes_Doors_And_Starts_Engine_After_Pressing_A_Button()
        {
            // ARRANGE - CALLBACK ASSERTS         
            Elevator elevator = new Elevator(_machineWorking);
            elevator.State = ElevatorState.Waiting;
            int callOrder = 0;
            _machineWorkingMock.Setup(m => m.CloseDoors()).Callback(() => Assert.AreEqual(0, callOrder++));
            _machineWorkingMock.Setup(m => m.StartEngine()).Callback(() => Assert.AreEqual(1, callOrder++));

            // ACT
            int buttonNumber = 1;
            elevator.ButtonPress(buttonNumber);

            // ASSERT
            _machineWorkingMock.Verify(e => e.CloseDoors(), Times.Once);
            _machineWorkingMock.Verify(e => e.StartEngine(), Times.Once);
        }

        [TestMethod]
        public void Elevator_Open_Doors_And_Stops_Engine_After_Reaching_Requested_Floor()
        {
            // ARRANGE - CALLBACK ASSERTS         
            Elevator elevator = new Elevator(_machineWorking);
            int callOrder = 0;
            _machineWorkingMock.Setup(m => m.StopEngine()).Callback(() => Assert.AreEqual(0, callOrder++));
            _machineWorkingMock.Setup(m => m.OpenDoors()).Callback(() => Assert.AreEqual(1, callOrder++));

            // ACT
            int floorNumber = 1;
            elevator.VisitFloor(floorNumber);

            // ASSERT
            _machineWorkingMock.Verify(e => e.StopEngine(), Times.Once);
            _machineWorkingMock.Verify(e => e.OpenDoors(), Times.Once);
        }

        [TestMethod]
        public void Elevator_Visits_Floors_In_Order()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);

            // ACT
            elevator.ButtonPress(4);
            elevator.ButtonPress(2);
            elevator.ButtonPress(5);

            elevator.VisitNextFloor();

            // ASSERT
            _machineWorkingMock.Verify(e => e.StopEngine(), Times.Exactly(1));
            _machineWorkingMock.Verify(e => e.OpenDoors(), Times.Exactly(1));

            CollectionAssert.AreEqual(elevator.FloorsToVisit, new List<int> { 4, 5 });
        }

        [TestMethod]
        public void Elevator_Does_Not_Change_Direction()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);

            // ACT
            elevator.ButtonPress(4);
            elevator.ButtonPress(5);

            elevator.VisitNextFloor();
            elevator.ButtonPress(3);
            elevator.VisitNextFloor();

            // ASSERT
            _machineWorkingMock.Verify(e => e.StopEngine(), Times.Exactly(2));
            _machineWorkingMock.Verify(e => e.OpenDoors(), Times.Exactly(2));

            CollectionAssert.AreEqual(elevator.FloorsToVisit, new List<int> { 3 });
        }

        [TestMethod]
        public void When_Elevator_Engine_Breaks_Then_Elevator_Stops_And_State_Equals_MaintenanceState()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);
            _machineWorkingMock.Setup(m => m.EngineIsOk()).Returns(false);

            // ACT
            elevator.ButtonPress(1);

            // ASSERT
            Assert.AreEqual(ElevatorState.RequiresMaitenance, elevator.State);
            CollectionAssert.AreEqual(elevator.FloorsToVisit, new List<int> { });
        }

        [TestMethod]
        public void Elevator_In_MaintenanceState_Does_Not_Respond_On_Button_Pressed()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);
            _machineWorkingMock.Setup(m => m.EngineIsOk()).Returns(false);

            // ACT
            elevator.ButtonPress(1);

            // ASSERT
            CollectionAssert.AreEqual(elevator.FloorsToVisit, new List<int> { });
            _machineWorkingMock.Verify(e => e.CloseDoors(), Times.Never);
            _machineWorkingMock.Verify(e => e.StartEngine(), Times.Never);
        }

        [TestMethod]
        public void Elevator_With_Sensor_Malfunction_Goes_Into_RequiresMaintenance_State()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);
            _machineWorkingMock.Setup(m => m.CheckSensor(It.IsAny<int>())).Returns(false);       

            // ACT    
            elevator.ButtonPress(1);
            elevator.ButtonPress(3);

            elevator.VisitNextFloor();
            elevator.VisitNextFloor();

            // ASSERT
            Assert.AreEqual(ElevatorState.RequiresMaitenance, elevator.State);
        }

        [TestMethod]
        public void Elevator_General_Working_Extra_Test()
        {
            // ARRANGE
            Elevator elevator = new Elevator(_machineWorking);

            // ACT
            elevator.ButtonPress(2);
            elevator.ButtonPress(6);
            elevator.ButtonPress(3);

            elevator.VisitNextFloor();
            elevator.VisitNextFloor();
            elevator.ButtonPress(5);
            elevator.ButtonPress(2);
            elevator.ButtonPress(1);
            elevator.VisitNextFloor();
            elevator.VisitNextFloor();
            elevator.ButtonPress(2);
            elevator.ButtonPress(4);
            elevator.VisitNextFloor();

            // ASSERT
            elevator.FloorsToVisit.Sort();
            CollectionAssert.AreEqual(elevator.FloorsToVisit, new List<int> { 1, 2 });
        }
    }
}
