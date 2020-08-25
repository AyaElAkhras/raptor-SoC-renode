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
				{(long)Registers.Data, new DoubleWordRegister(this)
				    .WithValueField(0, 8, FieldMode.Write, writeCallback: (_, b) =>
				    {
				    	// transmission happens instantaneously 
					this.TransmitCharacter((byte)b);    // method in the UARTBase class
				    }, name: "TXDATA")
// 				    .WithFlag(30, valueProviderCallback: _ => false, name: "FULL")
				    .WithFlag(31, valueProviderCallback: _ => Count == 0, name: "EMPTY")   // Count gets updated with the Queue size in UARTBase
				    
				   // generates an interrupt, which is driven HIGH whenever there is data to be read on the receiving FIFO
				    .WithValueField(8, 8, FieldMode.Read, readCallback: (_, __) => UpdateInterrupts(), valueProviderCallback: _ =>
				    {
				    	// Reading from the FIFO while empty would block the execution till the UART receives data
					if(!TryGetCharacter(out var character))  // function in the UARTBase that returns False if the Queue is empty
					{
					    this.Log(LogLevel.Warning, "Trying to read data from empty receive fifo");
					}
					return character;
				    }, name: "RXDATA")
				     .WithTag("RESERVED", 16, 15) // no implementation for the remaining fields 
				}
				
			};			
			
			registers = new DoubleWordRegisterCollection(this, registersMap);
			IRQ = new GPIO();   
			Reset();
			
			
		}   // end of constructor
		
		protected override void QueueEmptied()
		{
			UpdateInterrupts();
		}
		
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
		
		protected override void CharWritten()
		{
			UpdateInterrupts();

		}
		
		
		private void UpdateInterrupts()
		{
			lock(innerLock)
			{
				IRQ.Set(Count>0);  // interrupt is high whenever there is data to be read on the receiving FIFO, it is cleared when all data is read so when Count = 0
			}
		}

        	public long Size => 0x100000;
	
		private readonly uint frequency;  // supposing this is the sysclk
		private readonly uint fifo_size; // max size of the transmission fifo
 		private readonly DoubleWordRegisterCollection registers;
		public GPIO IRQ { get; private set; }

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
				return (this.frequency / 163*16);
			}
		}
		
		private enum Registers : long  
		{
			Data = 0x00   // a single register for the rx and tx data bits
			// CLK_DIV = 0x08   // todo
		}
		

	}

}
