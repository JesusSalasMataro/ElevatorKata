using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject1
{
    public class Elevator
    {
        public readonly int GROUND_FLOOR = 0;
        public readonly int NO_FLOOR = -99;
       
        private MachineWorking _machineWorking;

        public List<int> FloorsToVisit { get; set; }
        public ElevatorState State { get; set; }
        public int CurrentFloor { get; set; }

        public Elevator(MachineWorking machineWorking)
        {
            _machineWorking = machineWorking;

            CurrentFloor = GROUND_FLOOR;
            State = ElevatorState.Waiting;
            FloorsToVisit = new List<int>();
        }


        #region "Public methods"

        public void ButtonPress(int buttonNumber)
        {
            if (CheckEngine() && !FloorsToVisitContainsFloor(buttonNumber))
            {
                FloorsToVisit.Add(buttonNumber);
            }
            
            if (State == ElevatorState.Waiting)
            {
                _machineWorking.CloseDoors();
                _machineWorking.StartEngine();

                if (FloorsToVisit.Min() > CurrentFloor)
                {
                    State = ElevatorState.GoingUp;
                }
                else
                {
                    State = ElevatorState.GoingDown;
                }
            }
        }

        public void VisitFloor(int floorNumber)
        {
            _machineWorking.StopEngine();            
            _machineWorking.OpenDoors();

            CurrentFloor = floorNumber;
        }

        public void VisitNextFloor()
        {
            if (CheckEngine() && ThereAreFloorsToVisit())
            {
                int nextFloorToVisit = NextFloorToVisit();

                if (nextFloorToVisit != NO_FLOOR)
                {
                    PassByNotSelectedFloors(CurrentFloor, nextFloorToVisit);
                    VisitFloor(nextFloorToVisit);
                    RemoveVisitedFloorFromList(nextFloorToVisit);
                }
                else
                {
                    State = ElevatorState.Waiting;
                }
            }
        }

        public bool CheckEngine()
        {
            if (!_machineWorking.EngineIsOk())
            {
                State = ElevatorState.RequiresMaitenance;
            }

            return State != ElevatorState.RequiresMaitenance;
        }

        #endregion

        #region "Private methods"

        private int NextFloorToVisit()
        {
            int nextFloorToVisit = NO_FLOOR;

            if (State == ElevatorState.GoingDown)
            {
                if (ThereAreLowerFloorsToVisit())
                {
                    nextFloorToVisit = FloorsToVisit.Where(f => f < CurrentFloor).Max();
                }
                else if (ThereAreUpperFloorsToVisit())
                {
                    nextFloorToVisit = FloorsToVisit.Where(f => f > CurrentFloor).Min();
                    State = ElevatorState.GoingUp;
                }
            }
            else if (State == ElevatorState.GoingUp)
            {
                if (ThereAreUpperFloorsToVisit())
                {
                    nextFloorToVisit = FloorsToVisit.Where(f => f > CurrentFloor).Min();
                }
                else if (ThereAreLowerFloorsToVisit())
                {
                    nextFloorToVisit = FloorsToVisit.Where(f => f < CurrentFloor).Max();
                    State = ElevatorState.GoingDown;
                }
            }

            return nextFloorToVisit;
        }

        private bool ThereAreLowerFloorsToVisit()
        {
            return FloorsToVisit.Where(f => f < CurrentFloor).Count() > 0;
        }

        private bool ThereAreUpperFloorsToVisit()
        {
            return FloorsToVisit.Where(f => f > CurrentFloor).Count() > 0;
        }

        private bool ThereAreFloorsToVisit()
        {
            return FloorsToVisit.Count() > 0;
        }

        private void PassByNotSelectedFloors(int CurrentFloor, int nextFloorToVisit)
        {
            int increaseFloorValue = 1;

            if (nextFloorToVisit < CurrentFloor)
            {
                increaseFloorValue = -1;
            }

            for (int floor = CurrentFloor + increaseFloorValue; floor != nextFloorToVisit; floor += increaseFloorValue)
            {
                if (!CheckFloorSensor(floor))
                {
                    State = ElevatorState.RequiresMaitenance;
                }
            }
        }

        private bool CheckFloorSensor(int floor)
        {
            return _machineWorking.CheckSensor(floor);
        }

        private void RemoveVisitedFloorFromList(int lastVisitedFloor)
        {
            List<int> floorsToRemove = FloorsToVisit.Where(f => f == lastVisitedFloor).ToList();
            FloorsToVisit = FloorsToVisit.Except(floorsToRemove).ToList();
        }

        private bool FloorsToVisitContainsFloor(int buttonNumber)
        {
            return FloorsToVisit.Contains(buttonNumber);
        }

        #endregion
        
    }
}