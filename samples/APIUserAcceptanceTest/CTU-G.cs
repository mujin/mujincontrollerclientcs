using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;

namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Move towards a work and pick it - transform6d
        public static void CTUG00100()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1000.5, 800, 1050, 1, -89, -89 };

            task.PickAndMove("container1", "camera1", "1Base", GoalType.transform6d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - translatedirection5d
        public static void CTUG00200()
        {
            BinPickingTask task = CTUC00100();

            // For translationdirection5d, X[mm], Y[mm], Z[mm], DX[mm], DY[mm], DZ[mm]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            task.PickAndMove("container1", "camera1", "1Base", GoalType.translationdirection5d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - tool name
        public static void CTUG00300()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            task.PickAndMove("container1", "camera1", "0", GoalType.transform6d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - multiple goals
        public static void CTUG00400()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0,
                                                        150, 850, 500, 0, 0, 0 };

            task.PickAndMove("container1", "camera1", "1Base", GoalType.transform6d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - Invalid box name
        public static void CTUG00500()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            task.PickAndMove("hogehogebox", "camera1", "1Base", GoalType.transform6d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - Invalid sensor name
        public static void CTUG00600()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            task.PickAndMove("container1", "foobarcamera", "1Base", GoalType.transform6d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - Invalid tool name
        public static void CTUG00700()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            task.PickAndMove("container1", "camera1", "hogehoge", GoalType.transform6d, goalpos, 0.2, 60000);
        }

        // Move towards a work and pick it - Invalid speed
        public static void CTUG00800()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            task.PickAndMove("container1", "camera1", "1Base", GoalType.transform6d, goalpos, 1.2, 60000);
        }

        // Move towards a work and pick it - Time out
        public static void CTUG00900()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 300, 900, 500, 0, 0, 0 };

            try
            {
                task.PickAndMove("container1", "camera1", "1Base", GoalType.transform6d, goalpos, 0.2, 100);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("timed out")) throw new Exception("Test case CTUG00900 failed");
            }
        }
    }
}
