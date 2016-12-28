\ WS2812 Driver for lm4f120-MECRISP
\ (c)copyright 2014 by Gerald Wodni <gerald.wodni@gmail.com>


\ rewind-ssi \ clear until ws2812


compiletoflash


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

: on      $FFFFFF leds n-leds ;
: off     $000000 leds n-leds ;
: red	  $1F0000 leds n-leds ;
: yellow  $1F1F00 leds n-leds ;
: green   $001F00 leds n-leds ;
: cyan	  $001F1F leds n-leds ;
: blue	  $00001F leds n-leds ;
: magenta $1F001F leds n-leds ;
: white	  $1F1F1F leds n-leds ;

: latch 50 us ;
: flash on latch off ;

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
row-size 2/ constant row-size/2
led-buffer-size buffer: led-buffer

\ wave-like pattern
: buffer-wave
        led-buffer led-buffer-size bounds do
                i 2 rshift $F and i !
        4 +loop ;

: buffer! ( x-color -- )
	led-buffer led-buffer-size bounds do
		dup i !
	4 +loop drop ;

: buffer-off
	led-buffer led-buffer-size bounds do
		$0 i !
	4 +loop ;

: led-n ( n-index -- a-addr ) inline
	4 * led-buffer + ;

: led-n! ( x-color n-index -- ) inline
	led-n ! ;

: led-xy ( n-x n-y -- index ) inline
	cols * + ;

: xy! ( x-color n-x n-y -- )
	over 0 cols between
	over 0 rows between
	and if			\ only draw in buffer region
		led-xy led-n!
	else
		2drop
	then ;

: xy@ ( n-x n-y -- x-color )
	led-xy led-n @ ;

: init-line 
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
	;


\ linear flush in one sequence
: l-flush cr
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

\ flush in one long zig-zag-sequence, takes about 10ms
: lz-flush ( -- )
	0 7 do
		i row-bounds
		i 1 and if
			-row>rgb
		else
			row>rgb
		then
	-1 +loop
	;

: r-row-bounds ( n -- )
	row-size *      \ row offset
	led-buffer +    \ total offset
	row-size/2 bounds ;

: l-row-bounds ( n -- )
	row-size *      \ row offset
	led-buffer +    \ total offset
	row-size/2 +	\ add half row 
	row-size/2 bounds ;

\ flush 2 zig-zags
: z-flush ( -- )
	0 7 do
		i r-row-bounds
		i 1 and if
			row>rgb
		else
			-row>rgb
		then
	-1 +loop
	0 7 do
		i l-row-bounds
		i 1 and if
			row>rgb
		else
			-row>rgb
		then
	-1 +loop
	;

\ mirror-flush 2 zig-zags
: mz-flush ( -- )
	0 7 do
		i l-row-bounds
		i 1 and if
			-row>rgb
		else
			row>rgb
		then
	-1 +loop
	0 7 do
		i r-row-bounds
		i 1 and if
			-row>rgb
		else
			row>rgb
		then
	-1 +loop
	;

' z-flush variable flush-target

: flush flush-target @ execute ;

: init-ws
	init-ws-spi
	off
	buffer-wave 		\ draw wave background
	init-line 	        \ draw red diagonal line
	$000F00 0 led-n! 	\ make first pixel green
	flush ;

: init init init-delay init-ws ;

init-ws
