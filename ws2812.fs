\ WS2812 Driver for lm4f120-MECRISP
\ (c)copyright 2014 by Gerald Wodni <gerald.wodni@gmail.com>

: init-ws-spi
    SSI_CR0_SPO SSI_CR0_SPH or
    4 2 init-ssi                ( Initialize SSI2 with 8 data bits as master )
    2 2 2 ssi-speed             ( Slow Speed, 16MHz )
    $F0 PORTB_AFSEL !           ( Associate Upper Nibble to Alternate Hardware )
    $22220000  PORTB_PCTL !     ( Specify SSI2 as Alternate Function )
    $F0 PORTB_DEN !             ( Enable Digital operation for SSI2 )
    $90 PORTB_DIR !             ( Configure TX and CLK as Output )
    2 enable-ssi              ( Setup complete, enable SSI2 )
    ;

: >ws ( x -- )
	ssi-wait-tx-fifo  		\ make sure fifo is empty
	8 0 do
		$8 			\ push "0" ($8) on stack
		over $80 and 0<> 	\ check msb
		$E and or 		\ msb=1, push "1" ($E) on stack
		>ssi
		1 lshift
	loop drop ;

: >rgb ( -- )
	dup 8  rshift >ws 	\ green
	dup 16 rshift >ws 	\ red
	>ws ; 			\ blue

: n-leds ( x-color n-leds -- )
	0 do
		dup >rgb
	loop drop ;

240 constant leds

: off     $000000 leds n-leds ;
: red	  $010000 leds n-leds ;
: yellow  $010100 leds n-leds ;
: green   $000100 leds n-leds ;
: cyan	  $000101 leds n-leds ;
: blue	  $000001 leds n-leds ;
: magenta $010001 leds n-leds ;
: white	  $010101 leds n-leds ;

: bill leds 0 do
		$FF0000 >rgb
		$FFFF00 >rgb
		$00FF00 >rgb
		$0000FF >rgb
		$FFFFFF >rgb
	loop ;

30 constant cols
8 constant rows
cols rows * constant leds
leds 4 * constant led-buffer-size
cols 4 * constant row-size
led-buffer-size 
compiletoram
here swap allot
compiletoflash
constant led-buffer

: buffer-white
        led-buffer led-buffer-size bounds do
                $010101 i !
        4 +loop ;

\ wave-like pattern
: buffer-wave
        led-buffer led-buffer-size bounds do
                i 2 rshift $F and i !
        4 +loop ;

: buffer-blue
	led-buffer led-buffer-size bounds do
		$000001 i !
	4 +loop ;

: buffer-off
	led-buffer led-buffer-size bounds do
		$0 i !
	4 +loop ;

: led-n ( n-index -- a-addr ) inline
	4 * led-buffer + ;

: led-n! ( x-color n-index -- ) inline
	led-n ! ;

: line 
	8 0 do
		$0F0000 i 30 * i + led-n!
	loop ;

: runner
	led-buffer led-buffer-size 4 -
	bounds
	over over @ >r >r
	do
		i 4 + @
		i !
	4 +loop 
	r> r> swap !
	xflush
	;

\ flush in one sequence
: flush cr
	led-buffer led-buffer-size bounds do
		i @ >rgb
	4 +loop ; 

: row-bounds ( n -- )
	row-size *      \ row offset
	led-buffer +    \ total offset
	row-size bounds ;

\ write line
: row>rgb ( a-addr-start a-addr-end -- )
	do
		i @ >rgb
	4 +loop ;

\ write reversed
: -row>rgb ( a-addr-start a-addr-end -- )
	swap
	4 -
	do
		i @ >rgb
	-4 +loop ;

\ flush in one zig-zag-sequence, takes about 10ms
: z-flush ( -- )
	0 7 do
		i row-bounds
		i 1 and if
			-row>rgb
		else
			row>rgb
		then
	-1 +loop
	;

: init-ws
	init-ws-spi
	off
	buffer-wave 		\ draw wave background
	line 			\ draw red diagonal line
	$000F00 0 led-n! 	\ make first pixel green
	z-flush ;

init-ws
