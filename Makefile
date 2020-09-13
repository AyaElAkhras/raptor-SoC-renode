PREFIX	?= arm-none-eabi-
CC	= $(PREFIX)gcc
AS = $(PREFIX)as
LD = $(PREFIX)ld

#FLAGS = -marm -mcpu=cortex-m0 -mthumb -mfloat-abi=soft -nostartfiles -static -Wl,--gc-sections -Wl,--entry=Reset_Handler -Wl,--start-group -lm -lc -lgcc -Wl,--end-group -fno-exceptions -nostdlib --specs=nano.specs -t -lstdc++ -lc -lnosys -lm -Wl
FLAGS = -Wall  -mthumb -nostdlib -nostartfiles -ffreestanding -mcpu=cortex-m0 -Wno-unused-value

LDSCRIPT = sections.ld

SRC = raptor-example.c
OBJ = raptor-example.o

ELF = raptor-example.elf
STARTUP = cm0_startup.o

all:$(ELF)

$(ELF): $(STARTUP) $(OBJ)
	$(LD) -T $(LDSCRIPT) $(STARTUP) $(OBJ) -o $@

$(STARTUP): cm0_startup.s
	$(AS) $< -o $@

$(OBJ): $(SRC) 
	$(CC) $(FLAGS) $(SRC) -o $@ 
clean:
	rm -f *.o *.elf

