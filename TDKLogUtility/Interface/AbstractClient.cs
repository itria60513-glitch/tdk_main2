using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Linq;
using TDKLogUtility.Module;

public abstract class AbstractClient : MarshalByRefObject
{
    public static AbstractClient GetUniqueInstance(string ip, int port)
    {
        return null;
    }

    protected AbstractClient(string ip, int port)
    {
    }

    public abstract ILogUtility CreateInstance();
}
public class MarshalByRefObjectEx : MarshalByRefObject
{
    public override object InitializeLifetimeService()
    {
        return null;
    }
}
public class DnsSvr
{
    private static string m_szXML = "d:\\hmi2010\\Config\\System\\RemotingObjectInfo.xml";

    private static IPAddress RemotingServerPreferredNIC = null;

    private static byte[] GetRemotingServerPreferredNIC()
    {
        if (RemotingServerPreferredNIC == null)
        {
            if (!File.Exists(m_szXML))
            {
                return null;
            }

            XDocument xDocument;
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(m_szXML);
                xDocument = XDocument.Load(xmlTextReader);
                xmlTextReader.Close();
            }
            catch
            {
                RemotingServerPreferredNIC = IPAddress.Parse("192.168.0.0");
                return RemotingServerPreferredNIC.GetAddressBytes();
            }

            try
            {
                IEnumerable<XElement> enumerable = from para in xDocument.Elements("RemotingObjects").Nodes()
                                                   where para.NodeType == XmlNodeType.Element && ((XElement)para).FirstAttribute.Value == "FileUtilities"
                                                   select (XElement)para;
                IEnumerator<XElement> enumerator = enumerable.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    XElement current = enumerator.Current;
                    RemotingServerPreferredNIC = IPAddress.Parse(current.FirstAttribute.NextAttribute.Value);
                }
            }
            catch
            {
                RemotingServerPreferredNIC = IPAddress.Parse("192.168.0.0");
            }
        }

        return RemotingServerPreferredNIC.GetAddressBytes();
    }

    public static string ResolveIP(string machinename)
    {
        try
        {
            IPAddress[] array = new IPAddress[1];
            if (IPAddress.TryParse(machinename, out var address))
            {
                array[0] = address;
            }
            else
            {
                array = Dns.GetHostAddresses(machinename);
            }

            if (array == null || array.Length == 0)
            {
                return null;
            }

            if (array.Length == 1)
            {
                return array[0].ToString();
            }

            byte[] remotingServerPreferredNIC = GetRemotingServerPreferredNIC();
            IPAddress[] array2 = array;
            foreach (IPAddress iPAddress in array2)
            {
                if (iPAddress.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    byte[] addressBytes = iPAddress.GetAddressBytes();
                    if (addressBytes[0] == remotingServerPreferredNIC[0] && addressBytes[1] == remotingServerPreferredNIC[1])
                    {
                        return iPAddress.ToString();
                    }
                }
            }

            IPAddress[] array3 = array;
            foreach (IPAddress iPAddress2 in array3)
            {
                if (iPAddress2.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    byte[] addressBytes2 = iPAddress2.GetAddressBytes();
                    if (addressBytes2[0] != 172)
                    {
                        return iPAddress2.ToString();
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return null;
        }
    }

    public static string GetBindIP(string targetIP)
    {
        try
        {
            byte[] addressBytes = IPAddress.Parse(targetIP).GetAddressBytes();
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in allNetworkInterfaces)
            {
                foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    byte[] addressBytes2 = unicastAddress.Address.GetAddressBytes();
                    if (addressBytes2[0] == addressBytes[0] && addressBytes2[1] == addressBytes[1] && addressBytes2[2] == addressBytes[2])
                    {
                        return unicastAddress.Address.ToString();
                    }
                }
            }
        }
        catch
        {
        }

        return "";
    }
}