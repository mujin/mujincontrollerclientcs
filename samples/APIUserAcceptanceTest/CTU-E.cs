using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;


namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Move robot's joints
        public static void CTUE00100()
        {
            BinPickingTask task = CTUC00100();

            // J7(linear rail), J1, J2, J3, J5, J6, J8(servo hand)
            List<double> jointValues = new List<double>() { -10.0, 0.0, 50.0, -100.0, 0.0, 0.0, 0.0 };
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };    

            task.MoveJoints(jointValues, jointIndices, 30, 0.2, "");
        }

        // Move robot's joints - Invalid Joint Values
        public static void CTUE00200()
        {
            BinPickingTask task = CTUC00100();

            // J7 (index=0) cannot go positive.
            List<double> jointValues = new List<double>() { -10.0, 0.0, 70.0, -140.0, 0.0, 0.0, 0.0 };
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 }; 

            try
            {
                task.MoveJoints(jointValues, jointIndices, 30, 0.2, "");
            }
            catch(ClientException ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

        // Move robot's joints - Invalid joint indices
        public static void CTUE00300()
        {
            BinPickingTask task = CTUC00100();

            // J7(linear rail), J1, J2, J3, J5, J6, J8(servo hand)
            List<double> jointValues = new List<double>() { -10.0, 0.0, 50.0, -100.0, 0.0, 0.0 }; // J8 is missing.
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 }; 

            try
            {
                task.MoveJoints(jointValues, jointIndices, 30, 0.2, "");
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Task Input Error")) throw new Exception("Test case CTUE00300 failed");
            }
        }

        // Move robot's joints - Invalid clearance
        public static void CTUE00400()
        {
            BinPickingTask task = CTUC00100();

            // J7(linear rail), J1, J2, J3, J5, J6, J8(servo hand)
            List<double> jointValues = new List<double>() { -10.0, 0.0, 50.0, -100.0, 0.0, 0.0, 0.0 };
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };    

            try
            {
                task.MoveJoints(jointValues, jointIndices, -5.0, 0.2, "");
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Task Input Error")) throw new Exception("Test case CTUE00400 failed");
            }
        }

        // Move robot's joints - Invalid speed
        public static void CTUE00500()
        {
            BinPickingTask task = CTUC00100();

            // J7(linear rail), J1, J2, J3, J5, J6, J8(servo hand)
            List<double> jointValues = new List<double>() { -10.0, 0.0, 50.0, -100.0, 0.0, 0.0, 0.0 };
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };

            try
            {
                task.MoveJoints(jointValues, jointIndices, 30.0, 1.2, "");
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Task Input Error")) throw new Exception("Test case CTUE00500 failed");
            }
        }

        // Move robot's joints - Time out
        public static void CTUE00600()
        {
            BinPickingTask task = CTUC00100();

            // J7(linear rail), J1, J2, J3, J5, J6, J8(servo hand)
            List<double> jointValues = new List<double>() { -10.0, 0.0, 50.0, -100.0, 0.0, 0.0, 0.0 };
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };

            try
            {
                task.MoveJoints(jointValues, jointIndices, 30.0, 1.2, "", 100);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("timed out")) throw new Exception("Test case CTUE00600 failed");
            }
        }
    }
}
