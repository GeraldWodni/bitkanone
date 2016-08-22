rewind-to-basis


compiletoflash

1 constant #leds
#leds 3 * constant #buffer
#buffer buffer: buffer

$00 buffer c!
$FF buffer 1+ c!
$55 buffer 2+ c!

: buffer-bounds ( -- c-addr-end c-addr-start )
    buffer #buffer bounds ;

\ write rgb to c-addr and 2 following addresses in GRB-order
: rgb! ( d-rgb c-addr -- )
    >r r@ 1+ c!         \ red
    dup 8 rshift r@ c!  \ green
    $FF and r> 2+ c! ; \ blue

\ write n-th pixel in buffer
: rgb-px! ( d-rgb index -- )
    buffer + rgb! ;

\ fill buffer with color
: buffer! ( d-rgb -- )
    buffer-bounds do
        2dup
        i rgb!
        ." w" i .
    3 +loop 2drop ;

: buffer. ( -- )
    base @ hex
    buffer-bounds do
        i c@ u.2
        i 1+ c@ u.2
        i 2+ c@ u.2
        cr
    3 +loop base ! ;

\ initialize UCSI_B0 as SPI Master
: init-spi ( -- )
    \ 1. Set UCSWRST
    1 UCB0CTL1 c!

    \ 2. Initialize all USCI registers with UCSWRST=1 (including UCxCTL1)
    $28 UCB0CTL0 c!     \ MSB first
    $C0 UCB0CTL1 cbis!  \ SMCLK Source

    0 UCB0BR1 c! \ clock: 1 cycle
    2 UCB0BR0 c! \ clock: 1 cycle

    \ 3. Configure ports
    $80 P1SEL  cbis! \ P1.7 = SIMO
    $80 P1SEL2 cbis!
    $80 P1DIR  cbis!

    \ 4. Clear UCSWRST via software
    1 UCB0CTL1 cbic!
    ;

: >spi ( x -- ) inline
    UCB0TXBUF c! ;

: >ws ( x -- )
	8 0 do
            dup $80 and if
                $E UCB0TXBUF c!
            else
                $8 UCB0TXBUF c!
            then
            2*
	loop drop ;

: >wsi ( x -- )
        dup $80 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then
        dup $40 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then
        dup $20 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then
        dup $10 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then

        dup $08 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then
        dup $04 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then
        dup $02 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then
        dup $01 and if $E UCB0TXBUF c!  else $8 UCB0TXBUF c!  then drop ;

: flush ( -- )
    buffer #buffer bounds do
        i c@ >ws
    loop ;

: flushi ( -- )
    buffer #buffer bounds do
        i c@ >wsi
    loop ;

\ interaction words

\ fill buffer and flush
: leds ( d-rgb -- )
    buffer! flushi ;

\ make all leds red
: r ( -- ) $FF.00.00 leds ;

\ make all leds green
: g ( -- ) $00.FF.00 leds ;

\ make all leds blue
: b ( -- ) $00.00.FF leds ;

\ make all leds white
: on ( -- ) $FF.FF.FF leds ;

\ make all leds black
: off ( -- ) $00.00.00 leds ;


init-spi
flush

cornerstone cold
