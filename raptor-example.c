#include <stdint.h>

#define reg_uart_data (*(volatile uint32_t*) (0x40200000)) 

void print_char(uint32_t x) {
   	reg_uart_data = x;
   	int j;
    for (j = 0; j < 70000; j++);
}

int main()
{
	int j;
	for (j = 0; j < 70000; j++);

    // will print "Hello Raptor"
	while (1) {
		print_char(0x0048);
		print_char(0x0065);
		print_char(0x006C);
		print_char(0x006C);
		print_char(0x006F);
		print_char(0x020);    //
		print_char(0x0052);
		print_char(0x0061);
		print_char(0x0070);
		print_char(0x0074);
		print_char(0x006F);
		print_char(0x0072);

		print_char(0x000a);  // end of line 
		for (j = 0; j < 7000000; j++);   // wait a bit then print again
	}

	return 0;
}