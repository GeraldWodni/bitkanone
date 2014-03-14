#include <msp430.h>

; --- basic registers ---
#define PC r0
#define SP r1
#define SR r2
#define CG1 r2
#define CG2 r3

; --- forth registers ---

; the original stack pointer remains unused ?

; Top of Stack element is always kept in a register
#define	TOS  r4
#define	TORS r5
#define TEMP r6

; Instruction Pointer (IP) always points to next instruction
#define	IP r7

; Instruction Address (IA) address of instruction to be executed
#define	IA r8

; Return Stack pointer (RSP) (grows up)
; RSP always points one above SORS (Second of Return Stack)
#define RSP r9

; Data Stack Pointer (DSP) (grows down)
; DSP always points to SOS (Second of Stack)
#define DSP r10
#define UDR r11 ; uart data register

#define LED_CUR r12
#define LED_CNT r13

#define NEXT MOV	@IP+, PC	


.text
reset:
uart:
	; http://www.forth-ev.de/repos/4e4th/4e-init430G2553.s43
	MOV   #WDTPW+WDTHOLD,&WDTCTL  ; Stop watchdog timer
        mov.w   #__stack,SP                     ; gcc ldscripts compute __stack based on mmcu
    	
    	MOV.B   &CALBC1_16MHZ, &BCSCTL1   ; Set DCO
    	MOV.B   &CALDCO_16MHZ, &DCOCTL    ; to 8 MHz.

	BIS.B	#0x01, &UCB0CTL1	; UCSI reset

	MOV.B	#0x69, &UCB0CTL0	; enable 3-wire SPI master
	MOV.B	#0xC1, &UCB0CTL1	; Use SMCLK, keep UCSI reset

					; Clock prescaling = UCxxBR0 + UCxxBR1 * 256
 	MOV.B	#0x05, &UCB0BR0
 	MOV.B	#0x00, &UCB0BR1

	BIC.B	#0x01, &UCB0CTL1	; UCSI resume ( start operation )
  
	MOV.B	#0x41, &P1DIR		; set red led
	MOV.B	#0x09, &P1OUT		; pullup sw2
	MOV.B   #0x08, &P1REN

    	MOV.B   #0xA0, &P1SEL		; Use P1.1/P1.2 for USCI_A0
    	MOV.B   #0xA0, &P1SEL2		; Use P1.1/P1.2 for USCI_A0

	; MOV.B	#0xEE, &UCB0TXBUF	; Send data

	MOV.B	#0xFF, LED_CUR
	MOV.B	#4, LED_CNT

	BIS.B	#UCB0TXIE, &IE2		; Enable UCB0-TX ISR
	EINT
	BIS.B	#UCB0TXIFG, &IFG2

lala:
	JMP     lala

btn_preWait:
	AND	#0x08, &P1IN
	JZ	btn_preWait
btn_wait:				; wait for button press
	AND	#0x08, &P1IN
	JNZ	btn_wait
	BIC.B	#0x01, &P1OUT



	MOV.B	#0x45, &P1DIR		; enable leds and TX
	MOV.B	#0x08, &P1REN		; enable sw2 pullup
	MOV.B	#0x09, &P1OUT		; enable sw2 pullup + red led

	JMP	init

; SPI TX ISR, fill next to bits into TXBUF
spiTxIsr:
	MOV	#0x88, TEMP
	BIT.B	#0x80, LED_CUR		; set MSB into upper nibble
	JZ	Bit7IsZero
	MOV	#0xE8, TEMP
Bit7IsZero:
	BIT.B	#0x40, LED_CUR		; set MSB-1 into lower nibble
	JZ	Bit6IsZero
	XOR.B	#0x06, TEMP
Bit6IsZero:
	RLA	LED_CUR			; shift Bit7&6 away
	RLA	LED_CUR

	MOV.B	TEMP, &UCB0TXBUF	; Send data
	BIC.B	#UCB0TXIFG, &IFG2
	XOR.B	#0x40, &P1OUT

	DEC	LED_CNT
	JNZ	spiTxIsrEnd
	BIC.B	#UCB0TXIE, &IE2		; Disable UCB0-TX ISR
spiTxIsrEnd:
	RETI

