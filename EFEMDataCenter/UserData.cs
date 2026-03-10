using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace EFEM.FileUtilities
{
    public enum UserDataQueryCommand
    {
        Command,
        Login,
        Logout,
        AddUser,
        RemoveUser,
        ChangePassword,
        GetUserType,
        VerifyUserType
    }

    public enum UserDataKey
    {
        UserName,
        Password,
        ReturnMessage,
        LoginType,
        AdminPassword,
        NewPassword,
        TypeList,
        ApplicationOwner
    }

    public enum LoginType
    {
        None = -1, //Not login
        Admin,
        Service,
        Engineer,
        Operator,
        SWRD,
    }

    [Serializable]
    public class UserInfo
    {
        public string username, password, type;
        public bool userExist;

        public UserInfo()
        {
            username = "";
            password = "";
            type = "";
            userExist = false;
        }

        public UserInfo(string user, string pw, string user_type, bool exist)
        {
            username = user;
            password = pw;
            type = user_type;
            userExist = exist;
        }
    }

    public class UserXml
    {
        private XmlDocument document = new XmlDocument();
        private string userfile = "";
        private string[] typelist;
        private ArrayList userList = new ArrayList();
        private bool _ParseOk = false;
        protected AbstractFileUtilities fouser = FileUtility.GetUniqueInstance();

        public UserXml(string UserDB)
        {
            userfile = UserDB;
            if (File.Exists(UserDB))
            {
                try
                {
                    this.document.Load(userfile);
                    userList = this.getUserNamePassword();
                    this.typelist = this.getTypeList();
                    if (userList == null)
                    {
                        _ParseOk = false;
                        throw new Exception("No User Login Data from FileUtilities");
                    }
                    else
                    {
                        _ParseOk = true;
                    }
                }
                catch (Exception ee)
                {
                    _ParseOk = false;
                    throw ee;
                }
            }
            else
            {
                _ParseOk = false;
            }
        }

        public UserXml()
        {
            try
            {
                userList = this.fouser.GetUserLoginData();
                this.typelist = this.fouser.GetTypeList();
                if (userList == null)
                    throw new Exception("No User Login Data Exist , Please check with HMI engineer First!!");
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }

        public bool ParseOk
        {
            get
            {
                return _ParseOk;
            }
        }

        private ArrayList getUserNamePassword()
        {
            ArrayList list = new ArrayList();
            // match user name and password 
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                UserInfo user = new UserInfo(null, null, null, false);
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        user.username = attr.Value;
                        user.userExist = true;
                    }
                    else if (attr.Name == "password")
                    {
                        user.password = attr.Value;
                    }
                    else if (attr.Name == "type")
                    {
                        user.type = attr.Value;
                    }
                }
                list.Add(user);
            }
            return list;
        }

        public string[] GetTypeList()
        {
            return typelist;
        }
        private string[] getTypeList()
        {
            string str = "";
            foreach (XmlNode node in document.GetElementsByTagName("HMIType"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "type")
                    {
                        str = attr.Value;
                    }

                }
            }
            return str.Split(',');
        }

        public UserInfo getUserNamePassword(string user)
        {
            // match user name and password 
            foreach (UserInfo info in userList)
            {
                if (info.username == user)
                {
                    return info;
                }
            }
            return new UserInfo("", "", "", false);
        }
        public ArrayList GetAllUserInfo()
        {
            return this.userList;
        }
        public bool AddUser(UserInfo info)
        {
            //check if user exist !
            foreach (UserInfo user in userList)
            {
                if (user.username == info.username)
                    return false;
            }

            // Add a new user.
            XmlElement newHMIUser = document.CreateElement("HMIUser");

            XmlAttribute newUser = document.CreateAttribute("name");
            newUser.Value = info.username;
            newHMIUser.Attributes.Append(newUser);

            XmlAttribute newPw = document.CreateAttribute("password");
            newPw.Value = info.password;
            newHMIUser.Attributes.Append(newPw);

            XmlAttribute newType = document.CreateAttribute("type");
            newType.Value = info.type;
            newHMIUser.Attributes.Append(newType);

            document.DocumentElement.AppendChild(newHMIUser);

            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            this.userList.Add(info);
            return true;
        }

        public bool ChangePassword(UserInfo info)
        {
            // find the user.
            XmlElement newHMIUser = document.CreateElement("HMIUser");
            bool found = false;
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        if (attr.Value == info.username)
                        {
                            found = true;
                        }
                    }
                    else if (attr.Name == "password")
                    {
                        if (found)
                        {
                            attr.Value = info.password;
                            break;
                        }
                    }
                }
                if (found)
                {
                    break;
                }
            }
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            //Update userList
            foreach (UserInfo info1 in userList)
            {
                if (info1.username == info.username)
                    info1.password = info.password;
            }

            return found;
        }
        public bool DeleteUser(string username)
        {
            // match user name and passowrd 
            bool found = false;
            XmlNode root = document.DocumentElement;
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        if (attr.Value == username)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    root.RemoveChild(node);
                    this.userList.Remove(getSingleUser(username));
                    break;
                }
            }

            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            return found;
        }
        private UserInfo getSingleUser(string username)
        {
            foreach (UserInfo info in userList)
            {
                if (info.username == username)
                    return info;
            }
            return null;
        }

        public string DoHash(string type, string username, string password)
        {
            string str = this.HashCode(type);
            str = this.HashCode(password + str);
            str = this.HashCode(username + str);
            str = this.HashCode("HMI" + str);
            return str;
        }
        private string HashCode(string str)
        {
            char[] data = str.ToCharArray();
            byte[] r = new byte[data.Length];
            // This is one implementation of the abstract class MD5.
            MD5 md5 = new MD5CryptoServiceProvider();
            for (int i = 0; i < data.Length; i++)
                r[i] = (byte)data[i];
            byte[] result = md5.ComputeHash(r);
            return Convert.ToBase64String(r);
        }
    }

}
