rewind-to-basis


compiletoflash

4 constant #leds
#leds 3 * constant #buffer
#buffer buffer: buffer

: buffer-bounds ( -- c-addr-end c-addr-start )
    buffer #buffer bounds ;

\ write rgb to c-addr and 2 following addresses in GRB-order
: rgb! ( d-rgb c-addr -- )
    >r r@ 1+ c!         \ red
    dup 8 rshift r@ c!  \ green
    $FF and r> 2+ c! ; \ blue

\ write n-th pixel in buffer
: rgb-px! ( d-rgb index -- )
    3 * buffer + rgb! ;

\ fill buffer with color
: buffer! ( d-rgb -- )
    buffer-bounds do
        2dup
        i rgb!
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
    \ set clock to 16mhz
    16mhz

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

\ write byte (unrolled)
: >ws ( x -- )
        dup $80 and if $E else $8 then >spi
        dup $40 and if $E else $8 then >spi
        dup $20 and if $E else $8 then >spi
        dup $10 and if $E else $8 then >spi

        dup $08 and if $E else $8 then >spi
        dup $04 and if $E else $8 then >spi
        dup $02 and if $E else $8 then >spi
        dup $01 and if $E else $8 then >spi drop ;

: flush ( -- )
    buffer #buffer bounds do
        i c@ >ws
    loop ;

: f flush ;

\ interaction words

\ fill buffer and flush
: leds ( d-rgb -- )
    buffer! flush ;

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

16mhz
init-spi
r

cornerstone cold
