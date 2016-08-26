\ eraseflash

compiletoflash

\ unwanted definitions are commented out to save precious flash for animations
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
$68 constant UCB0CTL0
$69 constant UCB0CTL1
$6A constant UCB0BR0
$6B constant UCB0BR1
$6D constant UCB0STAT
$6E constant UCB0RXBUF
$6F constant UCB0TXBUF
$01 constant IE2
$03 constant IFG2

\ clock control
$0056 constant DCOCTL
$0057 constant BCSCTL1
$0058 constant BCSCTL2

\ calibration registers
$10F6 constant TAG_DCO_30
$0003 constant CAL_BC1_16MHZ
$0002 constant CAL_DCO_16MHZ

\ button
: init ( -- )
    8 P1REN cbis! ; \ Pullup for button
0 variable btn-last
\ true when button is down
: pressed? ( -- f ) 8 P1IN c@ and 0= ;
\ true once when button is pressed
: button? ( -- f )
    btn-last c@ 0=
    pressed?        \ get current state
    dup btn-last c! \ store current state
    and ;           \ compare states

: cornerstone ( Name ) ( -- )
  <builds begin here $1FF and while 0 , repeat
  does>   begin dup  $1FF and while 1+  repeat eraseflashfrom
;

: bounds ( addr n -- addr-end addr-start )
    over + swap ;

\ helpers for handling 3 items on stack
: 3dup >r 2dup r@ -rot r> ;
: 3drop 2drop drop ;

: u.4 ( u -- ) 0 <# # # # # #> type ;
: u.2 ( u -- ) 0 <# # # #> type ;

: 16mhz ( -- )
    TAG_DCO_30 dup CAL_DCO_16MHZ + c@ DCOCTL  c! \ set calibrated DCO
                   CAL_BC1_16MHZ + c@ BCSCTL1 c! \ set calibrated BC1
                   $02 BCSCTL2 cbis!  ;          \ set SMCLK divider to 2

\ multiply by clock div
: clk-div* ( n1 -- n2 )
    BCSCTL2 c@ $02 and if 2* then ;

\ us and ms which are multi-frequency aware
: us clk-div* 0 ?do [ $3C00 , $3C00 , ] loop ;
: ms clk-div* 0 ?do 998 us loop ;

: noop ;
