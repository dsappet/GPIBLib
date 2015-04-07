using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.VisaNS;

namespace GPIBLib
{
    class GPIBException : Exception
    {
        // ..  The following three constructors are recommended as a minimum by msdn
        public GPIBException() { }
        public GPIBException(string message) : base(message) { }
        public GPIBException(string message, Exception inner) : base(message, inner) { }
    }
    public class Device
    {
        // variables
        private GpibSession mbSession;  // contained in NationalInstruments VisaNS
        public string manufacturer;
        public string model;
        public string address;
        // variables with accessors
        public int buffSize
        {
            get
            {
                int val;
                if (mbSession != null)
                {
                    return mbSession.DefaultBufferSize;
                }
                else
                {
                    Init();
                    val = mbSession.DefaultBufferSize;
                    Close();
                    return val;
                }
            }
            set
            {
                {
                    try
                    {
                        mbSession.DefaultBufferSize = value;
                    }
                    catch (Exception ex) { }
                }
            }
        }

        // Constructors
        public Device() { }
        public Device(string _mfr, string _model, string _addr)
        {
            manufacturer = _mfr;
            model = _model;
            address = _addr;
        }
        
        public void Init()
        {
            try
            {
                if (mbSession != null) mbSession.Dispose();
                mbSession = new GpibSession(this.address);            
            }
            catch (Exception ex)
            {
                // Exception occured 
                throw new GPIBException("inner exception occured in GPIBLib at Init()." , ex);
            }
        }
        public void Close()
        {
            try
            {
                if (mbSession != null) mbSession.Dispose();
            }
            catch (Exception ex) {
                throw new GPIBException("inner exception occured in GPIBLib at Close()." , ex);
            }
        }
        public string Query(string command)
        {
            string reply = string.Empty;
            try
            {
                reply = mbSession.Query(command + "\n");
            }
            catch (Exception ex) {
                throw new GPIBException("inner exception occured in GPIBLib at Query()." , ex);
            }
            return reply;
        }
        public void WriteOnly(string command)
        {
            try
            {
                mbSession.Write(command + "\n");
            }
            catch (Exception ex) { //
                throw new GPIBException("inner exception occured in GPIBLib at WriteOnly()." , ex);
            }
        }
        public string ReadOnly()
        {
            string reply = string.Empty;
            try
            {
                reply = mbSession.ReadString();
            }
            catch (Exception ex)
            {
                //
                throw new GPIBException("inner exception occured in GPIBLib at ReadOnly().", ex);
            }
            return reply;
        }
        public void changeTimeout(int timeMs)
        {
            mbSession.Timeout = timeMs;
        }
        public StatusByteFlags SerialPoll()
        {
            return mbSession.ReadStatusByte();          
        }
        public bool messageAvailable()
        {
            StatusByteFlags flags;
            try
            {
                flags = mbSession.ReadStatusByte();
            }
            catch (Exception ex)
            {
                throw new GPIBException("inner exception occured in GPIBLib @ messageAvailable(). INNER: ", ex);
            }
            if (flags.HasFlag(StatusByteFlags.MessageAvailable)) return true;
            else return false;
        }

    }
    public class GPIB
    {
        public List<Device> FindInstruments()
        {
            List<Device> resources = new List<Device>();
            ResourceManager manager = ResourceManager.GetLocalManager();
            string[] resourceList = manager.FindResources("GPIB?*");
            foreach (string a in resourceList)
            {
                HardwareInterfaceType intFaceType;
                short intFaceNum;
                string resourceClass = string.Empty;
                string reply = string.Empty;
                manager.ParseResource(a, out intFaceType, out intFaceNum, out resourceClass);
                if (resourceClass == "INTFC")
                {
                    try
                    {
                        GpibInterface gFace = new GpibInterface(a);
                        gFace.SendInterfaceClear();
                    }
                    catch (ArgumentException ex)
                    {
                        // The INTFC cannot be init. 
                        // Throw an Exception here
                        //throw new GPIBException("inner exception occured in GPIBLib at FindInstruments() the INTFC cannot be init. " + ex);
                    }
                    catch (Exception ex)
                    {
                        // Other exception occured 
                        //throw new GPIBException("inner exception occured in GPIBLib at FindInstruments()." + ex);
                    }
                }
                else if (resourceClass == "INSTR")
                {
                    try
                    {
                        GpibSession sesh = new GpibSession(a);
                        reply = sesh.Query("*IDN?\n");
                        string[] info = reply.Split(',');
                        resources.Add(new Device(info[0], info[1], a));
                        sesh.Dispose();
                    }
                    catch (Exception ex)
                    {
                        throw new GPIBException("Inner exception occured in GPIBLib at FindInstruments(): " + ex);
                    }
                }
            }
            return resources;
        }
    }
}