; Configure UART (Koch) 
	BIS.B   #UCSSEL_2,&UCA0CTL1     ;db2 SMCLK
	MOV.B   #65,&UCA0BR0            ;db3 8MHz 9600 Insgesamt 833
	MOV.B   #3,&UCA0BR1             ;db4 8MHz 9600
	MOV.B   #UCBRS_2,&UCA0MCTL      ;db5 Modulation UCBRSx = 2
	BIC.B   #UCSWRST,&UCA0CTL1      ;db6 **Initialize USCI state machine**

recv:	BIT.B	#UCA0RXIFG,&IFG2	; wait for byte
	JZ	recv

	MOV.B	&UCA0RXBUF, TOS		; RX -> TOS
	BIC.B	#UCA0RXIFG,&IFG2	; clear IF


	CMP.B	#0x41, TOS
	JL	noupper
	SUB.B	#0x20, TOS
noupper:


send:	BIT.B	#UCA0TXIFG,&IFG2	; wait for transmission to finish
	JZ	send

	MOV.B	TOS, &UCA0TXBUF		; TOS -> TX

	JMP	recv

end:
	MOV.B	#0x48, &P1OUT
	JMP	end



; --- jumps ---

; ?jmp ( f -- Code: addr:H addr:L )
k__question_jmp:
	MOV	TOS, TEMP		; Save TOS
	MOV	@DSP+, TOS		; SOS -> TOS
	CMP	#0, TEMP
	JNZ	performQJump	
	ADD	#2, IP			; skip jump target
performQJump:
	NEXT

; k_jmp not needed, as forth and assembler-opcode are both 16bit

k_next:
	NEXT

k_halt:
	JMP	k_halt

; -- stack manipulation --
; drop ( x -- )
k_drop:
	MOV	@DSP+, TOS
	NEXT

; dup ( x -- x x )
k_dup:
	SUB	#2, DSP
	MOV	TOS, @DSP
	NEXT

; nip ( x1 x2 -- x2 )
k_nip:
	ADD	#2, DSP
	NEXT

; over ( x1 x2 -- x1 x2 x1 )
k_over:
	SUB	#2, DSP		; store TOS
	MOV	TOS, @DSP
	MOV	2(DSP), TOS	; load (former) SOS
	NEXT

; swap ( x1 x2 -- x2 x1 )
k_swap:
	MOV	@DSP, TEMP	; SOS -> TEMP
	MOV	TOS, @DSP	; TOS -> SOS
	MOV	TEMP, TOS	; TEMP-> TOS
	NEXT



; -- logical --

; and ( x1 x2 -- x3 )
k_and:
	AND	@DSP+, TOS
	NEXT

; or ( x1 x2 -- x3 )
k_or:
	BIS	@DSP+, TOS
	NEXT

; xor ( x1 x2 -- x3 )
k_xor:
	XOR	@DSP+, TOS
	NEXT

; 2* ( x1 -- x2 )
k_2_star_:
	ADD	TOS, TOS
	NEXT

; 2/ ( x1 -- x2 )
k_2_slash_:
	RRA	TOS
	NEXT

; invert ( x1 -- x2 )
k_invert:
	XOR	#-1, TOS
	NEXT



; -- arithmetical --

; negate ( x1 -- x2 )
k_negate:
	XOR	#-1, TOS
	ADD	#1, TOS
	NEXT

; 1+ ( n1 -- n2 )
k_1_plus_:
	ADD	#1, TOS
	NEXT

; 1- ( n1 -- n2 )
k_1_minus_:
	SUB	#1, TOS
	NEXT

; + ( n1 n2 -- n3 )
k__plus_:
	ADD	@DSP+, TOS
	NEXT

; - ( n1 n2 -- n3 )
k__minus_:
	MOV	@DSP+, TEMP
	SUB	TOS, TEMP
	MOV	TEMP, TOS
	NEXT



; -- conditional arithmetic --

; 0= ( n -- f )
k_0_equal_:
	XOR	TEMP, TEMP
	CMP	#0, TOS
	JNE	kb_0_equal_zero
	MOV	#-1, TEMP
kb_0_equal_zero:
	MOV	TEMP, TOS
	NEXT

