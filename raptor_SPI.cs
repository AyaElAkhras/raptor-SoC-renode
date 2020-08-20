/*
  SPI Master Controller 
*/ 

using System;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Utilities;
using Antmicro.Renode.Exceptions;

namespace Antmicro.Renode.Peripherals.SPI
{
  public class raptor_SPI: SimpleContainer<ISPIPeripheral>, IDoubleWordPeripheral, IKnownSize
  {  
    public raptor_SPI(Machine machine, uint transmitDepth, uint receiveDepth) : base(machine)
    {
      transmitBuffer = new Queue<ushort>();  // 16 bits 
      receiveBuffer = new Queue<ushort>();

      this.transmitDepth = transmitDepth;

      var registersMap = new Dictionary<long, DoubleWordRegister>
      {
        {(long)Registers.SPIDATA, new DoubleWordRegister(this)
                    .WithReservedBits(16, 16)
                    .WithValueField(0, 16, valueProviderCallback: _ =>    // whenever the field is read, dequeue from receieve buffer 
                    {
                        if(!start.Value)
                        {
                            this.Log(LogLevel.Warning, "Trying to read value from a disabled SPI");
                            return 0;
                        }
                        
                        if(!TryDequeueFromReceiveBuffer(out var data))
                        {
                            this.Log(LogLevel.Warning, "Trying to read from an empty FIFO");
                            return 0;
                        }

                        return data; 
                    },
                    writeCallback: (_, val) =>    // whenever the register is written to, enqueue to transmit buffer 
                    {
                        if(!start.Value)
                        {
                            this.Log(LogLevel.Warning, "Cannot write to SPI buffer while disabled");
                            return;
                        }
                    
                        EnqueueToTransmitBuffer((ushort)val);
                    }, name: "DATA")
                   
        },
         
        {(long)Registers.SPICTRL, new DoubleWordRegister(this)
                  .WithFlag(0, out start, changeCallback: (_, val) =>
                    {
                        if(val == false)
                        {
                            ClearBuffers();
                        }
                    }, name: "START")
                    
                    /*
                    There is a single bit for slave select, if it's high, the slave is disconnected (active low), if it's low, the slave transmits data
                    */
                    
                    .WithFlag(1, changeCallback: (_, val) => 
                    {
                        if(val == false)
                        {
                           TrySendData(1);   // 1 is the address of the single slave we have 
                        }
                        
                        else
                        {
                          TryGetByAddress(1, out var slave);  // 1 is the address of the single slave we have 
                          slave.FinishTransmission(); 
                        }
                    }, name: "SS0")  
        },
        
        {(long)Registers.SPICFG, new DoubleWordRegister(this)
                    
        },
        
        {(long)Registers.SPISTATUS, new DoubleWordRegister(this)
              .WithFlag(0, FieldMode.Read, valueProviderCallback: _ => true, name: "done")  // raises the done flag
              .WithReservedBits(1, 31)
        }
          
      };

      registersCollection = new DoubleWordRegisterCollection(this, registersMap);
            
    }
    
    public override void Register(ISPIPeripheral peripheral, NumberRegistrationPoint<int> registrationPoint)
    {
        if(registrationPoint.Address != 1)
        {
            throw new RegistrationException("SPI Master supports 1 slave at address 1");
        }

        base.Register(peripheral, registrationPoint);
    }

    public override void Reset()
    {
        ClearBuffers();

        registersCollection.Reset();
    }
    
    private void ClearBuffers()
    {
      lock(innerLock)
      {
        receiveBuffer.DequeueAll();
        transmitBuffer.DequeueAll();
      } 
    }

    public bool TryDequeueFromReceiveBuffer(out ushort data)
    {
        if(!receiveBuffer.TryDequeue(out data))
        {
            data = 0;
            return false;
        }

        return true;
    }
    
    private void EnqueueToTransmitBuffer(ushort val)
    {
        transmitBuffer.Enqueue(val);
    }

    public uint ReadDoubleWord(long offset)
    {
      return registersCollection.Read(offset);
    }

    public void WriteDoubleWord(long offset, uint value)
    {
      registersCollection.Write(offset, value);
    }
        
    private bool TrySendData(int slaveAddress)
    {
           /*
            Note that SimpleContainer template class contains a dictionary that maps integers to SPI peripheral 
            The following method returns back the SPI peripheral that is mapped to the passed slaveAddress
           */
           this.TryGetByAddress(slaveAddress, out var peripheral);  // to return the SPI slave peripheral connected to this address 
           DoTransfer(peripheral, transmitBuffer.Count, readFromFifo: true, writeToFifo: true);  // read or write 
            
           return true;
    }

    private void DoTransfer(ISPIPeripheral peripheral, int size, bool readFromFifo, bool writeToFifo)
    { 
      this.Log(LogLevel.Noisy, "Doing an SPI transfer of size {0} bytes (reading from fifo: {1}, writing to fifo: {2})", size, readFromFifo, writeToFifo);
      for(var i = 0; i < size; i++)
      {
        var dataToSlave = readFromFifo ? transmitBuffer.Dequeue() : (ushort)0;    // returns ushort variable dequeued 
        ushort dataFromSlave = peripheral.Transmit((byte)dataToSlave);   // Transmit is a method in ISPIPeripheral class

        this.Log(LogLevel.Noisy, "Sent 0x{0:X}, received 0x{1:X}", dataToSlave, dataFromSlave);

        if(!writeToFifo)
        {
          continue;
        }

        lock(innerLock)
        {
          receiveBuffer.Enqueue(dataFromSlave);
        }
      }
    }
            
    public long Size => 0x10000000;
    
    private IFlagRegisterField start; // start enabler of the SPI
   
    private readonly Queue<ushort> receiveBuffer;
    private readonly Queue<ushort> transmitBuffer;

    private readonly DoubleWordRegisterCollection registersCollection;
    private readonly object innerLock = new object();
    
    private enum Registers
    {
      SPIDATA = 0x0,
      SPICTRL = 0x4,  // 0:Start, 1:Slave Select (SS0 bit)
      SPICFG = 0X8,   // 0: SPI Clock Polarization (CPOL) 1: SPI Clock Phase (CPHA) 2-9: SPI clock prescale 
      SPISTATUS = 0x10  // 0:Done
    }
  
  }
}
