using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;

namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Move robot's joints based on hand position - transform6d
        public static void CTUF00100()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0 };

            task.MoveToHandPosition(goalpos, GoalType.transform6d, "1Base", 0.2, 60000);
        }

        // Move robot's joints based on hand position - translatedirection5d
        public static void CTUF00200()
        {
            BinPickingTask task = CTUC00100();

            // For translationdirection5d, X[mm], Y[mm], Z[mm], DX[mm], DY[mm], DZ[mm]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0 };

            task.MoveToHandPosition(goalpos, GoalType.translationdirection5d, "1Base", 0.2, 60000);
        }

        // Move robot's joints based on hand position - tool name
        public static void CTUF00300()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0 };

            task.MoveToHandPosition(goalpos, GoalType.transform6d, "0", 0.2, 60000);
        }

        // Move robot's joints based on hand position - Multiple goals
        public static void CTUF00400()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0,
                                                        1550, 850, 450, 0, 0, 0 };

            task.MoveToHandPosition(goalpos, GoalType.transform6d, "1Base", 0.2, 60000);
        }

        // Move robot's joints based on hand position - Invalid goal position
        public static void CTUF00500()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 0, 600, 0, 0, 0 };

            task.MoveToHandPosition(goalpos, GoalType.transform6d, "1Base", 0.2, 60000);
        }

        // Move robot's joints based on hand position - Invalid tool name
        public static void CTUF00600()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0 };

            try
            {
                task.MoveToHandPosition(goalpos, GoalType.transform6d, "hogehoge", 0.2, 60000);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("failed to find manipulator with name")) throw new Exception("Test case CTUF00600 failed");
            }
        }

        // Move robot's joints based on hand position - Invalid speed
        public static void CTUF00700()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0 };

            try
            {
                task.MoveToHandPosition(goalpos, GoalType.transform6d, "1Base", 1.2, 60000);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("failed to find manipulator with name")) throw new Exception("Test case CTUF00600 failed");
            }
        }

        // Move robot's joints based on hand position - Time out
        public static void CTUF00800()
        {
            BinPickingTask task = CTUC00100();

            // For transform6d, X[mm], Y[mm], Z[mm], RX[deg], RY[deg], RZ[deg]
            List<double> goalpos = new List<double>() { 1500, 900, 500, 0, 0, 0 };

            try
            {
                task.MoveToHandPosition(goalpos, GoalType.transform6d, "1Base", 0.2, 100);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("timed out")) throw new Exception("Test case CTUF00600 failed");
            }
        }

    }
}
