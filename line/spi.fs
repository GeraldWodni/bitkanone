rewind-to-basis


compiletoflash

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
		$8 			\ push "0" ($8) on stack
		over $80 and 0<> 	\ check msb
		$E and or 		\ msb=1, push "1" ($E) on stack
		>spi
		1 lshift
	loop drop ;

init-spi
$00 >ws
