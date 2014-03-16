rewind-ssi



compiletoflash

: init-ws
    SSI_CR0_SPO SSI_CR0_SPH or
    4 2 init-ssi                ( Initialize SSI2 with 8 data bits as master )
    2 2 2 ssi-speed             ( Slow Speed, 16MHz )
    \ 2 4 2 ssi-speed             ( Slow Speed, 80MHz )
    $F0 PORTB_AFSEL !           ( Associate Upper Nibble to Alternate Hardware )
    $22220000  PORTB_PCTL !     ( Specify SSI2 as Alternate Function )
    $F0 PORTB_DEN !             ( Enable Digital operation for SSI2 )
    $90 PORTB_DIR !             ( Configure TX and CLK as Output )
    2 enable-ssi              ( Setup complete, enable SSI2 )
    ;

: >ws ( x -- )
	ssi-wait-tx-fifo
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

: >red ( -- )
	$FF0000 >rgb ;

: >green ( -- )
	$00FF00 >rgb ;

: >blue ( -- )
	$0000FF >rgb ;

: n-leds ( x-color n-leds -- )
	0 do
		dup >rgb
	loop drop ;

: off 0 240 n-leds ;
: red	  $010000 240 n-leds ;
: yellow  $010100 240 n-leds ;
: green   $000100 240 n-leds ;
: cyan	  $000101 240 n-leds ;
: blue	  $000001 240 n-leds ;
: magenta $010001 240 n-leds ;
: white	  $010101 240 n-leds ;

: bill 240 0 do
		$FF0000 >rgb
		$FFFF00 >rgb
		$00FF00 >rgb
		$0000FF >rgb
		$FFFFFF >rgb
	loop ;

: x 239 0 do
		i 16 lshift
		255 i - or
		>rgb
	loop ; 

240 constant leds
leds 4 * constant led-buffer-size
led-buffer-size 
compiletoram
here swap allot
compiletoflash
constant led-buffer

: buffer-white
        led-buffer led-buffer-size bounds do
                $010101 i !
		i .
        4 +loop ;

: buffer-wh
        led-buffer led-buffer-size bounds do
                i 2 rshift $F and i !
		i .
        4 +loop ;

: buffer-blue
	led-buffer led-buffer-size bounds do
		$000001 i !
	4 +loop ;

: buffer-off
	led-buffer led-buffer-size bounds do
		$0 i !
	4 +loop ;

: buffer.
	led-buffer led-buffer-size bounds do
		i . ." : " i @ . cr
	4 +loop ;

: flush cr
	led-buffer led-buffer-size bounds do
		i @ >rgb
	4 +loop ; 

: fflush
	led-buffer
	leds 0 do
		dup @ >rgb
		4 +
	loop drop ;

: xflush
	led-buffer led-buffer-size +
	leds 0 do
		4 -
		dup @ >rgb
	loop drop ;

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

: xr
	begin runner again ;

: x $1F2F3F 1 n-leds ;
: y $9F2F3F 1 n-leds ;
	
: initi
	init-ws
	yellow
	buffer-wh
	line
	$000F00 0 led-n!
	xflush 
;

initi


compiletoram
