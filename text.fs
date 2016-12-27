\ High-Level Display interface: Fonts & Images
\ (c)copyright 2014 by Gerald Wodni

\ raw \ clear all until ws2812



compiletoflash

$000400 variable text-color
\ TODO: cvariable would be sufficient
cols variable max-column 	\ stop printing at this column
  0 variable cur-column 		\ current column
  0 variable col-offset 		\ column offset
  1 variable boldness 		\ times to repeat pattern
8px-cond variable font

0 variable cur-text

\ font-sizes, usage: "regular wide
: wide 3 boldness ! ;
: bold 2 boldness ! ;
: regular 1 boldness ! ;

\ get start-address of char
: c-pos ( u-char -- )
	font @ swap $7F and $20 - 0 ?do dup c@ 1+ + loop ;

: d-column drop ;

: offset-column ( n-offset n-column )
	dup d-column cur-column ! col-offset ! ;
: column ( n-column -- )
	0 swap offset-column ;

\ expand lower nibble across byte
: stretch ( x -- x )
	0 4 0 do
		2 lshift
	over $08 and
	if
		$03 or
	then
	swap 1 lshift swap
	loop nip ;

: >d 
	led-buffer cur-column @ 4 * +
	8 0 do
		\ i . 2dup swap hex. hex. cr
		over $01 and if 		\ bit lit?
			text-color @ over !
		then

		row-size +
		swap 1 rshift swap 	\ next bit
	loop 2drop ;

\ emit single byte and respect column-max
: d-emit-max ( x-char -- )
	cur-column @ max-column @ < if \ don't print after max-column
		>d
	 	1 cur-column +!
	else
		drop
	then ;

\ emit single byte and respect column-offset
: d-emit-off ( x-char -- )
	col-offset @ 0= if 	\ don't print negative offsets
		d-emit-max
	else
		-1 col-offset +!
		drop
	then ;

\ emit pattern n-times
: d-emit-n ( x-char n-count -- )
	boldness @ 0 do dup d-emit-off loop drop ;

: d-emit-byte ( x-char -- )
	d-emit-n ;

\ emit single char
: d-emit ( u-char -- )
	c-pos str-bounds do
		i c@ d-emit-byte
	loop
	$00 d-emit-byte
	;


\ : d-emit dup ." EMIT:  " emit cr d-emit ;

: d-type ( c-addr u -- )
	\ str-bounds ?do i c@ d-emit loop ;
	bounds ?do i c@ d-emit loop ;

: d-length ( c-addr -- n-len )
	\ 0 swap str-bounds
	0 -rot bounds
	?do
		i c@ c-pos c@ +
		1+
	loop
	dup 0 > if
		1-
	then boldness @ * ;

: d( [char] ) parse d-type flush immediate ;

: d" postpone s" postpone d-type immediate ;

: clear
	buffer-off
	off
	0 cur-column !
	;

: bussi 
	init-ws
	clear
	buffer-wave
	$3F0000 text-color !  [char] B d-emit
	$3F3F00 text-color !  [char] u d-emit
	$003F00 text-color !  [char] s d-emit
	$003F3F text-color !  [char] s d-emit
	$00004F text-color !  [char] i d-emit
	$3F3F3F text-color !  d" ;)"
	flush
	;

: >scroll ( c-addr -- )
	2dup d-length 1+ 0 do
		i 0 offset-column
		clear
		2dup d-type flush
		100 ms
	loop 2drop ;

: scroll( [char] ) parse >scroll immediate ;

: test-text
	s"    Hi there can anybody read this?" >scroll
	;
