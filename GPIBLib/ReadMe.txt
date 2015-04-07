GPIBLib by Douglas Sappet (dsappet@gmail.com)

NOTICE: Documentation not complete. It's on my TODO list. This document provides a top level overview. 

Overview: 
The idea behind this library is to compile commonly repeated code in GPIB apps into one re-usable lib. This uses the NationalInstuments VISA.NS library but does not use National Instruments GPIB library. The NI GPIB library only works for National Instruments usb-gpib adapters. This LIB shall work for others such as Agilent as long as their appropriate drivers are installed.
A class called Device is provided to represent a device on the GPIB bus you wish to talk to. A separate instance shall be used for each instrument. 

The Device Class
- This class is added to represent a specific device on the GPIB bus.
- A separate instance of this class should be used for each instance. 
- This class can be initialized by the GPIB class functions described below but the address element is a public memeber and can be set manually if so desired. 
- Init / Query / WriteOnly / Read functions provided in the Device class. 

The GPIB Class
- Contains a function called FindInstruments that returns a list of the device class for each device on the bus. 
- If possible the FindInstruments function performs an *IDN? request of the instrument and parses the manuf and model information then populates that into the device class. 