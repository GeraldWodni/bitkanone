\ High-Level Display interface: Fonts & Images
\ (c)copyright 2014 by Gerald Wodni

cold \ clear all until ws2812


compiletoflash

$000F00 variable text-color
\ TODO: cvariable would be sufficient
132 variable max-column 	\ stop printing at this column
  0 variable cur-column 		\ current column
  0 variable col-offset 		\ column offset
  1 variable boldness 		\ times to repeat pattern
8px variable font

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


: d-type ( c-addr u -- )
	str-bounds ?do i c@ d-emit loop ;

: d( [char] ) parse d-type z-flush immediate ;

: d" postpone s" postpone d-type immediate ;

: clear
	buffer-off
	off
	0 cur-column !
	;

: test 
	init-ws
	clear
	buffer-wave
	$3F0000 text-color !
	d" B"
	$3F3F00 text-color !
	d" u"
	$003F00 text-color !
	d" s"
	$003F3F text-color !
	d" s"
	$00004F text-color !
	d" i"
	$3F3F3F text-color !
	d" ;"
	[char] ) d-emit
	z-flush
	;

test
