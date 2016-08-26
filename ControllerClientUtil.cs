using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fastJSON;

namespace Mujin
{
    public enum HttpMethod { GET, POST, PUT }
    public enum GoalType { transform6d, translationdirection5d }

    public class ClientValidator
    {
        public static void ValidateClearance(double clearance)
        {
            if (clearance < 5 || 1000 < clearance)
                throw new ClientException("Invalid parameter. Clearance out of range.");
        }

        public static void ValidateSpeed(double speed)
        {
            if (speed < 0.001 || 1.00 < speed)
                throw new ClientException("Invalid parameter. Speed out of range.");
        }

        public static void ValidateJointValues(List<double> jointValues, List<int> jointIndices)
        {
            if (jointValues == null || jointIndices == null)
                throw new ClientException("Invalid parameters. jointValues or jointIndices is null.");

            if (jointValues.Count != jointIndices.Count)
                throw new ClientException("Invalid parameters. Joint count does not match.");

            int pointer = 0;

            foreach (int index in jointIndices)
            {
                if (index < 0 || 7 < index)
                    throw new ClientException("Invalid parameters. Joint index out of bound.");

                double value = jointValues[pointer++];

                // IREX2013 validations. 
                // Mujin controller should perform this validation. 
                switch (index)
                {
                    case 0: // Linear rail
                        if (value < -1605 || 0 < value)
                            throw new ClientException("Invalid parameters. J7 value out of range.");
                        break;
                    case 1: // J1
                        if (value < -160 || 160 < value)
                            throw new ClientException("Invalid parameters. J1 value out of range.");
                        break;
                    case 2: // J2
                        if (value < -120 || 120 < value)
                            throw new ClientException("Invalid parameters. J2 value out of range.");
                        break;
                    case 3: // J3
                        if (value < -128 || 136 < value)
                            throw new ClientException("Invalid parameters. J3 value out of range.");
                        break;
                    case 4: // J5
                        if (value < -160 || 120 < value)
                            throw new ClientException("Invalid parameters. J5 value out of range.");
                        break;
                    case 5: // J6
                        if (value < -180 || 180 < value)
                            throw new ClientException("Invalid parameters. J6 value out of range.");
                        break;
                    case 6: // Servo chuck
                        //if (value < -18 || 33 < value)
                        //throw new ClientException("Invalid parameters. J8 value out of range.");
                        break;
                    default:
                        throw new NotSupportedException("Unexpected error.");
                }
            }
        }

        public static void ValidateGoalPositions(List<double> goals, GoalType goalType)
        {
            if (goals == null) throw new ClientException("Invalid parameters. goals is null.");
            if (goals.Count % 6 != 0) throw new ClientException("Invalid parameters. Cannot devide goals size by 7. size = " + goals.Count);

            // TBD
        }
    }

    public class WorkProperties
    {
        public string objectName{get; set;}
        public List<double> goalPosition { get; set; }

        public WorkProperties(string objectName, List<double> goalPosition)
        {
            this.objectName = objectName;
            this.goalPosition = goalPosition;
        }
    }

    [Serializable()]
    public class ClientException : System.Exception
    {
        public ClientException() : base() { }
        public ClientException(string message) : base(message) { }
        public ClientException(string message, System.Exception inner) : base(message, inner) { }
        protected ClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
}
