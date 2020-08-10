
/* Raptor design specifications 

General purpose configurable digital I/O with pullup/pulldown,
input or output, and enable/disable.

The GPIO pins are sixteen assignable digital inputs or outputs. 

All writes to reg_gpio_data are registered. 

All reads from reg_gpio_data are immediate.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Utilities;

namespace Antmicro.Renode.Peripherals.GPIOPort
{
    public class RaptorGPIO : BaseGPIOPort, IDoubleWordPeripheral, IKnownSize
    {
    	public RaptorGPIO(Machine machine) : base(machine, NumberOfPins)
        {
		locker = new object();
		pins = new Pin[NumberOfPins];
		
		var registersMap = new Dictionary<long, DoubleWordRegister>
		{
		     {(long)Registers.GPIO_DATA, new DoubleWordRegister(this)
		          .WithValueField(0, 16,
				readCallback: (_, _) =>
				{
				    var readOperations = pins.Select(x => (x.pinOperation & Operation.Read) != 0);  // select pins configured as input pins
				    var result = readOperations.Zip(State, (operation, state) => operation && state);
				    return BitHelper.GetValueFromBitsArray(result);   // register value after adjusting the read value for read pins 
				})
				
// 			   .WithValueField(16, 16,
// 				readCallback: (_, _) =>
// 				{
// // 				    var readOperations = pins.Select(x => (x.pinOperation & Operation.Read) != 0);
// // 				    var result = readOperations.Zip(State, (operation, state) => operation && state);
// // 				    return BitHelper.GetValueFromBitsArray(result);
// 				})
		      },
		      
		       {(long)Registers.GPIO_ENB, new DoubleWordRegister(this)
		          .WithValueField(0,16, writeCallback: (_, val) =>
                         {
			 	if(val == 1) // input
				{
				
				}
				else  // output
				{
				
				}
			 })
			  .WithTag("RESERVED", 16, 16)   
		      }
		      
		      // todo: pullup and pulldown
		      
// 		       {(long)Registers.GPIO_PUB, new DoubleWordRegister(this)
// 		          .WithValueField(0, 16, writeCallback: (_, val) =>
//                          {
// 			 	if(val == 0) // pullup
// 				{
				
// 				}
// 				else  // none
// 				{
				
// 				}
// 			 })
// 			  .WithTag("RESERVED", 16, 16)   
// 		      },
		      
// 		       {(long)Registers.GPIO_PDB, new DoubleWordRegister(this)
// 		          .WithValueField(0, 16, writeCallback: (_, val) =>
//                          {
// 			 	if(val == 0) // pulldown
// 				{
				
// 				}
// 				else  // none
// 				{
				
// 				}
// 			 })
// 			  .WithTag("RESERVED", 16, 16)   
// 		      } 
		
		};

		registers = new DoubleWordRegisterCollection(this, registersMap);
        }
	
	
	public uint ReadDoubleWord(long offset)
        {
            return RegistersCollection.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            RegistersCollection.Write(offset, value);
        }
	
	public override void OnGPIO(int number, bool value)   // Note that Connections and State are fields in the BaseGPIOPort class
        {
            lock(locker)
            {
                base.OnGPIO(number, value);
                Connections[number].Set(value);
            }
        }
	
	public override void Reset()
        {
            lock(locker)
            {
                base.Reset();
                registers.Reset();
            }
        }
	
	private readonly DoubleWordRegisterCollection registers;
        private readonly object locker;   // for blocking until the process is done 
        private readonly Pin[] pins;

	private const int NumberOfPins = 16;  // to be confirmed 
	
	private struct Pin
        {
            public Operation pinOperation;
        }

        [Flags]
        private enum Operation : long
        {
            Disabled = 0x0,
            Read = 0x1,
            Write = 0x2
        }

	private enum Registers : long
        {
            GPIO_DATA = 0x80000000,  // GPIO input/output (lower 16 bits) GPIO output readback (upper 16 bits)
            GPIO_ENB = 0x80000004, // GPIO output enable (0 = output, 1 = input)
            GPIO_PUB = 0x80000008, // GPIO pullup enable (0 = pullup, 1 = none)
            GPIO_PDB = 0x8000000c // GPIO pulldown enable (0 = pulldown, 1 = none)
        }


        
       
    }
}
