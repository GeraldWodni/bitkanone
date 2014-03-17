\ High-Level Display interface: Fonts & Images
\ (c)copyright 2014 by Gerald Wodni

rewind-r



compiletoflash

\ font-types
1 constant regular
2 constant bold
3 constant wide

\ TODO: cvariable would be sufficient
$000F00 variable text-color
132 variable max-column 	\ stop printing at this column
  0 variable cur-column 		\ current column
  0 variable col-offset 		\ column offset
  1 variable boldness 		\ times to repeat pattern
7px variable font

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
		i . 2dup swap hex. hex. cr
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

: str-bounds dup c@ bounds 1+ swap 1+ swap ;

\ emit single char
: d-emit ( u-char -- )
	c-pos str-bounds do
		i c@ d-emit-byte
	loop
	$00 d-emit-byte
	;


: d-type ( c-addr u -- )
	str-bounds ?do i c@ d-emit loop ;

: d( [char] ) parse d-type immediate ;

: d" postpone s" postpone d-type immediate ;

: clear
	buffer-off
	off
	0 cur-column !
	;

: test 
	\ bold d" Forth" 
	rinit
	cr
	clear
	d" Claudia"
	rflush
	;

test
