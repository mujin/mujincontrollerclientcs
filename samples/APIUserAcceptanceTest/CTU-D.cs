using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;

namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Get robot's joint values
        public static void CTUD00100()
        {
            BinPickingTask task = CTUC00100();

            RobotState state = task.GetJointValues();
            Console.WriteLine("[Joint names]" + string.Join(",", state.jointNames));
            Console.WriteLine("[Joint values]" + string.Join(",", state.jointValues));
        }

        // Get robot's joint values - Time out
        public static void CTUD00200()
        {
            BinPickingTask task = CTUC00100();

            try
            {
                RobotState state = task.GetJointValues(100);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("timed out")) throw new Exception("Test case CTUD00200 failed");
            }
        }
    }
}
