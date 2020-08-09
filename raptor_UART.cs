/*  Raptor UART Design Specifications:

1) Each UART uses two FIFOs as buffers for both sending and receiving data.

2) Reading from the FIFO while being empty would block the execution 
till the UART receives data. Same behavior is observed when trying to 
write to the transmission FIFO while being full. 

3) The UART generates an interrupt, which is driven HIGH whenever 
there is data to be read on the receiving FIFO. The interrupt is cleared
when the data is read. 
UART0 uses IRQ0 (INT16) and UART1 uses IRQ1 (INT17).

4) Data is 8 bits and there is 1 stop bit 

*/

using System;
using Antmicro.Renode.Core;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using System.Collections.Generic;
using Antmicro.Renode.Core.Structure.Registers;

namespace Antmicro.Renode.Peripherals.UART
{
	public class Raptor_UART : UARTBase, IDoubleWordPeripheral, IKnownSize
	{
		public Raptor_UART(Machine machine, uint fifo_size = 10, uint frequency = 50000000) : base(machine)
		{
			this.frequency = frequency;
			this.fifo_size = fifo_size;
						
			var registersMap = new Dictionary<long, DoubleWordRegister>
		        {
				{(long)Registers.TransmitData, new DoubleWordRegister(this)
				    .WithFlag(31, valueProviderCallback: _ => Count == this.fifo_size, name: "FULL")
				    .WithValueField(0, 8, FieldMode.Write, writeCallback: (_, b) =>
				    {
					if(Count < this.fifo_size) // transmit only if the fifo isn't full
					{
					    this.TransmitCharacter((byte)b);    // field in the UARTBase class
					    UpdateInterrupts();
					}
					else
					{
					    this.Log(LogLevel.Warning, "Trying to transmit '{1}' (0x{0}), but the transmitter is disabled", b, (char) b);
					}
				    }, name: "DATA")
				    .WithTag("RESERVED", 8, 23)
				    
				},

				{(long)Registers.ReceiveData, new DoubleWordRegister(this)
				    // the "EMPTY" flag MUST be declared before "DATA" value field because 'Count' value
				    // might change as a result of dequeuing a character; otherwise if the queue was of
				    // length 1, the read of this register would return both the character and "EMPTY" flag
				    .WithFlag(31, valueProviderCallback: _ => Count == 0, name: "EMPTY")   // Count gets updated with the Queue size in UARTBase
				    .WithValueField(0, 8, FieldMode.Read, valueProviderCallback: _ =>
				    {
					if(!TryGetCharacter(out var character))  // function in the UARTBase that returns False if the Queue is empty
					{
					    this.Log(LogLevel.Warning, "Trying to read data from empty receive fifo");
					}
					return character;
				    }, name: "DATA")
				     .WithFlag(0, out transmitWatermarkInterruptEnable, changeCallback: (_, __) => UpdateInterrupts(), name: "TXWM")
                    		     .WithFlag(1, out receiveWatermarkInterruptEnable, changeCallback: (_, __) => UpdateInterrupts(), name: "RXWM")
				     .WithTag("RESERVED", 8, 23)
				 },
			};			
			
			registers = new DoubleWordRegisterCollection(this, registersMap);
			IRQ = new GPIO();   
			Reset();
			
			
		}   // end of constructor
		
		public uint ReadDoubleWord(long offset)   
		{
			lock(innerLock)
			{	
				return registers.Read(offset);
			}
		}

		public void WriteDoubleWord(long offset, uint value)
		{
			lock(innerLock)
			{
				registers.Write(offset, value);
			}
		}
		
		public override void Reset()   
		{
		    base.Reset();
		    
		    registers.Reset();
		    UpdateInterrupts();
		   
		}
		
		
		private void UpdateInterrupts()
		{
			lock(innerLock)
			{
				transmitWatermarkInterruptPending.Value = (transmitWatermarkInterruptEnable.Value);
				receiveWatermarkInterruptPending.Value = (receiveWatermarkInterruptEnable.Value);

				IRQ.Set(transmitWatermarkInterruptPending.Value || receiveWatermarkInterruptPending.Value);
			}
		}

		private readonly uint frequency;  // supposing this is the sysclk
		private readonly uint fifo_size; // max size of the transmission fifo
		
// 		private readonly IFlagRegisterField transmitEnable;
// 		private readonly IFlagRegisterField receiveEnable;
		
// 		private readonly IValueRegisterField transmitWatermarkLevel;
// 		private readonly IValueRegisterField receiveWatermarkLevel;
// 		private readonly IFlagRegisterField transmitWatermarkInterruptPending;
// 		private readonly IFlagRegisterField receiveWatermarkInterruptPending;
		private readonly IFlagRegisterField transmitWatermarkInterruptEnable;
		private readonly IFlagRegisterField receiveWatermarkInterruptEnable;



		public override Parity ParityBit
		{
			get
			{	// No parity
				return Parity.None;
			}
		}

		public override Bits StopBits
		{
			get 
			{
				// 1 stop bit 
				return Bits.One;
			}
		}	

		public override uint BaudRate
		{
			get
			{
				// baud rate = sysclk/163*1
				return this.frequency / 163*16;
			}
		}
		
		private enum Registers : long   // Need to check the addresses 
		{
			TransmitData = 0x0,
			ReceiveData = 0x04
			// CLK_DIV = 0x08   // todo
		}
		

	}

}
