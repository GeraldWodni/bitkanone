@
@    Mecrisp-Stellaris - A native code Forth implementation for ARM-Cortex M microcontrollers
@    Copyright (C) 2013  Matthias Koch
@
@    This program is free software: you can redistribute it and/or modify
@    it under the terms of the GNU General Public License as published by
@    the Free Software Foundation, either version 3 of the License, or
@    (at your option) any later version.
@
@    This program is distributed in the hope that it will be useful,
@    but WITHOUT ANY WARRANTY; without even the implied warranty of
@    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@    GNU General Public License for more details.
@
@    You should have received a copy of the GNU General Public License
@    along with this program.  If not, see <http://www.gnu.org/licenses/>.
@

@ Terminalroutinen
@ Terminal code and initialisations.
@ Porting: Rewrite this !

        @ Bit-position equates (for setting or clearing a single bit)

        .equ  BIT0,    0x00000001
        .equ  BIT1,    0x00000002
        .equ  BIT2,    0x00000004
        .equ  BIT3,    0x00000008
        .equ  BIT4,    0x00000010
        .equ  BIT5,    0x00000020
        .equ  BIT6,    0x00000040
        .equ  BIT7,    0x00000080
        .equ  BIT8,    0x00000100
        .equ  BIT9,    0x00000200
        .equ  BIT10,   0x00000400
        .equ  BIT11,   0x00000800
        .equ  BIT12,   0x00001000
        .equ  BIT13,   0x00002000
        .equ  BIT14,   0x00004000
        .equ  BIT15,   0x00008000
        .equ  BIT16,   0x00010000
        .equ  BIT17,   0x00020000
        .equ  BIT18,   0x00040000
        .equ  BIT19,   0x00080000
        .equ  BIT20,   0x00100000
        .equ  BIT21,   0x00200000
        .equ  BIT22,   0x00400000
        .equ  BIT23,   0x00800000
        .equ  BIT24,   0x01000000
        .equ  BIT25,   0x02000000
        .equ  BIT26,   0x04000000
        .equ  BIT27,   0x08000000
        .equ  BIT28,   0x10000000
        .equ  BIT29,   0x20000000
        .equ  BIT30,   0x40000000
        .equ  BIT31,   0x80000000


@ Registerdefinitionen

        .equ GPIOA_BASE      ,   0x40020000
        .equ GPIOA_MODER     ,   GPIOA_BASE + 0x00
        .equ GPIOA_OTYPER    ,   GPIOA_BASE + 0x04
        .equ GPIOA_OSPEEDR   ,   GPIOA_BASE + 0x08
        .equ GPIOA_PUPDR     ,   GPIOA_BASE + 0x0C
        .equ GPIOA_IDR       ,   GPIOA_BASE + 0x10
        .equ GPIOA_ODR       ,   GPIOA_BASE + 0x14
        .equ GPIOA_BSRR      ,   GPIOA_BASE + 0x18
        .equ GPIOA_LCKR      ,   GPIOA_BASE + 0x1C
        .equ GPIOA_AFRL      ,   GPIOA_BASE + 0x20
        .equ GPIOA_AFRH      ,   GPIOA_BASE + 0x24

        .equ RCC_BASE        ,   0x40023800
        .equ RCC_CR          ,   RCC_BASE + 0x00
        .equ RCC_CFGR        ,   RCC_BASE + 0x08
        .equ RCC_AHB1ENR     ,   RCC_BASE + 0x30
        .equ RCC_APB1ENR     ,   RCC_BASE + 0x40

        .equ USART2_BASE     ,   0x40004400
        .equ USART2_SR       ,   USART2_BASE + 0x00
        .equ USART2_DR       ,   USART2_BASE + 0x04
        .equ USART2_BRR      ,   USART2_BASE + 0x08
        .equ USART2_CR1      ,   USART2_BASE + 0x0c
        .equ USART2_CR2      ,   USART2_BASE + 0x10
        .equ USART2_CR3      ,   USART2_BASE + 0x14
        .equ USART2_GTPR     ,   USART2_BASE + 0x18

        .equ RXNE            ,   BIT5
        .equ TC              ,   BIT6
        .equ TXE             ,   BIT7
        .equ HSERDY          ,   BIT17
        .equ HSEON           ,   BIT16

        .equ GPIOD_BASE      ,   0x40020C00
        .equ GPIOD_MODER     ,   GPIOD_BASE + 0x00
        .equ GPIOD_OTYPER    ,   GPIOD_BASE + 0x04
        .equ GPIOD_OSPEEDR   ,   GPIOD_BASE + 0x08
        .equ GPIOD_PUPDR     ,   GPIOD_BASE + 0x0C
        .equ GPIOD_IDR       ,   GPIOD_BASE + 0x10
        .equ GPIOD_ODR       ,   GPIOD_BASE + 0x14
        .equ GPIOD_BSRR      ,   GPIOD_BASE + 0x18
        .equ GPIOD_LCKR      ,   GPIOD_BASE + 0x1C
        .equ GPIOD_AFRL      ,   GPIOD_BASE + 0x20
        .equ GPIOD_AFRH      ,   GPIOD_BASE + 0x24

