using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            //throw new NotImplementedException("Fill in username and password and remove this exception.");

            string mujinIpAddress = "https://controller.mujin.co.jp/";
            string scenePrimaryKey = "irex2013.mujin.dae";
            string username = "knowledge3dp";
            string password = "hihipya3meamojIpzav8";

            ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            controllerClient.Login();
            SceneResource scene = controllerClient.GetScene(scenePrimaryKey);
        }
    }
}
