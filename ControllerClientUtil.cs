using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mujin
{
    public enum HttpMethod { GET, POST, PUT }

    // This util class should be replaced with fastJson.
    public class Command
    {
        private string command = null;

        public Command Add(string key, string value)
        {
            command = (command == null) ? string.Format("\"{0}\": \"{1}\"", key, value)
                : command += string.Format(", \"{0}\": \"{1}\"", key, value);
            return this;
        }

        public Command Add(string key, int value)
        {
            command = (command == null) ? string.Format("\"{0}\": {1}", key, value)
                : command += string.Format(", \"{0}\": {1}", key, value);
            return this;
        }

        public Command Add(string key, float value)
        {
            command = (command == null) ? string.Format("\"{0}\": {1}", key, value)
                : command += string.Format(", \"{0}\": {1}", key, value);
            return this;
        }

        public Command Add<T>(string key, List<T> objects)
        {
            string arrayString = null;

            foreach (T obj in objects)
            {
                arrayString += (arrayString == null) ? obj.ToString() : string.Format(", {0}", obj.ToString());
            }

            command += (command == null) ? string.Format("\"{0}\": [{1}]", key, arrayString)
                : string.Format(", \"{0}\": [{1}]", key, arrayString);

            return this;
        }

        public Command Add(string key, Command addCommand)
        {
            command = (command == null) ? string.Format("\"{0}\": {1}", key, addCommand.GetString())
                : command += string.Format(", \"{0}\": {1}", key, addCommand.GetString());
            return this;
        }

        public string GetString()
        {
            return string.Format("{{{0}}}", this.command);
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