; = ( n1 n2 -- f )
k__equal_:
	XOR	TEMP, TEMP
	CMP	@DSP+, TOS
	JNE	kb__equal_zero
	MOV	#-1, TEMP
kb__equal_zero:
	MOV	TEMP, TOS
	NEXT

; < ( n1 n2 -- f )
k__lower_:
	XOR	TEMP, TEMP
	CMP	@DSP+, TOS
	JGE	kb__lower_
	MOV	#-1, TEMP
kb__lower_:
	MOV	TEMP, TOS
	NEXT



; -- memory --

; @ ( addr -- n )
k__fetch_:
	MOV	@TOS, TOS
	NEXT

; ! ( n addr -- )
k__store_:
	MOV	@DSP+, @TOS	; store 
	MOV	@DSP+, TOS	; load new TOS
	NEXT

; c@ ( addr -- c )
k_c_fetch_:
	MOV.B	@TOS, TOS
	NEXT

; c! ( c addr -- )
k_c_store_:
	MOV.B	@DSP, @TOS	; store 
	ADD	#2, DSP
	MOV	@DSP+, TOS	; load new TOS
	NEXT


; -- static memory --

; lit ( -- n )
k_lit:
	SUB	#2, DSP
	MOV	TOS, @DSP
	MOV	@IP+, TOS
	NEXT

; clit ( -- n )
k_clit:
	SUB	#2, DSP
	MOV	TOS, @DSP
	MOV.B	@IP+, TOS
	NEXT

	

; --- secondaries ---

; call ( -- R: -- IP )
k_call:
	MOV	@IP+, TEMP	; load target address
	PUSH	IP		; store IP
	MOV	TEMP, IP	
	NEXT

; execute ( addr -- R: -- IP )
k_execute:
	PUSH	IP		; store IP
	MOV	TOS, IP		; TOS -> IP
	MOV	@DSP+, TOS	; SOS -> TOS
	NEXT
	
; primitive ( addr -- )
k_primitive:
	MOV TOS, TEMP		; Store target
	MOV @DSP+, TOS		; SOS -> TOS
	MOV TEMP, PC		; Jump to target

; ret ( -- R: IP -- )
k_ret:
	MOV	@SP+, IP	; recall IP
	NEXT

k_delay:
	SUB	#1, TOS
	JNZ	k_delay
	JMP	k_drop

init:
	MOV	#0x210, DSP
	MOV	#cold, IP
	NEXT
cold:
.WORD	k_lit
.WORD	0x0000
.WORD	k_lit
.WORD	P1OUT
.WORD	k_c_store_

.WORD	k_lit
.WORD	0x0011
.WORD	k_delay

.WORD	k_lit
.WORD	0x0020
.WORD	k_dup
.WORD	k__plus_
.WORD	k_lit
.WORD	0x0001
.WORD	k_or
.WORD	k_lit
.WORD	P1OUT
.WORD	k_c_store_
.WORD	init


.section ".vectors", "ax", @progbits
	.word reset    ;0xffe0 slot  0  0
        .word reset    ;0xffe2 slot  1  2
        .word reset    ;0xffe4 slot  2  4 (PORT1_VECTOR)
        .word reset    ;0xffe6 slot  3  6 (PORT2_VECTOR)
        .word reset    ;0xffe8 slot  4  8 
        .word reset    ;0xffea slot  5  A (ADC10_VECTOR)
        .word spiTxIsr ;0xffec slot  6  C (USCIAB0TX_VECTOR) 
        .word reset    ;0xffee slot  7  E (USCIAB0RX_VECTOR)
        .word reset    ;0xfff0 slot  8 10 (TIMER0_A1_VECTOR)
        .word reset    ;0xfff2 slot  9 12 (TIMER0_A0_VECTOR)
        .word reset    ;0xfff4 slot 10 14 (WDT_VECTOR)
        .word reset    ;0xfff6 slot 11 16 (COMPARATORA_VECTOR)
        .word reset    ;0xfff8 slot 12 18 (TIMER1_A1_VECTOR)
        .word reset    ;0xfffa slot 13 1a (TIMER1_A0_VECTOR)
        .word reset    ;0xfffc slot 14 1c (NMI_VECTOR)
        .word reset    ;0xfffe slot 15
	.end
