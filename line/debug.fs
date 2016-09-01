\ Debug helpers
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com> 

\ some usefull words which just don't fit into the flash,
\ and are only needed for debugging anyways.

\ UART registers
\ $60 constant UCA0CTL0
\ $61 constant UCA0CTL1
\ $62 constant UCA0BR0
\ $63 constant UCA0BR1
\ $64 constant UCA0MCTL
\ $65 constant UCA0STAT
\ $66 constant UCA0RXBUF
\ $67 constant UCA0TXBUF

$6D constant UCB0STAT
$01 constant IE2
$03 constant IFG2

\ port registers
$21 constant P1OUT
$23 constant P1IFG
$24 constant P1IES
$25 constant P1IE

$2F constant P2REN
$28 constant P2IN
$29 constant P2OUT
$2A constant P2DIR
$2B constant P2IFG
$2C constant P2IES
$2D constant P2IE
$2E constant P2SEL
$42 constant P2SEL2


$0007 constant CAL_BC1_8MHZ
$0006 constant CAL_DCO_8MHZ

\ set speed to 8mhz (only opposite is needed - speed to 16mhz)
: 8mhz ( -- )
    TAG_DCO_30 dup CAL_DCO_8MHZ  + c@ DCOCTL  c!
                   CAL_BC1_8MHZ  + c@ BCSCTL1 c!
                   $02 BCSCTL2 cbic!  ;          \ reset SMCLK divider to 1

\ convert double-rgb into components
: rgb>crgb ( d-rgb -- r g b )
    swap \ red is already isolated
    >r r@ 8 rshift  \ shift green down
    r> $FF and ;    \ mash blue

\ print buffer in lines with prefixed index
: buffer. ( -- )
    cr ." ##: RRGGBB"
    base @ hex 0
    buffer-bounds do
        \ show index
        cr dup u.2 ." : " 1+
        i 1+ c@ u.2
        i c@ u.2
        i 2+ c@ u.2
    3 +loop drop base !  ;

\ limit rgb value to a component sum below 256
\ remark: does not work due to non-linear brightness of leds
: limit ( n-r1 n-g1 n-b1 -- n-r2 n-g2 n-b2 )
    3dup + + \ get sum
    >r rot 255 r@ */ -rot
    swap 255 r@ */ swap
    255 r> */ ;