@ -----------------------------------------------------------------------------
Setup_Clocks:
@ -----------------------------------------------------------------------------
        @ Initialize STM32 Clocks

        @ Ideally, we would just take the defaults to begin with and
        @ do nothing.  Because it is possible that HSI is not
        @ accurate enough for the serial communication (USART2), we
        @ will switch from the internal 8 MHz clock (HSI) to the
        @ external 8 MHz clock (HSE).

        ldr r6, = RCC_CR
        mov r0, HSEON
        str r0, [r6]            @ turn on the external clock

awaitHSE:
        ldr r0, [r6]
        and r0, # HSERDY
        beq awaitHSE            @ hang here until external clock is stable

        @ at this point, the HSE is running and stable but I suppose we have not yet
        @ switched Sysclk to use it.

        ldr r6, = RCC_CFGR
        mov r0, # 1
        str r0, [r6]            @ switch to the external clock
        
        @ Turn off the HSION bit
        ldr r6, = RCC_CR
        ldr r0, [r6]
        and r0, 0xFFFFFFFE      @ Zero the 0th bit
        str r0, [r6]

        bx lr

@ -----------------------------------------------------------------------------
Setup_UART:
@ -----------------------------------------------------------------------------

        @ Enable the CCM RAM and all GPIO peripheral clock
        ldr r6, = RCC_AHB1ENR
        ldr r0, = BIT20+0x1FF
        str r0, [r6]

        @ Set PORTA pins in alternate function mode
        ldr r6, = GPIOA_MODER
        ldr r0, [r6]
        ldr r5, = 0xA0
        orr r0, r5
        str r0, [r6]

        @ Set alternate function 7 to enable USART2 pins on Port A
        ldr r6, = GPIOA_AFRL
        ldr r0, = 0x7700              @ Alternate function 7 for TX and RX pins of USART2 on PORTA 
        str r0, [r6]

        @ Enable the USART2 peripheral clock by setting bit 17
        ldr r6, = RCC_APB1ENR
        ldr r0, = BIT17
        str r0, [r6]

  @ Baudrate bestimmen: Bit 11-4 Teiler, Bit 3-0 Bruchterm

  @ Baud rate generation:
  @ 16000000 / (16 * 115200 ) = 1000000 / 115200 = 8.6805
  @ 0.6805... * 16 = 10,8 etwa 11
  @ $8B

  @ 16000000 / (16 * 9600 ) = 104.1666
  @ 104.1875 ist das nächste passende Wert.
  @ 104.1875 * 16 = $683

  @ Läuft aber hier mit 8 MHz.

        ldr r6, = USART2_BRR
        @ ldr r0, = 0x00000341  @  9600 bps
        @ ldr r0, = 0x000000D0  @ 38400 bps
        @ ldr r0, = 0x00000045  @ 115200 bps
        ldr r0, = 0x00000046  @ 115200 bps, ein ganz kleines bisschen langsamer...
        str r0, [r6]

        @ set TE (transmit enable) (bit 3), and RE (receiver enable) (bit 2) 
        @ this should cause an idle frame to be sent
        ldr r6, =USART2_CR1
        ldr r0, =BIT13+BIT3+BIT2
        str r0, [r6]

        bx lr

@ -----------------------------------------------------------------------------
uart_init: @ ( -- )
@ -----------------------------------------------------------------------------
  push {lr}

  bl Setup_Clocks
  bl Setup_UART

  @ Leuchtdioden-Anschlüsse vorbereiten
@  ldr r6, = GPIOD_MODER
@  ldr r0, = 0x55000000 @ Alle vier Leuchtdioden als Ausgänge !
@  str r0, [r6]

  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "emit"
emit: @ ( c -- ) Sendet Wert in r0
@ -----------------------------------------------------------------------------
   push {r0, r1, r2}
   popda r0

   ldr r2, =USART2_SR
        
1: ldr r1, [r2]           @ Load USART status register
   ands r1, #TXE          @ Transmit buffer empty?
   beq 1b                 @ loop until buffer is empty

   ldr r2, =USART2_DR
   strb r0, [r2]          @ Output the character

   @ An den Leuchtdioden ausgeben:
@   ldr r2, = GPIOD_ODR    @  point to Port D output data register
@   lsls r0, #12
@   str r0, [r2]


   pop {r0, r1, r2}
   bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "key"
key: @ ( -- c ) Empfängt Wert in r0
@ -----------------------------------------------------------------------------
   push {r0, r1, r2}

   ldr r2, =USART2_SR
        
1: ldr r1, [r2]           @ Load USART status register
   ands r1, #RXNE        @ Receive buffer not empty ?
   beq 1b                 @ Loop until a character arrives

   ldr r2, =USART2_DR
   ldrb r0, [r2]          @ Fetch the character

   pushda r0
   pop {r0, r1, r2}
   bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "?key"
  @ ( -- ? ) Ist eine Taste gedrückt ?
@ -----------------------------------------------------------------------------
   ldr r0, =USART2_SR
   ldr r1, [r0]     @ Fetch status
   ands r1, #RXNE
   beq 1f
     pushdaconst -1
     bx lr

1: pushdaconst 0
   bx lr


  .ltorg @ Hier werden viele spezielle Hardwarestellenkonstanten gebraucht, schreibe sie gleich !
