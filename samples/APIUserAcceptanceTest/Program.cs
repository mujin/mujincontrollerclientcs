using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace APIUserAcceptanceTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // ControllerClient
            //CTUA00100();
            //CTUA00200();
            //CTUA00300();
            //CTUA00400();
            //CTUA00401();

            // GetScene
            //CTUB00100();
            //CTUB00200();

            // GetOrCreateTaskFromName
            //CTUC00100();
            //CTUC00200_CTUC00300();
            //CTUC00400_CTUC00500();

            // GetJointValues
            //CTUD00100();
            //CTUD00200();

            // MoveJoints
            //CTUE00100();
            //CTUE00200();
            //CTUE00300();
            //CTUE00400();
            //CTUE00500();
            //CTUE00600();

            // MoveToHandPosition
            //CTUF00100();
            //CTUF00200();
            //CTUF00300();
            //CTUF00400();
            //CTUF00500();
            //CTUF00600();
            //CTUF00700();
            //CTUF00800();

            // PickAndMove
            //CTUG00100();
            //CTUG00200();
            //CTUG00300();
            //CTUG00400();
            //CTUG00500();
            //CTUG00600();
            //CTUG00700();
            //CTUG00800();
            //CTUG00900();
            RosenParams();
        }
        static void RosenParams()
        {
            Mujin.BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 100,240,700,1,1,1 };

            task.PickAndMove(
                "container2",   // boxname
                "camera1",      // sensorname
                "1Base",        // toolname
                Mujin.GoalType.translationdirection5d,  // goaltype
                goalpos,
                0.1);
        }
    }
}
