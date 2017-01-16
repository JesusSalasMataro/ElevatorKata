using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    public interface MachineWorking
    {
        void OpenDoors();
        void CloseDoors();
        void StartEngine();
        void StopEngine();
        bool EngineIsOk();
        bool CheckSensor(int floor);
    }
}
