eraseflash


compiletoflash

\ Ports

$20 constant P1IN
$21 constant P1OUT
$22 constant P1DIR
$23 constant P1IFG
$24 constant P1IES
$25 constant P1IE
$26 constant P1SEL
$27 constant P1REN
$41 constant P1SEL2

$28 constant P2IN
$29 constant P2OUT
$2A constant P2DIR
$2B constant P2IFG
$2C constant P2IES
$2D constant P2IE
$2E constant P2SEL
$2F constant P2REN
$42 constant P2SEL2

\ SPI registers
$60 constant UCA0CTL0
$61 constant UCA0CTL1
$62 constant UCA0BR0
$63 constant UCA0BR1
$64 constant UCA0MCTL
$65 constant UCA0STAT
$66 constant UCA0RXBUF
$67 constant UCA0TXBUF

$68 constant UCB0CTL0
$69 constant UCB0CTL1
$6A constant UCB0BR0
$6B constant UCB0BR1
$6D constant UCB0STAT
$6E constant UCB0RXBUF
$6F constant UCB0TXBUF
$01 constant IE2
$03 constant IFG2

$4A constant ADC10AE0 \ If you want to use Pins of Port 1 as analog inputs, you have to set the corresponding bits in ADC10AE0 !

: init \ Launchpad hardware initialisations
  8 p1out cbis! \ High
  8 p1ren cbis! \ Pullup for button

  1 64 or p1out cbic! \ LEDs off
  1 64 or p1dir cbis! \ LEDs are outputs
;

\ Measure Vcc/2 on analog channel 11 with 2.5V reference.
: vcc. 0  11 analog  204,6 f/ 2 f.n ." V " ;

\ Measure temperature on analog channel 10.
: temp.
  0
  0.  200 0 do 10 adc-1.5 0 d+ loop 200 um/mod nip  \ Average 200 measurements with 1.5V reference because of strong noise.
  0,41263 f* 280,28 d- 2 f.n ." °C "                 \ Calculate uncalibrated temperature...
;

\ Busy waits for the given time or slightly more, if interrupts are active.
\ DCO clock is only accurate to +-3% and varies with Vcc and temperature.
\ For precise timings, connect a crystal and use timer.
\ 8 cycles per loop run for 1 us @ 8 MHz.

: us 0 ?do [ $3C00 , $3C00 , ] loop ;
: ms 0 ?do 998 us loop ;


: pwm-init ( Interval -- ) \ Init PWM on green LED
  64 p1dir cbis! \ Output
  64 p1sel cbis! \ Timer special function

  \ Configure Timer-A

       $172 ! \ PWM Interval in cycles
  $E0  $164 ! \ CCR1 Set/Reset
  0    $174 ! \ CCR1 PWM Duty Cycle
  $210 $160 ! \ SMCLK/1, upmode
;

: pwm ( Duty-Cycle -- ) $174 ! ; \ Sets duty cycle


: random ( -- u )
 ( Generiert Zufallszahlen mit dem Rauschen vom Temperatursensor am ADC )
 ( Random numbers with noise of temperature sensor on ADC )
   0
   16 0 do
     shl
    10 analog 1 and
    xor
  loop
;

: cornerstone ( Name ) ( -- )
  <builds begin here $1FF and while 0 , repeat
  does>   begin dup  $1FF and while 1+  repeat eraseflashfrom
;

: bounds ( addr n -- addr-end addr-start )
    over + swap ;

: u.4 ( u -- ) 0 <# # # # # #> type ;
: u.2 ( u -- ) 0 <# # # #> type ;

$10F6 constant TAG_DCO_30
$0007 constant CAL_BC1_8MHZ
$0006 constant CAL_DCO_8MHZ
$0003 constant CAL_BC1_16MHZ
$0002 constant CAL_DCO_16MHZ
$0056 constant DCOCTL
$0057 constant BCSCTL1
$0058 constant BCSCTL2
: 8mhz ( -- )
    TAG_DCO_30 dup CAL_DCO_8MHZ  + c@ DCOCTL  c!
                   CAL_BC1_8MHZ  + c@ BCSCTL1 c! ;
                   $02 BCSCTL2 cbic!  ;          \ reset SMCLK divider to 1
\ TODO: ajust uart speeds, or select different uart clock
: 16mhz ( -- )
    TAG_DCO_30 dup CAL_DCO_16MHZ + c@ DCOCTL  c! \ set calibrated DCO
                   CAL_BC1_16MHZ + c@ BCSCTL1 c! \ set calibrated BC1
                   $02 BCSCTL2 cbis!  ;          \ set SMCLK divider to 2

\ multiply by clock div
: clk-div* ( n1 -- n2 )
    BCSCTL2 c@ $02 and if 2* then ;
\ update us and ms to be multi-frequency aware
: us clk-div* us ;
: ms clk-div* ms ;

cornerstone Rewind-to-Basis
